using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Threading;

namespace HTTP
{
    public class HttpContextReceivedEventArgs : EventArgs
    {
        public HttpListenerContext Context;

        public HttpContextReceivedEventArgs(HttpListenerContext c)
        {
            Context = c;
        }
    }

    public class HttpService
    {
        HttpListener listener;

        //private static int num = 0;

        public HttpService(string ip, int port)
        {
            listener = new HttpListener();
            listener.Prefixes.Add("http://" + ip + ":" + port.ToString() + "/");
        }

        public void RegisterNewPrefix(string prefix)
        {
            listener.Prefixes.Add(prefix);
        }

        public event EventHandler<HttpContextReceivedEventArgs> HttpContextReceived;

        bool firstStarted = true;

        private void xRun()
        {
            while (true)
            {
                    if (firstStarted)
                    {
                        listener.Start();
                        firstStarted = false;
                    }
                    HttpListenerContext con = listener.GetContext();
                
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        if (HttpContextReceived != null)
                            HttpContextReceived(this, new HttpContextReceivedEventArgs(con));
                    });
            }
        }

        public void Start()
        {
            listener.Start();
            new Thread(xRun).Start();
        }

        public void Pause()
        {
            listener.Stop();
        }
    }
}
