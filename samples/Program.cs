using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Samples;

var webHost = Host
    .CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(_ => _.UseStartup<Startup>())
    .Build();

webHost.Run();
