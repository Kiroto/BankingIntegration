using BankingIntegration.BankModel;

namespace BankingIntegration
{
    class IntegrationServer : HttpServer
    {
        public IntegrationServer(int port = 8081) : base(port) {
            Route makeTransactionRoute = new Route("/v1/transaction");
            Route pingRoute = new Route("/");
            pingRoute.HandledMethods.Add("get", (req, res) =>
            {
                EncodeMessage(res, "Home has been hit!");
                return 1;
            });

            makeTransactionRoute.HandledMethods.Add("post", (req, res) =>
            {
                string reqBody = Utils.ReadFromStream(req.InputStream);
                TransactionAttempt ta = TransactionAttempt.FromJsonString(reqBody);
                MakeLog(new Log($"Request body was:\n{reqBody}"));
                MakeLog(new Log($"Upon serializing and deserializing, we have\n{ta.asJsonString()}"));
                return 1;
            });

            handledRoutes.Add(makeTransactionRoute);
            handledRoutes.Add(pingRoute);
        }
    }
}
