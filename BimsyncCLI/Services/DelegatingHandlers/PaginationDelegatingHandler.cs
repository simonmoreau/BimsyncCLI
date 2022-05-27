using BimsyncCLI.Models.Bimsync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BimsyncCLI.Services.DelegatingHandlers
{
    class PaginationDelegatingHandler : DelegatingHandler
    {
        public PaginationDelegatingHandler()
            : base()
        {

        }

        public PaginationDelegatingHandler(HttpMessageHandler innerHandler)
      : base(innerHandler)
        {

        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
            
            return response;
        }
    }
}
