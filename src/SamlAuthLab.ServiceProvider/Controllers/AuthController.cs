namespace SamlAuthLab.ServiceProvider.Controllers
{
    using ITfoxtec.Identity.Saml2;
    using ITfoxtec.Identity.Saml2.MvcCore;
    using ITfoxtec.Identity.Saml2.Schemas;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;

    using SamlAuthLab.ServiceProvider.Data;

    [AllowAnonymous]
    public class AuthController : Controller
    {
        private readonly Saml2Configuration configuration;
        private readonly UserStoreOptions _userStore;
        public AuthController(IOptions<Saml2Configuration> config, IOptions<UserStoreOptions> users)
        {
            configuration = config.Value;
            _userStore = users.Value;
        }

        [HttpGet]
        public IActionResult Login()
        {
            var binding = new Saml2RedirectBinding();

            var request = new Saml2AuthnRequest(configuration)
            {
                Subject = new Subject { NameID = new NameID { ID = "abcd" } },
                NameIdPolicy = new NameIdPolicy { AllowCreate = true, Format = "urn:oasis:names:tc:SAML:2.0:nameid-format:persistent" },
                AssertionConsumerServiceUrl = new Uri("https://localhost:7145/Auth/ConsumeAssertion")
            };

            HttpContext.Session.SetString("RequestId", request.IdAsString);

            var bound = binding.Bind(request);
            return bound.ToActionResult();
        }

        /// <summary>
        /// No validation whatsoever
        /// </summary>
        [HttpPost] 
        public async Task<IActionResult> ConsumeAssertionWithoutChecks()
        {
            var genericRequest = await Request.ToGenericHttpRequestAsync();

            var binding = new Saml2PostBinding();
            var response = new Saml2AuthnResponse(null);

            // Simply reads the response XML but does not validate XML signatures
            // Ensures we receive a valid SAML2 AuthnResponse
            binding.ReadSamlResponse(genericRequest, response);

            await AuthenticateUser(response);
            return RedirectToPage("Index");
        }

        /// <summary>
        /// Message validation
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ConsumeAssertionAllowUnsolicited()
        {
            var genericRequest = await Request.ToGenericHttpRequestAsync();

            var binding = new Saml2PostBinding();
            var response = new Saml2AuthnResponse(null);

            binding.ReadSamlResponse(genericRequest, response);
            binding.Unbind(genericRequest, response);

            await AuthenticateUser(response);
            return RedirectToPage("Index");
        }

        /// <summary>
        /// Message validation + no unsolicited responses
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ConsumeAssertion()
        {
            var genericRequest = await Request.ToGenericHttpRequestAsync();

            var binding = new Saml2PostBinding();
            var response = new Saml2AuthnResponse(configuration);

            binding.ReadSamlResponse(genericRequest, response);

            var requestId = HttpContext.Session.GetString("RequestId");
            if (requestId == null || response.InResponseToAsString != requestId)
            {
                // Authentication failed
            }
             
            binding.Unbind(genericRequest, response);

            await AuthenticateUser(response);
            return RedirectToPage("Index");
        }

        private async Task AuthenticateUser(Saml2AuthnResponse response)
        {
            // Authenticate user
            // The data is present in response.ClaimsIdentity
        }
    }
}
