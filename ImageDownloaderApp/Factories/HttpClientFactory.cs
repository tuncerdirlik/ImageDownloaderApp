using System;
using System.Net.Http;

namespace ImageDownloaderApp.Factories
{
    public class HttpClientFactory : IDisposable
    {
        private readonly HttpClientHandler _handler;

        public HttpClientFactory()
        {
            _handler = new HttpClientHandler();
        }

        public HttpClient CreateClient()
        {
            return new HttpClient(_handler, false);
        }

        public void Dispose()
        {
            _handler.Dispose();
        }
    }
}
