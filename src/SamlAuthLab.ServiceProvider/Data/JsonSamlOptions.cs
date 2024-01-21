namespace SamlAuthLab.ServiceProvider.Data;

using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.Schemas.Metadata;

using Microsoft.Extensions.Options;

public class JsonSamlOptions : Saml2Configuration, IPostConfigureOptions<JsonSamlOptions>
{
    public JsonSamlOptions()
    {
    }
    public const string Name = "Saml2";

    public string IdpMetadata { get; set; } = string.Empty;

    public void PostConfigure(string? name, JsonSamlOptions options)
    {
        var entityDescriptor = new EntityDescriptor();
        entityDescriptor.ReadIdPSsoDescriptorFromFile(options.IdpMetadata);

        if (entityDescriptor.IdPSsoDescriptor == null)
            throw new Exception("IdPSsoDescriptor not loaded from metadata.");

        options.AllowedIssuer = entityDescriptor.EntityId;
        options.SingleSignOnDestination = entityDescriptor.IdPSsoDescriptor.SingleSignOnServices.First().Location;

        foreach (var signingCertificate in entityDescriptor.IdPSsoDescriptor.SigningCertificates)
        {
            if (signingCertificate.IsValidLocalTime())
            {
                options.SignatureValidationCertificates.Add(signingCertificate);
            }
        }

        if (options.SignatureValidationCertificates.Count <= 0)
            throw new Exception("The IdP signing certificates has expired.");

        if (entityDescriptor.IdPSsoDescriptor.WantAuthnRequestsSigned.HasValue)
        {
            options.SignAuthnRequest = entityDescriptor.IdPSsoDescriptor.WantAuthnRequestsSigned.Value;
        }
    }
}
