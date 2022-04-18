using BankingIntegration.BankModel;
using System;
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

        public UserSession[] userSessions = new UserSession[] {};

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

        private void EncodeError(string message, ErrorCode code, HttpListenerResponse res)
        {
            ErrorMesage em = new ErrorMesage();
            em.Code = code;
            em.ErrorMessage = message;
            EncodeMessage(res, em.AsJsonString());
        }
        private static String sha256_hash(string value)
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

        private bool IsCoreOnline()
        {
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
