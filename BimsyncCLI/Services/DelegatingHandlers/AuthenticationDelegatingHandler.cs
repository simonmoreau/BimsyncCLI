using BimsyncCLI.Models.Bimsync;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace BimsyncCLI.Services.DelegatingHandlers
{
    class AuthenticationDelegatingHandler : DelegatingHandler
    {
        private readonly AuthenticationService _authenticationService;

        public AuthenticationDelegatingHandler(AuthenticationService authenticationService)
            : base()
        {
            _authenticationService = authenticationService;
        }

        public AuthenticationDelegatingHandler(HttpMessageHandler innerHandler,
          AuthenticationService authenticationService)
      : base(innerHandler)
        {
            _authenticationService = authenticationService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {

            if (request.RequestUri.OriginalString.Contains("https://api.bimsync.com/") || request.RequestUri.OriginalString.Contains("https://bcf.bimsync.com/bcf/"))
            {
                Token token = await _authenticationService.Login();

                if (token != null)
                {
                    request.Headers.Accept.Clear();
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.access_token);
                }
                else
                {
                    return null;
                }
            }

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
            return response;
        }
    }
}
