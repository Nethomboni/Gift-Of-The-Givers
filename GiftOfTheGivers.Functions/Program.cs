using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;

// ConfigureFunctionsWorkerDefaults() is the classic isolated-worker bootstrap
// (HttpRequestData/HttpResponseData functions, no extra ASP.NET Core
// integration package required) - the most broadly compatible option for a
// straightforward HTTP-triggered Function App like this one.
var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .Build();

host.Run();
