using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.MvcCore.Configuration;
using SamlAuthLab.IdentityProvider.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.Configure<Settings>(builder.Configuration.GetSection("Settings"));
builder.Services.AddOptions<Saml2Configuration>()
    .Bind(builder.Configuration.GetSection("Saml2"))
    .Validate(cfg => {

        return true;
    }, "Certificate must be valid");
//builder.Services.BindConfig<Saml2Configuration>(Configuration, "Saml2", (serviceProvider, saml2Configuration) =>
//{
//    //saml2Configuration.SignAuthnRequest = true;
//    saml2Configuration.SigningCertificate = CertificateUtil.Load(AppEnvironment.MapToPhysicalFilePath(Configuration["Saml2:SigningCertificateFile"]), Configuration["Saml2:SigningCertificatePassword"], X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);
//    if (!saml2Configuration.SigningCertificate.IsValidLocalTime())
//    {
//        throw new Exception("The IdP signing certificates has expired.");
//    }
//    saml2Configuration.AllowedAudienceUris.Add(saml2Configuration.Issuer);

//    return saml2Configuration;
//});

builder.Services.AddSaml2();

var app = builder.Build();


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

app.UseAuthorization();

app.MapRazorPages();

app.Run();
