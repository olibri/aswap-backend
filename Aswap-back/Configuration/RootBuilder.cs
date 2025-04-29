using Autofac.Extensions.DependencyInjection;
using System.Text;
using App;
using Aswap_back.Controllers;
using Aswap_back.Middleware;
using Autofac;
using Domain.Interfaces.Database.Command;
using Domain.Interfaces.Database.Queries;
using Infrastructure;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;

namespace Aswap_back.Configuration;

public class RootBuilder
{
    public static IHost GetHost()
    {
        //Log.Logger = new LoggerConfiguration()
        //    .MinimumLevel.Information()
        //    .WriteTo.Console()
        //    .Enrich.WithProperty("Environment", Global.Environment)
        //    .CreateLogger();

        //var publicKeyPem = consulConfiguration.GetConfigurationValue(ConfigurationKeys.JwtPublicKey.Source);
        //var rsa = RSA.Create();
        //rsa.ImportFromPem(publicKeyPem.ToCharArray());
        //var key = new RsaSecurityKey(rsa);

        var secretKey = "qwertyuiopasdfghjklzxcvbnm123456";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        var host = Host.CreateDefaultBuilder()
            .UseServiceProviderFactory(new AutofacServiceProviderFactory())
            .ConfigureContainer<ContainerBuilder>(builder =>
            {
                builder.RegisterType<MarketController>().InstancePerDependency();

                builder.RegisterType<MarketDbCommand>().As<IMarketDbCommand>().SingleInstance();
                builder.RegisterType<MarketDbQueries>().As<IMarketDbQueries>().SingleInstance();


                builder.Register(ctx =>
                    {
                        var config = ctx.Resolve<IConfiguration>();

                        var opt = new DbContextOptionsBuilder<P2PDbContext>()
                            .UseNpgsql(config.GetConnectionString("PgDatabase"))
                            .Options;

                        return new P2PDbContext(opt);
                    }).AsSelf()
                    .InstancePerLifetimeScope();

            })

            .ConfigureServices((ctx, services) =>
            {
                var cfg = ctx.Configuration;


                services.AddControllers();
                services.AddEndpointsApiExplorer();
                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "api/Payment", Version = "v1" });
                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Description =
                            "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey
                    });
                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            []
                        }
                    });
                });
                
                services.AddDbContext<P2PDbContext>(opt =>
                    opt.UseNpgsql(cfg.GetConnectionString("PgDatabase")));

                services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            //ValidIssuer = issuer,
                            //ValidAudience = audience,
                            IssuerSigningKey = key,
                            NameClaimType = "userName"
                        };
                    });
                services.AddAuthorization();
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.Configure(app =>
                {
                    app.UseMiddleware<ErrorHandlingMiddleware>();

                    app.UseSwagger();
                    app.UseSwaggerUI();

                    app.UseHttpsRedirection();
                    app.UseRouting();

                    app.UseAuthentication();
                    app.UseAuthorization();


                    app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
                });
            })
            .Build();

        using var scope = host.Services.CreateScope();
        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<P2PDbContext>();
            dbContext.Database.Migrate();
            //Log.Information("Database migrated successfully.");
        }
        catch (Exception e)
        {
            //Log.Error(e, "An error occurred while migrating the database.");
            throw;
        }

        return host;
    }
}