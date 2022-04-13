using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace BankingIntegration
{
    class HttpServer
    {
        public static void EncodeMessage(HttpListenerResponse res, string message)
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            res.ContentLength64 = data.Length;
            res.OutputStream.Write(data);
        }

        private bool running = false;
        private HttpListener listener;
        Thread serverThread;

        public List<Route> handledRoutes = new List<Route>();

        public delegate void LogHandler(Log log);
        public event LogHandler NewLog;

        public HttpServer(int port)
        {
            listener = new HttpListener();
            listener.Prefixes.Add($"http://localhost:{port}/");
        }

        public void Start()
        {
            serverThread = new Thread(new ThreadStart(Run));
            serverThread.Start();
        }

        public void Stop()
        {
            running = false;
            serverThread?.Interrupt();
        }

        void Run()
        {
            running = true;
            listener.Start();
            while (running)
            {
                HttpListenerContext client = listener.GetContext();
                ThreadPool.QueueUserWorkItem(HandleContext, client);
            }
            listener.Stop();
        }

        void HandleContext(object o)
        {
            HttpListenerContext ctx = o as HttpListenerContext;
            HandleContext(ctx.Request, ctx.Response);
        }

        Route? GetCorrespondingRoute(string localPath)
        {
            foreach (Route route in handledRoutes)
            {
                if (route.HandledPath == localPath)
                    return route;
            }
            return null;
        }

        public void MakeLog(Log log) {
            NewLog(log);
        }

        void HandleContext(HttpListenerRequest req, HttpListenerResponse res)
        {
            try
            {
                Route? foundRoute = GetCorrespondingRoute(req.Url.LocalPath);
                // By default return plain text.
                res.ContentType = "text/plain";
                string method = req.HttpMethod.ToLower();
                if (foundRoute != null)
                {
                    if (foundRoute.HandledMethods.ContainsKey(method))
                    {
                        foundRoute.HandledMethods[method](req, res);
                        NewLog(new HttpReqLog($"Served {req.Url.LocalPath} and handled.", method));
                    } else
                    {
                        EncodeMessage(res, "400 - Bad Request");
                        res.StatusCode = (int)HttpStatusCode.BadRequest;
                        MakeLog(new HttpReqLog($"Served {req.Url.LocalPath}, but was wrong method.", method)
                        {
                            Severity = Log.LogSeverity.Warning
                        });
                    }
                }
                else
                {
                    EncodeMessage(res, "404 - Not found");
                    res.StatusCode = (int)HttpStatusCode.NotFound;
                    NewLog(new HttpReqLog($"Served {req.Url.LocalPath}, but hit 404", method)
                    {
                        Severity = Log.LogSeverity.Warning
                    });
                }
            } finally
            {
                res.Close();
            }
        }
    }
}
