using AppHost;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Kubernetes.Resources;
using Perigon.AspNetCore.Constants;

var builder = DistributedApplication.CreateBuilder(args);
builder.AddKubernetesEnvironment("k8s");
var aspireSetting = AppSettingsHelper.LoadAspireSettings(builder.Configuration);
var isTesting = builder.Configuration["ASPIRE_ENVIRONMENT"]?.ToLowerInvariant() == "testing";
var isMultiTenant = builder.Configuration["Components:IsMultiTenant"] ?? "false";

IResourceBuilder<IResourceWithConnectionString>? cache = null;

// if you have exist resource, you can set connection string here, without create container
// database = builder.AddConnectionString(AppConst.Default);
// nats = builder.AddConnectionString("mq");
// qdrant = builder.AddConnectionString("qdrant");

#region infrastructure
var defaultName = isTesting ? "PerigonModulesTest" : "PerigonModulesDev";
var devPassword = builder.AddParameter(
    "dev-password",
    value: aspireSetting.DevPassword,
    secret: true
);

var infrastructureGroup = builder.AddGroup("Infrastructure", "Cloud");
var postgres = builder
    .AddPostgres(name: "Database", password: devPassword, port: aspireSetting.DbPort)
    .WithImageTag("18.1-alpine");
if (!isTesting)
{
    postgres = postgres.WithDataVolume();
}
var database = postgres.AddDatabase(AppConst.Default, databaseName: defaultName);
_ = aspireSetting.CacheType?.ToLowerInvariant() switch
{
    "memory" => null,
    _ => cache = builder
        .AddRedis("Cache", password: devPassword, port: aspireSetting.CachePort)
        .WithImageTag("8.2-alpine")
        .WithDataVolume()
        .WithPersistence(interval: TimeSpan.FromMinutes(5)),
};

devPassword.WithParentRelationship(infrastructureGroup);
database.WithParentRelationship(infrastructureGroup);
cache?.WithParentRelationship(infrastructureGroup);

#endregion

#region services
var serviceGroup = builder.AddGroup("Services", "Globe");
var adminService = builder.AddProject<Projects.AdminService>("AdminService")
    .WithEnvironment("Components__Cache", aspireSetting.CacheType)
    .WithEnvironment("Components__IsMultiTenant", isMultiTenant)
    .WithParentRelationship(serviceGroup);

var apiService = builder.AddProject<Projects.ApiService>("ApiService")
    .WithReference(adminService)
    .WithEnvironment("Components__Cache", aspireSetting.CacheType)
    .WithEnvironment("Components__IsMultiTenant", isMultiTenant)
    .WithParentRelationship(serviceGroup);

// Angular verification client. It is a local-development resource and is not part of
// the production Kubernetes workload; dependencies are restored by pnpm before startup.
if (!string.Equals(builder.Environment.EnvironmentName, "Production", StringComparison.OrdinalIgnoreCase))
{
    builder.AddJavaScriptApp(
            "frontend",
            "../ClientApp/WebApp",
            runScriptName: "start")
        .WithPnpm(installArgs: ["--frozen-lockfile"])
        .WithUrl("http://localhost:4200")
        .WithReference(adminService)
        .WaitFor(adminService)
        .WithParentRelationship(serviceGroup);
}

apiService.WithReference(database);
adminService.WithReference(database);
if (cache != null)
{
    apiService.WithReference(cache);
    adminService.WithReference(cache);
}

// The migration instance is the same AdminService binary, started in an explicit
// migration-only mode. It must finish successfully before either web service starts.
// The mode is implemented by the service startup and calls Perigon.PostgreSQL's
// migration API; it never calls EnsureCreated or performs an implicit baseline.
var adminMigrations = builder.AddProject<Projects.AdminService>("AdminService-Migrations")
    .WithEnvironment("Components__IsMultiTenant", isMultiTenant)
    .WithEnvironment("Perigon__MigrationMode", "Apply")
    .PublishAsKubernetesService(resource =>
    {
        if (resource.Workload is not Deployment deployment)
        {
            throw new InvalidOperationException(
                "Aspire did not generate a Deployment workload for the PostgreSQL migration resource."
            );
        }

        var job = new KubernetesJobResource
        {
            Metadata = deployment.Metadata,
            Spec = new KubernetesJobSpec
            {
                Template = deployment.Spec.Template
            }
        };

        job.Metadata.Name = "adminservice-migrations-job";
        job.Metadata.Annotations["argocd.argoproj.io/hook"] = "Sync";
        job.Metadata.Annotations["argocd.argoproj.io/sync-wave"] = "-1";
        job.Metadata.Annotations["argocd.argoproj.io/hook-delete-policy"] =
            "BeforeHookCreation,HookSucceeded";
        job.Spec.Template.Spec.RestartPolicy = "OnFailure";

        resource.Workload = null;
        resource.AdditionalResources.Add(job);
    })
    .WithParentRelationship(serviceGroup);

adminMigrations.WithReference(database).WaitFor(database);

apiService.WaitForCompletion(adminMigrations);
adminService.WaitForCompletion(adminMigrations);
#endregion

builder.Build().Run();
