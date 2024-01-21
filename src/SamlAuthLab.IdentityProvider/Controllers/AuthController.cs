namespace SamlAuthLab.IdentityProvider.Controllers
{
    using System.Diagnostics;
    using System.Security.Claims;

    using ITfoxtec.Identity.Saml2;
    using ITfoxtec.Identity.Saml2.MvcCore;
    using ITfoxtec.Identity.Saml2.Schemas;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens.Saml2;

    using SamlAuthLab.IdentityProvider.Cache;

    public class AuthController : Controller
    {
        private readonly Saml2Configuration config;
        private readonly SamlRequestStore _requestStore;

        public AuthController(IOptions<Saml2Configuration> samlOptions, SamlRequestStore requestStore)
        {
            config = samlOptions.Value;
            _requestStore = requestStore;
        }

        public IActionResult Login()
        {
            var requestBinding = new Saml2RedirectBinding();
            var relyingParty = requestBinding.ReadSamlRequest(Request.ToGenericHttpRequest(), new Saml2AuthnRequest(config))?.Issuer;

            var saml2AuthnRequest = new Saml2AuthnRequest(config);
            try
            {
                requestBinding.Unbind(Request.ToGenericHttpRequest(), saml2AuthnRequest);

                if (HttpContext.Request.Cookies.TryGetValue(".AspNetCore.Session", out var id))
                    _requestStore.StoreRequest(id, saml2AuthnRequest);

                return View();
            }
            catch (Exception exc)
            {
#if DEBUG
                Debug.WriteLine($"Saml 2.0 Authn Request error: {exc}\nSaml Auth Request: '{saml2AuthnRequest.XmlDocument?.OuterXml}'\nQuery String: {Request.QueryString}");
#endif
                return LoginPostResponse(saml2AuthnRequest.Id, saml2AuthnRequest.AssertionConsumerServiceUrl, Saml2StatusCodes.Responder, requestBinding.RelayState);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DoLogin()
        {
            Saml2AuthnRequest? request = null;
            try
            {
                if (!HttpContext.Request.Cookies.TryGetValue(".AspNetCore.Session", out var id))
                    return BadRequest("No session id found");

                if (_requestStore.TryGetRequestForSessionId(id!, out request))
                {
                    return LoginPostResponse(request!.Id, request.AssertionConsumerServiceUrl, Saml2StatusCodes.Success, string.Empty, CreateTestUserClaims());
                }
                return BadRequest("No request found");
            }
            catch (Exception exc)
            {
                return LoginPostResponse(request!.Id, request.AssertionConsumerServiceUrl, Saml2StatusCodes.Responder, string.Empty);
            }
        }

        private IActionResult LoginPostResponse(Saml2Id inResponseTo, Uri assertionConsumerUri, Saml2StatusCodes status, string relayState, IEnumerable<Claim>? claims = null)
        {
            var responsebinding = new Saml2PostBinding
            {
                RelayState = relayState
            };

            var saml2AuthnResponse = new Saml2AuthnResponse(config)
            {
                InResponseTo = inResponseTo,
                Status = status,
                Destination = assertionConsumerUri,
                SessionIndex = string.Empty
            };
            if (status == Saml2StatusCodes.Success && claims != null)
            {
                var claimsIdentity = new ClaimsIdentity(claims);
                saml2AuthnResponse.NameId = new Saml2NameIdentifier(claimsIdentity.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).Single(), NameIdentifierFormats.Persistent);
                saml2AuthnResponse.ClaimsIdentity = claimsIdentity;

                var token = saml2AuthnResponse.CreateSecurityToken(config.Issuer, subjectConfirmationLifetime: 5, issuedTokenLifetime: 60);
            }

            return responsebinding.Bind(saml2AuthnResponse).ToActionResult();
        }

        private static IEnumerable<Claim> CreateTestUserClaims()
        {
            var userId = "12345";
            yield return new Claim(ClaimTypes.NameIdentifier, userId);
            yield return new Claim(ClaimTypes.Upn, $"{userId}@email.test");
            yield return new Claim(ClaimTypes.Email, $"{userId}@someemail.test");
        }
    }
}