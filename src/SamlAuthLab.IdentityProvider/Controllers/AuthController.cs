#if TODO
namespace SamlAuthLab.IdentityProvider.Controllers
{
    using ITfoxtec.Identity.Saml2;
    using ITfoxtec.Identity.Saml2.MvcCore;
    using ITfoxtec.Identity.Saml2.Schemas;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.IdentityModel.Tokens.Saml2;
    using System.Diagnostics;
    using System.Security.Claims;
    using System.ServiceModel.Channels;

    public class AuthController : Controller
    {
        [Route("Login")]
        public async Task<IActionResult> Login()
        {
            var requestBinding = new Saml2RedirectBinding();
            var relyingParty = requestBinding.ReadSamlRequest(Request.ToGenericHttpRequest(), new Saml2AuthnRequest(null))?.Issuer;

            var saml2AuthnRequest = new Saml2AuthnRequest(null);
            try
            {
                requestBinding.Unbind(Request.ToGenericHttpRequest(), saml2AuthnRequest);

                // ****  Handle user login e.g. in GUI ****
                // Test user with session index and claims
                var sessionIndex = Guid.NewGuid().ToString();
                var claims = CreateTestUserClaims(saml2AuthnRequest.Subject?.NameID?.ID);

                return LoginPostResponse(saml2AuthnRequest.Id, Saml2StatusCodes.Success, requestBinding.RelayState, relyingParty, sessionIndex, claims);
            }
            catch (Exception exc)
            {
#if DEBUG
                Debug.WriteLine($"Saml 2.0 Authn Request error: {exc}\nSaml Auth Request: '{saml2AuthnRequest.XmlDocument?.OuterXml}'\nQuery String: {Request.QueryString}");
#endif
                return LoginPostResponse(saml2AuthnRequest.Id, Saml2StatusCodes.Responder, requestBinding.RelayState, relyingParty);
            }
        }

        private IActionResult LoginPostResponse(Saml2Id inResponseTo, Saml2StatusCodes status, string relayState, RelyingParty relyingParty, string sessionIndex = null, IEnumerable<Claim> claims = null)
        {
            var responsebinding = new Saml2PostBinding();
            responsebinding.RelayState = relayState;

            var saml2AuthnResponse = new Saml2AuthnResponse(GetRpSaml2Configuration(relyingParty))
            {
                InResponseTo = inResponseTo,
                Status = status,
                Destination = relyingParty.AcsDestination,
            };
            if (status == Saml2StatusCodes.Success && claims != null)
            {
                saml2AuthnResponse.SessionIndex = sessionIndex;

                var claimsIdentity = new ClaimsIdentity(claims);
                saml2AuthnResponse.NameId = new Saml2NameIdentifier(claimsIdentity.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).Single(), NameIdentifierFormats.Persistent);
                //saml2AuthnResponse.NameId = new Saml2NameIdentifier(claimsIdentity.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).Single());
                saml2AuthnResponse.ClaimsIdentity = claimsIdentity;

                var token = saml2AuthnResponse.CreateSecurityToken(relyingParty.Issuer, subjectConfirmationLifetime: 5, issuedTokenLifetime: 60);
            }

            return responsebinding.Bind(saml2AuthnResponse).ToActionResult();
        }

        private Saml2Configuration GetRpSaml2Configuration(RelyingParty relyingParty = null)
        {
            var rpConfig = new Saml2Configuration()
            {
                Issuer = config.Issuer,
                SignAuthnRequest = config.SignAuthnRequest,
                SingleSignOnDestination = config.SingleSignOnDestination,
                SingleLogoutDestination = config.SingleLogoutDestination,
                ArtifactResolutionService = config.ArtifactResolutionService,
                SigningCertificate = config.SigningCertificate,
                SignatureAlgorithm = config.SignatureAlgorithm,
                CertificateValidationMode = config.CertificateValidationMode,
                RevocationMode = config.RevocationMode
            };

            rpConfig.AllowedAudienceUris.AddRange(config.AllowedAudienceUris);

            if (relyingParty != null)
            {
                rpConfig.SignatureValidationCertificates.Add(relyingParty.SignatureValidationCertificate);
                rpConfig.EncryptionCertificate = relyingParty.EncryptionCertificate;
            }

            return rpConfig;
        }

        private IEnumerable<Claim> CreateTestUserClaims(string selectedNameID)
        {
            var userId = selectedNameID ?? "12345";
            yield return new Claim(ClaimTypes.NameIdentifier, userId);
            yield return new Claim(ClaimTypes.Upn, $"{userId}@email.test");
            yield return new Claim(ClaimTypes.Email, $"{userId}@someemail.test");
        }
    }
}

#endif