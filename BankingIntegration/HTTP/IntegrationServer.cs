using BankingIntegration.BankModel;
using BankingIntegration.BankModel.General;
using BankingIntegration.BankModel.General.Responses;
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
                queuedRequests.Add(new QueuedRequest()
                {
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
            AddUserSession(new UserSession() { SessionToken = "123456", UserId = 5 });
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

                    if (ulr.ServiceId == 1) // If it comes from web
                    {
                        return LoginInternetBanking(ulr);
                    }
                    else
                    {
                        return LoginCaja(ulr);
                    }

                }
            });
            handledRoutes.Add(new Route("/v1/createClient")
            {
                DoPost = (reqBody) =>
                {
                    return DefaultTransaction<ClientCreationRequest, ClientCreationAttempt, BankClient>(reqBody, "/v1/createClient", true);
                }
            });
            handledRoutes.Add(new Route("/v1/getClient")
            {
                DoPost = (reqBody) =>
                {
                    return DefaultTransaction<ClientInfoRequest, ClientInfoAttempt, BankClient>(reqBody, "/v1/getClient");
                }
            });
            handledRoutes.Add(new Route("/v1/updateClient")
            {
                DoPost = (reqBody) =>
                {
                    return DefaultTransaction<ClientEditionRequest, ClientEditionAttempt, BankClient>(reqBody, "/v1/updateClient", true);
                }
            });
            handledRoutes.Add(new Route("/v1/removeClient")
            {
                DoPost = (reqBody) =>
                {
                    return DefaultTransaction<ClientDeletionRequest, ClientDeletionAttempt, BankClient>(reqBody, "/v1/removeClient", true);
                }
            });
            handledRoutes.Add(new Route("/v1/createAccount")
            {
                DoPost = (reqBody) =>
                {
                    return DefaultTransaction<AccountCreationRequest, AccountCreationAttempt, BankAccount>(reqBody, "/v1/createAccount", true);
                }
            });
            handledRoutes.Add(new Route("/v1/getAccount")
            {
                DoPost = (reqBody) =>
                {
                    return DefaultTransaction<AccountByIdRequest, AccountByIdAttempt, BankAccount>(reqBody, "/v1/getAccount");
                }
            });
            handledRoutes.Add(new Route("/v1/getAccounts")
            {
                DoPost = (reqBody) =>
                {
                    return DefaultTransaction<AccountsByClientRequest, AccountsByClientAttempt, BList<BankAccount>>(reqBody, "/v1/getAccounts");
                }
            });
            handledRoutes.Add(new Route("/v1/removeAccount")
            {
                DoPost = (reqBody) =>
                {
                    return DefaultTransaction<AccountDeletionRequest, AccountDeletionAttempt, BankAccount>(reqBody, "/v1/removeAccount", true);
                }
            });

            // TODO:
            // Route createAdminUser
            // Route updateUser

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

            handledRoutes.Add(new Route("/test/clientStructure")
            {
                DoPost = (reqBody) =>
                {
                    return new BankClient();
                }
            });
            handledRoutes.Add(new Route("/test/accountStructure")
            {
                DoPost = (reqBody) =>
                {
                    return new BankAccount();
                }
            });

            handledRoutes.Add(new Route("/test/accountsStructure")
            {
                DoPost = (reqBody) =>
                {
                    BList<BankAccount> bas = new BList<BankAccount>();
                    bas.Add(new BankAccount());
                    bas.Add(new BankAccount());
                    bas.Add(new BankAccount());
                    return bas;
                }
            });
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
            return us;
        }
        private static UserSession GetUserSession(int userId)
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
                RefreshUserSession(us);
                userSessions.Add(us); // Then add the new one
            }
        }

        // Account Functions
        private static BankAccount CreateBankAccount(AccountCreationRequest acr)
        {
            if (IsUserSessionValid(acr.SessionToken))
            {
                return CoreRequestOrQueue<BankAccount>("/v1/createAccount", acr.BankAccountInfo.AsJsonString());
            }
            else
            {
                throw new NoSuchUserSessionException();
            }
        }

        private static BankAccount GetBankAccount(AccountByIdRequest abir)
        {
            if (IsUserSessionValid(abir.SessionToken))
            {
                return MakeCoreRequest<BankAccount>("/v1/getAccount", abir.AsJsonString());
            }
            else
            {
                throw new NoSuchUserSessionException();
            }
        }

        private static BList<BankAccount> GetBankAccounts(AccountsByClientRequest abcr)
        {
            if (IsUserSessionValid(abcr.SessionToken))
            {
                return MakeCoreRequest<BList<BankAccount>>("/v1/getAccount", abcr.AsJsonString());
            }
            else
            {
                throw new NoSuchUserSessionException();
            }
        }

        private static T DefaultTransaction<A,B, T>(string reqBody, string path, bool queue = false) where A : Sessioned, IAttemptable<B> where T : IResponsible where B : BankSerializable
        {
            A inputValue = JsonSerializer.Deserialize<A>(reqBody);
            UserSession us = GetUserSession(inputValue.SessionToken);
            if (IsUserSessionValid(us))
            {
                if (queue) {
                    return CoreRequestOrQueue<T>(path, inputValue.ToAttempt(us.UserId).AsJsonString());
                }
                else
                {
                    return MakeCoreRequest<T>(path, inputValue.ToAttempt(us.UserId).AsJsonString());
                }
            } else
            {
                throw new NoSuchUserSessionException();
            }
        }

        private ClientSession GenerateClientSession(int userId, int clientId, UserLoginRequest ulr)
        {
            DateTime currentTime = DateTime.Now;
            ClientSession clientSession = new ClientSession()
            {
                UserId = userId,
                ClientId = clientId,
                SessionToken = GenerateSessionToken(ulr),
                SessionStart = currentTime,
                LastRequest = currentTime,
                Service = ulr.ServiceId
            };
            clientSession.StatusCode = 200;
            AddUserSession(clientSession);
            return clientSession;
        }
        private IResponsible LoginInternetBanking(UserLoginRequest ulr)
        {
            ClientSession clientSession = ClientLogin(ulr);

            UserSession existingSession = GetUserSession(clientSession.UserId); // Return an already existing session by default
            if (IsUserSessionValid(existingSession))
            {

                RefreshUserSession(existingSession);
            }
            else
            {
                clientSession = GenerateClientSession(clientSession.UserId, clientSession.ClientId, ulr);
            }
            return clientSession;
        }
        private IResponsible LoginCaja(UserLoginRequest ulr)
        {

            return new UserSession();
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
                }
                catch (TransactionQueuedException e)
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
