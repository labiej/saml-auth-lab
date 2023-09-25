namespace SamlAuthLab.IdentityProvider.Controllers
{
    using ITfoxtec.Identity.Saml2;
    using ITfoxtec.Identity.Saml2.MvcCore;
    using ITfoxtec.Identity.Saml2.Schemas;
    using ITfoxtec.Identity.Saml2.Schemas.Metadata;
    using Microsoft.AspNetCore.Mvc;
    using System.Security.Cryptography.X509Certificates;

    public class MetadataController : Controller
    {
        private readonly Saml2Configuration config;
        public MetadataController(Saml2Configuration config)
        {
            this.config = config;
        }

        public IActionResult Index()
        {
            var entityDescriptor = new EntityDescriptor(config)
            {
                ValidUntil = 365,
                IdPSsoDescriptor = new IdPSsoDescriptor
                {
                    WantAuthnRequestsSigned = config.SignAuthnRequest,
                    SigningCertificates = new X509Certificate2[]
                    {
                        config.SigningCertificate
                    },
                    SingleSignOnServices = new SingleSignOnService[]
                    {
                        new SingleSignOnService { Binding = ProtocolBindings.HttpRedirect, Location = config.SingleSignOnDestination }
                    },
                    SingleLogoutServices = new SingleLogoutService[]
                    {
                        new SingleLogoutService { Binding = ProtocolBindings.HttpPost, Location = config.SingleLogoutDestination }
                    },
                    NameIDFormats = new Uri[] { NameIdentifierFormats.X509SubjectName },
                    Attributes = new SamlAttribute[]
                    {
                        new SamlAttribute("urn:oid:1.3.6.1.4.1.5923.1.1.1.6", friendlyName: "eduPersonPrincipalName"),
                        new SamlAttribute("urn:oid:1.3.6.1.4.1.5923.1.1.1.1", new string[] { "member", "student", "employee" })
                    }
                }
            };
            return new Saml2Metadata(entityDescriptor).CreateMetadata().ToActionResult();
        }
    }
}
