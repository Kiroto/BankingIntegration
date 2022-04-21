using BankingIntegration.BankModel;
using BankingIntegration.BankModel.General;
using BankingIntegration.HTTP;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HttpMethod = BankingIntegration.HTTP.HttpMethod;

namespace BankingIntegration
{
    class IntegrationServer : HttpServer
    {
        private static readonly HttpClient httpClient = new HttpClient();
        public static bool coreIsOnline = false;
        public static Uri coreUri = new Uri("http://localhost:8082");
        public static int defaultRequestTimeout = 1000;
        public static int sessionTimeoutMin = 15;

        private static T MakeCoreRequest<T>(string path, string contents, HttpMethod method = HttpMethod.POST)
        {
            UriBuilder builder = new UriBuilder(coreUri);
            builder.Path = path;
            StringContent httpContents = new StringContent(contents);
            Task<HttpResponseMessage> TResponseMessage;
            if (method == HttpMethod.GET)
            {
                TResponseMessage = httpClient.GetAsync(builder.Uri);
            }
            else if (method == HttpMethod.POST)
            {
                TResponseMessage = httpClient.PostAsync(builder.Uri, httpContents);
            }
            else if (method == HttpMethod.DELETE) // These last two will never be used
            {
                TResponseMessage = httpClient.DeleteAsync(builder.Uri);
            }
            else
            {
                throw new Exception("Unsupported request type");

            }

            if (!TResponseMessage.Wait(defaultRequestTimeout)) throw new CoreTimeoutException();
            Task<string> TResultString = TResponseMessage.Result.Content.ReadAsStringAsync();
            if (!TResultString.Wait(defaultRequestTimeout)) throw new CoreTimeoutException();
            return JsonSerializer.Deserialize<T>(TResultString.Result);
        }

        // Returns the result of the core request or -1 if the request was sent to queue.
        private static T CoreRequestOrQueue<T>(string path, string contents, HttpMethod method = HttpMethod.POST)
        {
            try
            {
                return MakeCoreRequest<T>(path, contents, method);
            }
            catch (CoreTimeoutException e)
            {
                queuedRequests.Add(new QueuedRequest() {
                    Method = method,
                    Contents = contents,
                    Path = path,
                    QueuedTime = DateTime.Now
                });
                throw new TransactionQueuedException();
            }
        }


        public static List<UserSession> userSessions = new List<UserSession>();
        public static List<QueuedRequest> queuedRequests = new List<QueuedRequest>();

        public static readonly ProcessedResponse coreOfflineResponse = new ProcessedResponse() { StatusCode = 503, Contents = MakeErrorMessage("The core is offline at the moment and cannot process this request.", ErrorCode.CORE_OFFLINE) };
        public static readonly ProcessedResponse invalidCredentialsResponse = new ProcessedResponse() { StatusCode = 400, Contents = MakeErrorMessage("The Credentials given are not valid.", ErrorCode.CREDENTIALS_INVALID) };
        public static readonly ProcessedResponse invalidSessionResponse = new ProcessedResponse() { StatusCode = 403, Contents = MakeErrorMessage("The received session key is not valid.", ErrorCode.CREDENTIALS_INVALID) };
        public static readonly ProcessedResponse transactionQueuedResponse = new ProcessedResponse() { StatusCode = 200, Contents = MakeErrorMessage("The transaction has been queued.", ErrorCode.CORE_OFFLINE) };

        public delegate void CoreIsUp();
        public static event CoreIsUp OnCoreUp;

