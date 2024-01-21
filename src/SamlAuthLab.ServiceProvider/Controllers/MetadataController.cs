namespace SamlAuthLab.ServiceProvider.Controllers;

using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.MvcCore;
using ITfoxtec.Identity.Saml2.Schemas;
using ITfoxtec.Identity.Saml2.Schemas.Metadata;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using SamlAuthLab.ServiceProvider.Data;

public class MetadataController : Controller
{
    private readonly Saml2Configuration config;
    public MetadataController(IOptions<Saml2Configuration> config)
    {
        this.config = config.Value;
    }

    [AllowAnonymous]
    [Route("Metadata")]
    public IActionResult Index()
    {
        var defaultSite = new Uri($"{Request.Scheme}://{Request.Host.ToUriComponent()}/");

        var entityDescriptor = new EntityDescriptor(config)
        {
            SPSsoDescriptor = new SPSsoDescriptor
            {
                WantAssertionsSigned = true,
                NameIDFormats = new Uri[] { NameIdentifierFormats.X509SubjectName },
                AssertionConsumerServices = new AssertionConsumerService[]
                {
                    new AssertionConsumerService { Binding = ProtocolBindings.HttpPost, Location = new Uri(defaultSite, "Auth/ConsumeAssertion"), IsDefault = true },
                    new AssertionConsumerService { Binding = ProtocolBindings.HttpPost, Location = new Uri(defaultSite, "Auth/ConsumeAssertionWithoutChecks"), IsDefault = false },
                    new AssertionConsumerService { Binding = ProtocolBindings.HttpPost, Location = new Uri(defaultSite, "Auth/ConsumeAssertionAllowUnsolicited"), IsDefault = false },
                }
            }
        };
        return new Saml2Metadata(entityDescriptor).CreateMetadata().ToActionResult();
    }
}
