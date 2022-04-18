using BankingIntegration.BankModel;
using BankingIntegration.HTTP;
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
        public int port = 8081;
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

        public HttpServer(int preferredPort)
        {
            port = preferredPort;
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
            try
            {
                HttpListenerContext ctx = o as HttpListenerContext;
                HandleContext(ctx.Request, ctx.Response);
            } catch (Exception e)
            {
                MakeLog(new Log($"Fatal Error, Unhandled exception {e.Message}", Log.LogSource.Self, Log.LogSeverity.Error));
            }
        }

        public Route? GetCorrespondingRoute(string localPath)
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
                ProcessedResponse reqResponse = new ProcessedResponse() { StatusCode = -2 };
                Route? foundRoute = GetCorrespondingRoute(req.Url.LocalPath);
                // By default return plain text.
                res.ContentType = "text/plain";
                if (foundRoute != null)
                {
                    HttpMethod method = Route.MethodFromString(req.HttpMethod);
                    string reqBody = Utils.ReadFromStream(req.InputStream);
                    reqResponse = foundRoute.Handle(reqBody, method);
                    
                    if (reqResponse.StatusCode == -1)
                    {
                        res.StatusCode = (int)HttpStatusCode.BadRequest;
                        EncodeMessage(res, "400 - Bad Request");
                    } else
                    {
                        res.StatusCode = reqResponse.StatusCode;
                        res.ContentType = "application/json";
                        EncodeMessage(res, reqResponse.Contents);
                    }
                }
                else
                {
                    res.StatusCode = (int)HttpStatusCode.NotFound;
                    EncodeMessage(res, "404 - Not Found");
                }
                MakeLog(new HttpReqLog(req, reqResponse.StatusCode));
            }
            finally
            {
                res.Close();
            }
        }
    }
}
