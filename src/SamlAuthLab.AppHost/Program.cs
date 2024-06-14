var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.SamlAuthLab_IdentityProvider>("samlauthlab-identityprovider");

builder.AddProject<Projects.SamlAuthLab_ServiceProvider>("samlauthlab-serviceprovider");

builder.Build().Run();
