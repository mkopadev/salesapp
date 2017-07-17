using System;
using System.IO;
using System.Net;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.Connectivity;
using SalesApp.Core.Services.DependancyInjection;

namespace SalesApp.Core.Tests.Services.Connectivity
{
    public class ConnectivityService : IConnectivityService
    {
        class OurWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri address)
            {
                WebRequest webRequest = base.GetWebRequest(address);
                webRequest.Timeout = 10000;
                return webRequest;
            }
        }

        private ILog _log = Resolver.Instance.Get<ILog>();

        public ConnectivityService()
        {
            _log.Initialize(this.GetType().FullName);
        }

        public bool HasConnection()
        {
            return ConnectionExists();
        }

        private bool ConnectionExists()
        {
            try
            {
                _log.Debug("Checking for internet connection. This call times out in 10 seconds so don't freak");
                using (var client = new OurWebClient())
                {
                    var stream = client.OpenRead("http://www.google.com");
                    _log.Debug("Connection exists.");
                    stream.Dispose();
                    stream = Stream.Null;
                     return true;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                _log.Debug("Not to worry, above stuff probably just means there is currently no internet connection. Hmm or maybe google is down?");
                return false;
            }
        }
    }
}