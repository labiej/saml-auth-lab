using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.MvcCore.Configuration;
using ITfoxtec.Identity.Saml2.Schemas.Metadata;

using SamlAuthLab.ServiceProvider.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.Configure<Saml2Configuration>(builder.Configuration.GetSection("Saml2"))
    .Configure<Saml2Configuration>(saml2Configuration =>
    {
        saml2Configuration.AllowedAudienceUris.Add(saml2Configuration.Issuer);

        var metadataFile = builder.Configuration["Saml2:IdpMetadata"];
        var entityDescriptor = new EntityDescriptor();
        entityDescriptor.ReadIdPSsoDescriptorFromFile(metadataFile);

        if (entityDescriptor.IdPSsoDescriptor == null)
            throw new Exception("IdPSsoDescriptor not loaded from metadata.");

        saml2Configuration.AllowedIssuer = entityDescriptor.EntityId;
        saml2Configuration.SingleSignOnDestination = entityDescriptor.IdPSsoDescriptor.SingleSignOnServices.First().Location;

        foreach (var signingCertificate in entityDescriptor.IdPSsoDescriptor.SigningCertificates)
        {
            if (signingCertificate.IsValidLocalTime())
            {
                saml2Configuration.SignatureValidationCertificates.Add(signingCertificate);
            }
        }

        if (saml2Configuration.SignatureValidationCertificates.Count <= 0)
            throw new Exception("The IdP signing certificates has expired.");

        if (entityDescriptor.IdPSsoDescriptor.WantAuthnRequestsSigned.HasValue)
        {
            saml2Configuration.SignAuthnRequest = entityDescriptor.IdPSsoDescriptor.WantAuthnRequestsSigned.Value;
        }
    });

builder.Services.AddOptions<UserStoreOptions>(UserStoreOptions.Name)
    .Bind(builder.Configuration.GetSection(UserStoreOptions.Name))
    .ValidateOnStart();

builder.Services.AddMemoryCache();
builder.Services.AddSession(opt => { });

builder.Services.AddSaml2();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSaml2();

app.UseSession();
app.UseAuthorization();

app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

app.Run();
