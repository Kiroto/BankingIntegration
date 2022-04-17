using BankingIntegration.BankModel;

namespace BankingIntegration
{
    class IntegrationServer : HttpServer
    {
        public IntegrationServer(int preferredPort = 8081) : base(preferredPort) {

            Route makeTransactionRoute = new Route("/v1/transaction");
            makeTransactionRoute.DoPost = (req, res) =>
            {
                string reqBody = Utils.ReadFromStream(req.InputStream);
                TransactionAttempt ta = TransactionAttempt.FromJsonString(reqBody);
                MakeLog(new Log($"Request body was:\n{reqBody}"));
                MakeLog(new Log($"Upon serializing and deserializing, we have\n{ta.asJsonString()}"));
                return 1;
            };
            handledRoutes.Add(makeTransactionRoute);

            Route pingRoute = new Route("/");
            pingRoute.DoGet = (req, res) =>
            {
                EncodeMessage(res, "Home has been hit!");
                return 1;
            };
            handledRoutes.Add(pingRoute);
        }
    }
}
