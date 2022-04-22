using BankingIntegration.BankModel;
using BankingIntegration.BankModel.Beneficiary;
using BankingIntegration.BankModel.Beneficiary.In;
using BankingIntegration.BankModel.Employee;
using BankingIntegration.BankModel.General;
using BankingIntegration.BankModel.General.Responses;
using BankingIntegration.BankModel.Loan;
using BankingIntegration.BankModel.Transaction;
using BankingIntegration.BankModel.Transaction.CoreOut;
using BankingIntegration.BankModel.Transaction.In;
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

        // App info
        public static bool coreIsOnline = false;
        public static Uri coreUri = new Uri("http://www.cwebservice.somee.com/coreWservice.asmx");
        public static int defaultRequestTimeout = 10000;
        public static int sessionTimeoutMin = 15;
        public static string CoreNamespace = "http://bankCore.org/";

        // Queues and sessions
        public static List<UserSession> userSessions = new List<UserSession>();
        public static List<QueuedRequest> queuedRequests = new List<QueuedRequest>();

        // Prepared responses
        public static readonly ProcessedResponse coreOfflineResponse = new ProcessedResponse() { StatusCode = 503, Contents = MakeErrorMessage("The core is offline at the moment and cannot process this request.", ErrorCode.CORE_OFFLINE) };
        public static readonly ProcessedResponse invalidCredentialsResponse = new ProcessedResponse() { StatusCode = 400, Contents = MakeErrorMessage("The Credentials given are not valid.", ErrorCode.CREDENTIALS_INVALID) };
        public static readonly ProcessedResponse invalidSessionResponse = new ProcessedResponse() { StatusCode = 403, Contents = MakeErrorMessage("The received session key is not valid.", ErrorCode.CREDENTIALS_INVALID) };
        public static readonly ProcessedResponse transactionQueuedResponse = new ProcessedResponse() { StatusCode = 200, Contents = MakeErrorMessage("The transaction has been queued.", ErrorCode.CORE_OFFLINE) };
        public static readonly ProcessedResponse coreEndpointNotReachedResponse = new ProcessedResponse() { StatusCode = 500, Contents = MakeErrorMessage("The transaction couldn't reach core", ErrorCode.CORE_ERROR) };

        public delegate void CoreIsUp();
        public static event CoreIsUp OnCoreUp;

        // Configurator/initiator
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
                    try
                    {
                        if (ulr.ServiceId == 1) // If it comes from web
                        {
                            BankClient authenticatedClient = UnsessionedTransaction<UserLoginRequest, UserLoginRequest, BankClient>(reqBody);
                            ClientSession cs = new ClientSession(authenticatedClient);
                            AddUserSession(cs);

                            return cs;
                        }
                        else
                        {
                            BankEmployee authenticatedEmployee = UnsessionedTransaction<UserLoginRequest, UserLoginRequest, BankEmployee>(reqBody);
                            EmployeeSession es = new EmployeeSession(authenticatedEmployee);
                            AddUserSession(es);
                            return es;
                        }
                    }
                    catch (CoreDidntLikeThatException e)
                    {
                        throw new NoSuchAccountException();
                    }
                }
            });

            handledRoutes.Add(new Route("/v1/createClient")
            {
                DoPost = (reqBody) =>
                {
                    return SessionedTransaction<ClientCreationRequest, ClientCreationAttempt, BankClient>(reqBody, true);
                }
            });
            handledRoutes.Add(new Route("/v1/getClient")
            {
                DoPost = (reqBody) =>
                {
                    return SessionedTransaction<ClientInfoRequest, ClientInfoAttempt, BankClient>(reqBody);
                }
            });
            handledRoutes.Add(new Route("/v1/updateClient")
            {
                DoPost = (reqBody) =>
                {
                    return SessionedTransaction<ClientEditionRequest, ClientEditionAttempt, BankClient>(reqBody, true);
                }
            });
            handledRoutes.Add(new Route("/v1/removeClient")
            {
                DoPost = (reqBody) =>
                {
                    return SessionedTransaction<ClientDeletionRequest, ClientDeletionAttempt, BankClient>(reqBody, true);
                }
            });

            handledRoutes.Add(new Route("/v1/createAccount")
            {
                DoPost = (reqBody) =>
                {
                    return SessionedTransaction<AccountCreationRequest, AccountCreationAttempt, BankAccount>(reqBody, true);
                }
            });
            handledRoutes.Add(new Route("/v1/getAccount")
            {
                DoPost = (reqBody) =>
                {

                    return SessionedTransaction<AccountByIdRequest, AccountByIdAttempt, BankAccount>(reqBody);
                }
            });
            handledRoutes.Add(new Route("/v1/getAccounts")
            {
                DoPost = (reqBody) =>
                {
                    return SessionedTransaction<AccountsByClientRequest, AccountsByClientAttempt, BList<BankAccount>>(reqBody);
                }
            });
            handledRoutes.Add(new Route("/v1/removeAccount")
            {
                DoPost = (reqBody) =>
                {
                    return SessionedTransaction<AccountDeletionRequest, AccountDeletionAttempt, BankAccount>(reqBody, true);
                }
            });

            handledRoutes.Add(new Route("/v1/payLoan")
            {
                DoPost = (reqBody) =>
                {
                    return SessionedTransaction<PayLoanRequest, PayLoanAttempt, BankLoan>(reqBody, true);
                }
            });

            handledRoutes.Add(new Route("/v1/getLoansByClient")
            {
                DoPost = (reqBody) =>
                {
                    return SessionedTransaction<LoansByClientRequest, LoansByClientAttempt, BList<BankLoan>>(reqBody);
                }
            });
            handledRoutes.Add(new Route("/v1/addBeneficiario")
            {
                DoPost = (reqBody) =>
                {
                    return SessionedTransaction<BeneficiaryCreationRequest, BeneficiaryCreationAttempt, BankBeneficiary>(reqBody);
                }
            });

            handledRoutes.Add(new Route("/v1/getBeneficiarioByClient")
            {
                DoPost = (reqBody) =>
                {
                    return SessionedTransaction<BeneficiaryByClientRequest, BeneficiaryByClientAttempt, BList<BankBeneficiary>>(reqBody);
                }
            });
            handledRoutes.Add(new Route("/v1/insertTransaction") // Can add or withdraw here, too
            {
                DoPost = (reqBody) =>
                {
                    return SessionedTransaction<TransactionRequest, TransactionAttempt, Transaction>(reqBody);
                }
            });
            handledRoutes.Add(new Route("/v1/getAccountTransactions")
            {
                DoPost = (reqBody) =>
                {
                    return SessionedTransaction<TransactionsByAccountRequest, TransactionsByAccountAttempt, BList<Transaction>>(reqBody);
                }
            });

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

            handledRoutes.Add(new Route("/test/sha256")
            {
                DoPost = (reqBody) =>
                {
                    return new ProcessedResponse() { Contents = UserSession.sha256_hash(reqBody)};
                }
            });

            handledRoutes.Add(new Route("/test/transaction")
            {
                DoPost = (reqBody) =>
                {
                    return new ProcessedResponse() { Contents = new TransactionAttempt().AsJsonString() };
                }
            });

            handledRoutes.Add(new Route("/test/creationAttempt")
            {
                DoPost = (reqBody) =>
                {
                    return new ProcessedResponse() { Contents = new ClientCreationAttempt(new ClientCreationRequest() { BankClientInfo = new BankClient()}, 0).AsJsonString() };
                }
            });

            handledRoutes.Add(new Route("/test/creationAttempt")
            {
                DoPost = (reqBody) =>
                {
                    return new ProcessedResponse() { Contents = new ClientCreationAttempt(new ClientCreationRequest() { BankClientInfo = new BankClient() }, 0).AsJsonString() };
                }
            });
        }

        // Builds contents to make core requests
        private static StringContent buildContents(IAttempt attempt)
        {
            string json = attempt.AsJsonString();
            string content = @$"<?xml version=""1.0"" encoding=""utf-8""?>
            <soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
              <soap:Body>
                <{attempt.ActionName} xmlns=""{CoreNamespace}"">
                  <json>{json}</json>
                </{attempt.ActionName}>
              </soap:Body>
            </soap:Envelope>";
            return new StringContent(content, Encoding.UTF8, "text/xml");
        }
        // Extracts the json output from the xml response
        private static string GetJsonFromXml(string xml, string actionName)
        {
            string startTag = $"<{actionName}Result>";
            string endTag = $"</{actionName}Result>";
            int jsonStart = xml.IndexOf(startTag) + startTag.Length;
            int jsonLength = xml.IndexOf(endTag) - jsonStart;
            if (jsonLength < 0) throw new CoreEndpointNotReached();
            return xml.Substring(jsonStart, jsonLength);
        }
        // Makes a request to the core
        private static T MakeCoreRequest<T>(IAttempt contents, HttpMethod method = HttpMethod.POST)
        {
            StringContent httpContents = buildContents(contents);
            httpClient.DefaultRequestHeaders.Remove("SOAPAction");
            httpClient.DefaultRequestHeaders.Add("SOAPAction", $"{CoreNamespace}{contents.ActionName}");

            Task<HttpResponseMessage> TResponseMessage;
            if (method == HttpMethod.GET)
            {
                TResponseMessage = httpClient.GetAsync(coreUri);
            }
            else if (method == HttpMethod.POST)
            {
                TResponseMessage = httpClient.PostAsync(coreUri, httpContents);
            }
            else if (method == HttpMethod.DELETE) // These last two will never be used
            {
                TResponseMessage = httpClient.DeleteAsync(coreUri);
            }
            else
            {
                throw new Exception("Unsupported request type");
            }

            if (!TResponseMessage.Wait(defaultRequestTimeout)) throw new CoreTimeoutException();
            Task<string> TResultString = TResponseMessage.Result.Content.ReadAsStringAsync();
            if (!TResultString.Wait(defaultRequestTimeout)) throw new CoreTimeoutException();
            string xmlResult = TResultString.Result;
            string jsonResult = GetJsonFromXml(xmlResult, contents.ActionName);
            if (jsonResult == "null") throw new CoreDidntLikeThatException();
            return JsonSerializer.Deserialize<T>(jsonResult);
        }
        // Returns the result of the core request or -1 if the request was sent to queue.
        private static T CoreRequestOrQueue<T>(IAttempt contents, HttpMethod method = HttpMethod.POST)
        {
            try
            {
                return MakeCoreRequest<T>(contents, method);
            }
            catch (CoreTimeoutException e)
            {
                queuedRequests.Add(new QueuedRequest()
                {
                    Method = method,
                    Contents = contents,
                    QueuedTime = DateTime.Now
                });
                throw new TransactionQueuedException();
            }
        }

        // Encryption Functions
        public static string MakeErrorMessage(string message, ErrorCode code)
        {
            ErrorMesage em = new ErrorMesage();
            em.Code = code;
            em.ErrorMessage = message;
            return em.AsJsonString();
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
        private static bool IsUserSessionValid(UserSession us)
        {
            return us != null && us.LastRequest < DateTime.Now.AddMinutes(sessionTimeoutMin);
        }
        private static bool RefreshUserSession(UserSession us)
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
                us.SessionToken = oldUserSession.SessionToken;
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

        // Core Transaction functions
        private static T UnsessionedTransaction<A, B, T>(string reqBody, bool queue = false) where A : IRequest<B>, IAttempt where T : IResponsible where B : IAttempt
        {
            A inputValue = JsonSerializer.Deserialize<A>(reqBody);

            if (queue)
            {
                return CoreRequestOrQueue<T>(inputValue);
            }
            else
            {
                return MakeCoreRequest<T>(inputValue);
            }

        }
        private static T SessionedTransaction<A, B, T>(string reqBody, bool queue = false) where A : ISessioned, IRequest<B> where T : IResponsible where B : IAttempt
        {
            A inputValue = JsonSerializer.Deserialize<A>(reqBody);
            UserSession us = GetUserSession(inputValue.SessionToken);
            if (IsUserSessionValid(us))
            {
                RefreshUserSession(us);
                if (queue)
                {
                    return CoreRequestOrQueue<T>(inputValue.ToAttempt(us.UserId));
                }
                else
                {
                    return MakeCoreRequest<T>(inputValue.ToAttempt(us.UserId));
                }
            }
            else
            {
                throw new NoSuchUserSessionException();
            }
        }

        // Queued request functions
        private void DoQueuedRequests()
        {
            foreach (QueuedRequest qr in queuedRequests)
            {
                try
                {
                    CoreRequestOrQueue<dynamic>(qr.Contents, qr.Method);
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
    }
}
