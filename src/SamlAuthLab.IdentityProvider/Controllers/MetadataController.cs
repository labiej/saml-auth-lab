namespace SamlAuthLab.IdentityProvider.Controllers
{
    using System.Diagnostics;
    using System.Security.Cryptography.X509Certificates;
    using System.Text.Json;

    using ITfoxtec.Identity.Saml2;
    using ITfoxtec.Identity.Saml2.MvcCore;
    using ITfoxtec.Identity.Saml2.Schemas;
    using ITfoxtec.Identity.Saml2.Schemas.Metadata;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;

    public class MetadataController : Controller
    {
        private readonly Saml2Configuration config;
        public MetadataController(IOptions<Saml2Configuration> config)
        {
            this.config = config.Value;
        }

        public IActionResult Index()
        {
            var entityDescriptor = new EntityDescriptor(config)
            {
                IdPSsoDescriptor = new IdPSsoDescriptor
                {
                    WantAuthnRequestsSigned = config.SignAuthnRequest,
                    SigningCertificates = new X509Certificate2[] { config.SigningCertificate },
                    SingleSignOnServices = new SingleSignOnService[]
                    {
                        new SingleSignOnService { Binding = ProtocolBindings.HttpRedirect, Location = config.SingleSignOnDestination }
                    },
                    NameIDFormats = new Uri[] { NameIdentifierFormats.X509SubjectName },
                }
            };

            if (config.SigningCertificate == null)
            {
                return new ContentResult
                {
                    ContentType = "text/xml",
                    Content = string.Empty,
                };
            }

            //Debug.WriteLine(JsonSerializer.Serialize(entityDescriptor));

            return new Saml2Metadata(entityDescriptor).CreateMetadata().ToActionResult();
        }
    }
}
