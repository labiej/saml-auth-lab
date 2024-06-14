using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.MvcCore.Configuration;
using ITfoxtec.Identity.Saml2.Schemas.Metadata;
using ITfoxtec.Identity.Saml2.Util;

using SamlAuthLab.IdentityProvider.Cache;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();


builder.Services.AddControllersWithViews();

builder.Services.Configure<Saml2Configuration>(builder.Configuration.GetSection("Saml2"))
    .Configure<Saml2Configuration>(samlConfig =>
    {
        var serviceProviderMetadataFile = builder.Configuration["Saml2:SPMetadata"];

        var entityDescriptor = new EntityDescriptor();
        entityDescriptor.ReadSPSsoDescriptorFromFile(serviceProviderMetadataFile);

        if (entityDescriptor.SPSsoDescriptor is null)
            throw new Exception("SPSsoDescriptor not loaded from metadata.");

        samlConfig.SigningCertificate = CertificateUtil.Load(builder.Configuration["Saml2:SigningCertificateFile"], builder.Configuration["Saml2:SigningCertificatePassword"]);
        
    });

builder.Services.AddSingleton<SamlRequestStore>();

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

app.UseSaml2();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

app.UseSession();

app.Run();
