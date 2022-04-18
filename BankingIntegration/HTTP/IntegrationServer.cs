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
        private static readonly HttpClient client = new HttpClient();
        public bool coreIsOnline = false;
        public Uri coreUri = new Uri("http://localhost:8082");
        public int defaultRequestTimeout = 1000;
        public int sessionTimeoutMin = 15;

        public List<UserSession> userSessions = new List<UserSession>();
        public List<QueuedRequest> queuedRequests = new List<QueuedRequest>();

        private ProcessedResponse coreOfflineResponse = new ProcessedResponse() { StatusCode = 503, Contents = MakeErrorMessage("The core is offline at the moment and cannot process this request.", ErrorCode.CORE_OFFLINE) };
        private ProcessedResponse invalidCredentialsResponse = new ProcessedResponse() { StatusCode = 400, Contents = MakeErrorMessage("The Credentials given are not valid.", ErrorCode.CREDENTIALS_INVALID) };
        private ProcessedResponse invalidSessionResponse = new ProcessedResponse() { StatusCode = 403, Contents = MakeErrorMessage("The received session key is not valid", ErrorCode.CREDENTIALS_INVALID) };


        public IntegrationServer(int preferredPort = 8081) : base(preferredPort)
        {
            Route pingRoute = new Route("/");
            pingRoute.DoGet = (body) =>
            {
                ProcessedResponse response = new ProcessedResponse();
                response.Contents = "Home has been hit!";
                return response;
            };
            handledRoutes.Add(pingRoute);

            Route loginRequest = new Route("/v1/login");
            loginRequest.DoPost = (reqBody) =>
            {
                // Cant process if core is offline
                if (!IsCoreOnline())
                    return coreOfflineResponse;

                // Confirm credentials
                UserLoginRequest ulr = JsonSerializer.Deserialize<UserLoginRequest>(reqBody);
                int? userIdRequest = CoreGetUserId(ulr);
                if (userIdRequest == null)
                    return invalidCredentialsResponse;


                // Create the session
                ProcessedResponse response = new ProcessedResponse();
                int userId = (int)userIdRequest;
                UserSession? existingUserSession = GetUserSession(userId);
                if (IsUserSessionValid(existingUserSession))
                {
                    RefreshUserSession(existingUserSession);
                    response.Contents = existingUserSession.AsJsonString();
                }
                else
                {
                    DateTime currentTime = DateTime.Now;
                    UserSession newSession = new UserSession()
                    {
                        UserID = userId,
                        SessionToken = GenerateSessionToken(ulr),
                        SessionStart = currentTime,
                        LastRequest = currentTime,
                        Service = ulr.ServiceId
                    };
                    AddUserSession(newSession);
                    response.Contents = newSession.AsJsonString();
                }
                response.StatusCode = 200;
                return response;
            };
            handledRoutes.Add(loginRequest);

            Route createClientRequest = new Route("/v1/createClient");
            createClientRequest.DoPost = (reqBody) =>
            {
                // Confirm user session
                ClientCreationRequest ccr = JsonSerializer.Deserialize<ClientCreationRequest>(reqBody);
                UserSession? userSession = GetUserSession(ccr.EmployeeSessionToken);
                if (!IsUserSessionValid(userSession))
                    return invalidSessionResponse;

                ProcessedResponse response = new ProcessedResponse();
                ClientCreationAttempt cca = new ClientCreationAttempt(ccr, userSession.UserID);
                if (CreateNewClient(cca) != -1) response.StatusCode = 200;
                else response.StatusCode = 503;
                
                return response;

            };
            handledRoutes.Add(createClientRequest);
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
        private UserSession? GetUserSession(string sessionToken) {
            return userSessions.Find((us) => {
                return us.SessionToken == sessionToken;
            });
        }
        private UserSession? GetUserSession(int userId)
        {
            return userSessions.Find(new Predicate<UserSession>((us) => {
                return us.UserID == userId;
            }));
        }
        private bool IsUserSessionValid(string sessionToken)
        {
            UserSession? us = GetUserSession(sessionToken);
            if (us == null) return false;
            return IsUserSessionValid(us);
        }
        private bool IsUserSessionValid(UserSession us)
        {
            return us.LastRequest < DateTime.Now.AddMinutes(sessionTimeoutMin);
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
        private bool IsCoreOnline()
        {
            UriBuilder builder = new UriBuilder(coreUri);
            builder.Path = "ping";
            Task<HttpResponseMessage> res = client.GetAsync(builder.Uri);
            if (res.Wait(defaultRequestTimeout))
            {
                bool oldState = coreIsOnline;
                coreIsOnline = res.Result.StatusCode == HttpStatusCode.OK;
                if (!oldState && coreIsOnline)
                {
                    DoQueuedRequests();
                }
            }
            else
            {
                coreIsOnline = false;
            };
            return coreIsOnline;
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
        private int? CoreGetUserId(UserLoginRequest ulr)
        {
            return 1;
            UriBuilder builder = new UriBuilder(coreUri);
            builder.Path = "v1/login";
            HttpContent content = new StringContent(ulr.AsJsonString());
            Task<HttpResponseMessage> TResponseMessage = client.PostAsync(builder.Uri, content);

            if (!TResponseMessage.Wait(defaultRequestTimeout)) throw new CoreTimeoutException(); // Si no responde en un segundo, no es nada
            Task<string> TResultString = TResponseMessage.Result.Content.ReadAsStringAsync();
            if (!TResultString.Wait(defaultRequestTimeout)) throw new CoreTimeoutException(); // Si no se procesa la informacion en un segundo, falla
            string resultString = TResultString.Result;
            dynamic data = JsonSerializer.Deserialize<dynamic>(resultString);
            return data.UserId;
        }

        // Client Functions
        private int? CreateNewClient(ClientCreationAttempt cca)
        {
            if (IsCoreOnline())
            {
                UriBuilder builder = new UriBuilder(coreUri);
                builder.Path = "/v1/createClient";
                HttpContent content = new StringContent(cca.AsJsonString());

                Task<HttpResponseMessage> TResponseMessage = client.PostAsync(builder.Uri, content);
                if (!TResponseMessage.Wait(defaultRequestTimeout)) throw new CoreTimeoutException();
                Task<string> TResultString = TResponseMessage.Result.Content.ReadAsStringAsync();
                if (!TResultString.Wait(defaultRequestTimeout)) throw new CoreTimeoutException();
                string resultString = TResultString.Result;

                dynamic data = JsonSerializer.Deserialize<dynamic>(resultString);
                return data.UserId;
            } else
            {
                queuedRequests.Add(new QueuedRequest());
            }
            return -1; 
        }

    }
}
