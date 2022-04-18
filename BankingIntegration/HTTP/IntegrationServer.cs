using BankingIntegration.BankModel;
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

        public IntegrationServer(int preferredPort = 8081) : base(preferredPort)
        {
            Route pingRoute = new Route("/");
            pingRoute.DoGet = (req, res) =>
            {
                EncodeMessage(res, "Home has been hit!");
                return res.StatusCode;
            };
            handledRoutes.Add(pingRoute);

            Route loginRequest = new Route("/v1/login");
            loginRequest.DoPost = (req, res) =>
            {
                if (IsCoreOnline())
                {
                    string reqBody = Utils.ReadFromStream(req.InputStream);
                    UserLoginRequest ulr = JsonSerializer.Deserialize<UserLoginRequest>(reqBody);
                    int? userIdRequest = CoreGetUserId(ulr);
                    if (userIdRequest == null)
                    {
                        EncodeError("The Credentials given are not valid.", ErrorCode.CREDENTIALS_INVALID, res);
                        res.StatusCode = 400;
                    } 
                    else
                    {
                        int userId = (int)userIdRequest;
                        UserSession? existingUserSession = GetUserSession(userId);
                        if (IsUserSessionValid(existingUserSession))
                        {
                            RefreshUserSession(existingUserSession);
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
                            EncodeMessage(res, newSession.AsJsonString());
                            AddUserSession(newSession);
                        }
                        res.StatusCode = 200;
                    }
                } else
                {
                    EncodeError("The core is offline at the moment and cannot process this request.", ErrorCode.CORE_OFFLINE, res);
                    res.StatusCode = 400;
                }
                return res.StatusCode; 

            };
            handledRoutes.Add(loginRequest);


        }

        // Encryption Functions
        private void EncodeError(string message, ErrorCode code, HttpListenerResponse res)
        {
            ErrorMesage em = new ErrorMesage();
            em.Code = code;
            em.ErrorMessage = message;
            EncodeMessage(res, em.AsJsonString());
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
            return userSessions.Find(new Predicate<UserSession>((us) => {
                return us.SessionToken == sessionToken;
            }));
        }
        private UserSession? GetUserSession(int userId)
        {
            return userSessions.Find(new Predicate<UserSession>((us) => {
                return us.UserID == userId;
            }));
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
                userSessions.Add(us);
            }
        }

        // Core Functions
        private bool IsCoreOnline()
        {
            return true;
            Task<bool> coreOnlineTask = GetCoreOnlineTask();
            if (coreOnlineTask.Wait(defaultRequestTimeout))
            {
                coreIsOnline = coreOnlineTask.Result;
            }
            else
            {
                coreIsOnline = false;
            };
           
           return coreIsOnline;
        }
        private async Task<bool> GetCoreOnlineTask()
        {
            UriBuilder builder = new UriBuilder(coreUri);
            builder.Path = "ping";
            HttpResponseMessage res = await client.GetAsync(builder.Uri);
            return res.StatusCode == HttpStatusCode.OK;
        }
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
            return (int?) data.UserId;
        }

    }
}