        public IntegrationServer(int preferredPort = 8081) : base(preferredPort)
        {
            OnCoreUp += DoQueuedRequests;

            handledRoutes.Add(new Route("/v1/ping")
            {
                DoGet = (body) =>
                {
                    ProcessedResponse response = new ProcessedResponse();
                    response.Contents = "Home has been hit!";
                    return response;
                }
            });
            handledRoutes.Add(new Route("/v1/login")
            {
                DoPost = (reqBody) =>
                {
                    // Confirm credentials
                    UserLoginRequest ulr = JsonSerializer.Deserialize<UserLoginRequest>(reqBody);
                    ClientSession clientSession = ClientLogin(ulr);

                    UserSession existingSession = GetUserSession(clientSession.UserId); // Return an already existing session by default
                    if (IsUserSessionValid(existingSession))
                    {

                        RefreshUserSession(existingSession);
                    }
                    else
                    {
                        DateTime currentTime = DateTime.Now;
                        clientSession = new ClientSession()
                        {
                            UserId = clientSession.UserId,
                            ClientId = clientSession.ClientId,
                            SessionToken = GenerateSessionToken(ulr),
                            SessionStart = currentTime,
                            LastRequest = currentTime,
                            Service = ulr.ServiceId
                        };
                        AddUserSession(existingSession);
                    }
                    clientSession.StatusCode = 200;
                    return clientSession;
                }
            });
            handledRoutes.Add(new Route("/v1/createClient")
            {
                DoPost = (reqBody) =>
                {
                    // Confirm user session
                    ClientCreationRequest ccr = JsonSerializer.Deserialize<ClientCreationRequest>(reqBody);
                    UserSession userSession = GetUserSession(ccr.SessionToken);

                    ProcessedResponse response = new ProcessedResponse()
                    {
                        Contents = $"{{\"UserId\": {CreateNewClient(ccr, userSession.UserId)}}}",
                        StatusCode = 200
                    };
                    return response;

                }
            });
            handledRoutes.Add(new Route("/v1/getClient")
            {
                DoPost = (reqBody) =>
                {
                    ClientInfoRequest cir = JsonSerializer.Deserialize<ClientInfoRequest>(reqBody);
                    UserSession us = GetUserSession(cir.SessionToken);
                    return GetBankClient(cir, us.UserId);
                }
            });
            handledRoutes.Add(new Route("/v1/updateClient")
            {
                DoPost = (reqBody) =>
                {
                    ClientEditionRequest cer = JsonSerializer.Deserialize<ClientEditionRequest>(reqBody);
                    UserSession us = GetUserSession(cer.SessionToken);
                    return EditBankClient(cer, us.UserId);
                }
            });
            handledRoutes.Add(new Route("/v1/removeClient")
            {
                DoPost = (reqBody) =>
                {
                    ClientDeletionRequest cdr = JsonSerializer.Deserialize<ClientDeletionRequest>(reqBody);
                    UserSession us = GetUserSession(cdr.SessionToken);
                    return DeleteBankClient(cdr, us.UserId);
                }
            });

            // TODO:
            // Route createAdminUser
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
        private static UserSession GetUserSession(string sessionToken)
        {
            UserSession us = userSessions.Find((us) =>
            {
                return us.SessionToken == sessionToken;
            });
            if (us == null) throw new NoSuchUserSessionException(sessionToken);
            return us;
        }
        private static UserSession? GetUserSession(int userId)
        {
            UserSession us = userSessions.Find((us) =>
            {
                return us.UserId == userId;
            });
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
            UserSession? oldUserSession = GetUserSession(us.UserId);
            if (IsUserSessionValid(oldUserSession)) // If a session for that user is still valid, just refresh it
            {
                RefreshUserSession(oldUserSession);
            }
            else
            {
                while (oldUserSession != null) // Remove all other old sessions for that user
                {
                    DeleteUserSession(oldUserSession);
                    oldUserSession = GetUserSession(us.UserId);
                }
                userSessions.Add(us); // Then add the new one
            }
        }

        // <> Core Functions <>
        private static bool IsCoreOnline()
        {
            try
            {
                MakeCoreRequest<dynamic>("ping", "");
                OnCoreUp();
                return true;
            }
            catch
            {
                return false;
            }
        }
        private void DoQueuedRequests()
        {
            foreach (QueuedRequest qr in queuedRequests)
            {
                try
                {
                    CoreRequestOrQueue<dynamic>(qr.Path, qr.Contents, qr.Method);
                    qr.Tried = true;
                } catch (TransactionQueuedException e)
                {
                    // It will be retried anyways
                }
            }
            queuedRequests.RemoveAll((qr) =>
            {
                return qr.Tried == true;
            });
        }

        // Authentication Functions
        private ClientSession ClientLogin(UserLoginRequest ulr)
        {
            ClientSession clientSession = new ClientSession()
            {
                UserId = 42,
                ClientId = 35
            };
            // ClientSession clientSession = MakeCoreRequest<UserSession>("/v1/login", ulr.AsJsonString());
            if (clientSession.UserId == -1) throw new NoSuchUserSessionException();
            return clientSession;
        }

        // Client Functions
        private int CreateNewClient(ClientCreationRequest ccr, int userId)
        {
            try
            {
                ClientCreationAttempt cca = new ClientCreationAttempt(ccr, userId);

                UserSession data = CoreRequestOrQueue<UserSession>("/v1/createClient", cca.AsJsonString());
                return data.UserId;
            }
            catch (CoreTimeoutException e)
            {
                queuedRequests.Add(new QueuedRequest());
                return -1;
            }
        }
        private BankClient GetBankClient(ClientInfoRequest cir, int requesterId)
        {
            ClientInfoAttempt cca = new ClientInfoAttempt(cir, requesterId);
            BankClient bankClient = MakeCoreRequest<BankClient>("/v1/getClient", cca.AsJsonString());
            return bankClient;
        }

        // Returns an empty bankClient if the service is unavailable (it was queued).
        private BankClient EditBankClient(ClientEditionRequest cer, int requesterId)
        {
            ClientEditionAttempt cia = new ClientEditionAttempt(cer, requesterId);
            BankClient data = CoreRequestOrQueue<BankClient>("/v1/updateClient", cia.AsJsonString());
            return data;
        }

        // Returns an empty bankClient if the service is unavailable (it was queued).
        private BankClient DeleteBankClient(ClientDeletionRequest cdr, int requesterId)
        {
            ClientDeletionAttempt cda = new ClientDeletionAttempt(cdr, requesterId);
            BankClient data = CoreRequestOrQueue<BankClient>("/v1/removeClient", cda.AsJsonString());
            return data;
        }
    }
}
