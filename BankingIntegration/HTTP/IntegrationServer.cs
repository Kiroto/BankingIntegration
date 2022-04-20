using BankingIntegration.BankModel;
using BankingIntegration.HTTP;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BankingIntegration
{
    class IntegrationServer : HttpServer
    {
        private static readonly HttpClient httpClient = new HttpClient();
        public static bool coreIsOnline = false;
        public static Uri coreUri = new Uri("http://localhost:8082");
        public static int defaultRequestTimeout = 1000;
        public static int sessionTimeoutMin = 15;

        private static dynamic? MakeCoreRequest(string path, string contents)
        {
            UriBuilder builder = new UriBuilder(coreUri);
            builder.Path = path;
            Task<HttpResponseMessage> TResponseMessage = httpClient.PostAsync(builder.Uri, new StringContent(contents));
            if (!TResponseMessage.Wait(defaultRequestTimeout)) throw new CoreTimeoutException();
            Task<string> TResultString = TResponseMessage.Result.Content.ReadAsStringAsync();
            if (!TResultString.Wait(defaultRequestTimeout)) throw new CoreTimeoutException();
            return JsonSerializer.Deserialize<dynamic>(TResultString.Result);
        }

        public static List<UserSession> userSessions = new List<UserSession>();
        public static List<QueuedRequest> queuedRequests = new List<QueuedRequest>();

        public static readonly ProcessedResponse coreOfflineResponse = new ProcessedResponse() { StatusCode = 503, Contents = MakeErrorMessage("The core is offline at the moment and cannot process this request.", ErrorCode.CORE_OFFLINE) };
        public static readonly ProcessedResponse invalidCredentialsResponse = new ProcessedResponse() { StatusCode = 400, Contents = MakeErrorMessage("The Credentials given are not valid.", ErrorCode.CREDENTIALS_INVALID) };
        public static readonly ProcessedResponse invalidSessionResponse = new ProcessedResponse() { StatusCode = 403, Contents = MakeErrorMessage("The received session key is not valid.", ErrorCode.CREDENTIALS_INVALID) };

        public delegate void CoreIsUp();
        public static event CoreIsUp OnCoreUp;

        public IntegrationServer(int preferredPort = 8081) : base(preferredPort)
        {
            OnCoreUp += DoQueuedRequests;

            Route pingRequest = new Route("/v1/ping");
            pingRequest.DoGet = (body) =>
            {
                ProcessedResponse response = new ProcessedResponse();
                response.Contents = "Home has been hit!";
                return response;
            };
            handledRoutes.Add(pingRequest);

            Route loginRequest = new Route("/v1/login");
            loginRequest.DoPost = (reqBody) =>
            {
                // Confirm credentials
                UserLoginRequest ulr = JsonSerializer.Deserialize<UserLoginRequest>(reqBody);
                int userIdRequest = CoreGetUserId(ulr);

                // Create the session
                int userId = (int)userIdRequest;

                UserSession returnedSession = GetUserSession(userId); // Return an already existing session by default
                if (IsUserSessionValid(returnedSession))
                {
                    RefreshUserSession(returnedSession);
                }
                else
                {
                    DateTime currentTime = DateTime.Now;
                    returnedSession = new UserSession()
                    {
                        UserID = userId,
                        SessionToken = GenerateSessionToken(ulr),
                        SessionStart = currentTime,
                        LastRequest = currentTime,
                        Service = ulr.ServiceId
                    };
                    AddUserSession(returnedSession);
                }
                returnedSession.StatusCode = 200;
                return returnedSession;
            };
            handledRoutes.Add(loginRequest);

            Route createClientRequest = new Route("/v1/createClient");
            createClientRequest.DoPost = (reqBody) =>
            {
                // Confirm user session
                ClientCreationRequest ccr = JsonSerializer.Deserialize<ClientCreationRequest>(reqBody);
                UserSession userSession = GetUserSession(ccr.SessionToken);

                ProcessedResponse response = new ProcessedResponse();
                if (CreateNewClient(ccr, userSession.UserID) != -1) response.StatusCode = 200;
                else response.StatusCode = 503;
                
                return response;

            };
            handledRoutes.Add(createClientRequest);

            Route getClientRequest = new Route("/v1/getClient");
            getClientRequest.DoPost = (reqBody) =>
            {
                ClientInfoRequest cir = JsonSerializer.Deserialize<ClientInfoRequest>(reqBody);
                UserSession us = GetUserSession(cir.SessionToken);

                return GetBankClient(cir, us.UserID);
            };
            handledRoutes.Add(getClientRequest);

            // Route getClientInfo
            // Route updateClientInfo
            // Route removeClient

            // TODO:
            // Route createAdminUser
            // Route updatePassword
            // Route updateUser

            // Route createAccount
            // Route withdrawFromAccount
            // Route removeAccount
            // Route getAccountsByClient

            // Route createLoan
            // Route payLoan
            // Route removeLoan
            // Route getLoansByClient

            // Route addBeneficiario
            // Route updateBeneficiario
            // Route getBeneficiariosByClient
            // Route removeBeneficiario

            // Route createTransaction
            // Route getTransactionHistoryByAccount            
        }

        // Encryption Functions
        private static string MakeErrorMessage(string message, ErrorCode code)
        {
            ErrorMesage em = new ErrorMesage();
            em.Code = code;
            em.ErrorMessage = message;
            return em.AsJsonString();
        }
        private static string sha256_hash(string value)
        {
            StringBuilder Sb = new StringBuilder();

            using (var hash = SHA256.Create())
            {
                Encoding enc = Encoding.UTF8;
                byte[] result = hash.ComputeHash(enc.GetBytes(value));

                foreach (byte b in result)
                    Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString();
        }
        private string GenerateSessionToken(UserLoginRequest ulr)
        {
            return sha256_hash(ulr.Username + Convert.ToString(ulr.ServiceId) + DateTime.Now);
        }

        // Session Functions
        private static UserSession GetUserSession(string sessionToken) {
            UserSession us = userSessions.Find((us) => {
                return us.SessionToken == sessionToken;
            });
            if (us == null) throw new NoSuchUserSessionException(sessionToken);
            return us;
        }
        private static UserSession? GetUserSession(int userId)
        {
            UserSession us = userSessions.Find((us) => {
                return us.UserID == userId;
            });
            if (us == null) throw new NoSuchUserSessionException($"UserId: {userId}");
            return us;

        }
        private static bool IsUserSessionValid(string sessionToken)
        {
            try
            {
                UserSession us = GetUserSession(sessionToken);
                return IsUserSessionValid(us);

            }
            catch
            {
                return false;
            }
        }
        private static bool IsUserSessionValid(UserSession us)
        {
            return us != null && us.LastRequest < DateTime.Now.AddMinutes(sessionTimeoutMin);
        }
        private bool RefreshUserSession(UserSession us)
        {
            if (IsUserSessionValid(us))
            {
                us.Refresh();
                return true;
            }
            return false;
        }
        private bool DeleteUserSession(UserSession us)
        {
            return userSessions.Remove(us);
        }
        private void AddUserSession(UserSession us)
        {
            // We only allow one session per user
            UserSession? oldUserSession = GetUserSession(us.UserID);
            if (IsUserSessionValid(oldUserSession)) // If a session for that user is still valid, just refresh it
            {
                RefreshUserSession(oldUserSession);
            }
            else
            {
                while (oldUserSession != null) // Remove all other old sessions for that user
                {
                    DeleteUserSession(oldUserSession);
                    oldUserSession = GetUserSession(us.UserID);
                }
                userSessions.Add(us); // Then add the new one
            }
        }

        // <> Core Functions <>
        private static bool IsCoreOnline()
        {
            try
            {
                MakeCoreRequest("ping", "");
                OnCoreUp();
                return true;
            } catch
            {
                return false;
            }
        }
        private void DoQueuedRequests()
        {
            foreach (QueuedRequest qr in queuedRequests)
            {
                Route? route = GetCorrespondingRoute(qr.Path);
                ProcessedResponse pr = route.Handle(qr.Contents, qr.Method);
                qr.Tried = true;
            }
            queuedRequests.RemoveAll((qr) =>
            {
                return qr.Tried == true;
            });
        }

        // Authentication Functions
        private int CoreGetUserId(UserLoginRequest ulr)
        {
            string resultString = MakeCoreRequest("/v1/login", ulr.AsJsonString());
            dynamic data = JsonSerializer.Deserialize<dynamic>(resultString);
            ClientInfoAttempt cia = data;
            return data.UserId;
        }

        // Client Functions
        private int? CreateNewClient(ClientCreationRequest ccr, int userId)
        {
            try { 
                ClientCreationAttempt cca = new ClientCreationAttempt(ccr, userId);
          
                dynamic data = MakeCoreRequest("/v1/createClient", cca.AsJsonString());
                return data.UserId;
            } catch (CoreTimeoutException e)
            {
                queuedRequests.Add(new QueuedRequest());
                return -1;
            }
        }
        private BankClient? GetBankClient(ClientInfoRequest cir, int requesterId)
        {
            ClientInfoAttempt cca = new ClientInfoAttempt(cir, requesterId);
            BankClient bankClient = MakeCoreRequest("/v1/getClient", cca.AsJsonString());
            return bankClient;
        }
    }
}
