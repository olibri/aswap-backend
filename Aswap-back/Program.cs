using Aswap_back.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var host = RootBuilder.GetHost();
await host.RunAsync();