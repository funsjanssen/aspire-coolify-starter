var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("docker-compose")
    .WithProperties(options =>
    {
        options.DefaultNetworkName = null;
    })
    .ConfigureComposeFile(options =>
    {
        options.Networks.Clear();
        foreach (var service in options.Services)
        {
            service.Value.Networks.Clear();
            service.Value.PullPolicy = "always";
        }
    });

var apiService = builder.AddProject<Projects.AspireCoolifyStarter_ApiService>("apiservice")
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.AspireCoolifyStarter_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
