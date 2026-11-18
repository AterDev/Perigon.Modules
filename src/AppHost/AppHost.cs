using AppHost;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Configuration;
using Perigon.AspNetCore.Constants;

var builder = DistributedApplication.CreateBuilder(args);
builder.AddKubernetesEnvironment("k8s");
var aspireSetting = AppSettingsHelper.LoadAspireSettings(builder.Configuration);
var isTesting = builder.Configuration["ASPIRE_ENVIRONMENT"]?.ToLowerInvariant() == "testing";

#region infrastructure
var database = builder.AddConnectionString(AppConst.Default);
var cache = builder.AddConnectionString(AppConst.Cache);

#endregion

#region services
var serviceGroup = builder.AddGroup("Services", "Globe");
var apiService = builder.AddProject<Projects.ApiService>("ApiService")
    .WithParentRelationship(serviceGroup);
builder.AddJavaScriptApp(
        "frontend",
        "../ClientApp/WebApp",
        runScriptName: "start")
    .WithPnpm(installArgs: ["--frozen-lockfile"])
    .WithUrl("http://localhost:4200")
    .WithReference(apiService)
    .WaitFor(apiService)
    .WithParentRelationship(serviceGroup);

apiService.WithReference(database).WaitFor(database);
if (cache != null)
{
    apiService.WithReference(cache);
}

#endregion

builder.Build().Run();
