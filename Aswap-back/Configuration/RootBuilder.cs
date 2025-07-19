using App.Chat;
using App.Db;
using App.Metrics.BackgroundWorker;
using App.Metrics.TaskMetrics;
using App.Parsing;
using App.Strategy;
using App.Strategy.EventsHandler;
using App.Telegram;
using Aswap_back.Controllers;
using Aswap_back.Middleware;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Domain.Interfaces;
using Domain.Interfaces.Chat;
using Domain.Interfaces.Database.Command;
using Domain.Interfaces.Database.Queries;
using Domain.Interfaces.Hooks.Parsing;
using Domain.Interfaces.Metrics;
using Domain.Interfaces.Strategy;
using Domain.Interfaces.TelegramBot;
using Domain.Models;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Telegram.Bot;

namespace Aswap_back.Configuration;

public class RootBuilder
{
  public static IHost GetHost()
  {
    var host = Host.CreateDefaultBuilder()
      .UseServiceProviderFactory(new AutofacServiceProviderFactory())
      .ConfigureContainer<ContainerBuilder>((hostContext, builder) =>
      {
        var cfg = hostContext.Configuration;
        builder.RegisterType<MarketDbCommand>().As<IMarketDbCommand>().InstancePerDependency();
        builder.RegisterType<MarketDbQueries>().As<IMarketDbQueries>().InstancePerDependency();
        builder.RegisterType<AccountDbQueries>().As<IAccountDbQueries>().InstancePerDependency();
        builder.RegisterType<ChatDbCommand>().As<IChatDbCommand>().InstancePerDependency();
        builder.RegisterType<AccountDbCommand>().As<IAccountDbCommand>().InstancePerDependency();

        builder.RegisterType<EscrowInitializedHandler>().As<IAnchorEventHandler>().InstancePerDependency();
        builder.RegisterType<OfferInitializedHandler>().As<IAnchorEventHandler>().InstancePerDependency();

        builder.RegisterType<AnchorAnchorEventParser>().As<IAnchorEventParser>().InstancePerDependency();

        builder.RegisterType<OrderMetricsTask>().As<IPeriodicTask>().InstancePerLifetimeScope();
        builder.RegisterType<OutboxProcessorTask>().As<IPeriodicTask>().InstancePerLifetimeScope();
        builder.RegisterType<TvlSnapshotTask>().As<IPeriodicTask>().InstancePerLifetimeScope();
        builder.RegisterType<OrderStatusSnapshotTask>().As<IPeriodicTask>().InstancePerLifetimeScope();
        builder.RegisterType<TradeMetricsTask>().As<IPeriodicTask>().InstancePerLifetimeScope();


        builder.RegisterType<SystemTextJsonSerializer>()
          .As<IJsonSerializer>()
          .SingleInstance();

        builder.RegisterType<OutboxSaveChangesInterceptor>()
          .InstancePerLifetimeScope();

        builder.RegisterType<SchedulerService>()
          .As<IHostedService>()         
          .SingleInstance();

        //Tg bot
        builder.RegisterInstance(cfg).As<IConfiguration>();
        builder.Register(c =>
          {
            var token = cfg["Telegram:BotToken"]
                        ?? throw new ArgumentNullException("Telegram:BotToken");

            return new TelegramBotClient(token);
          })
          .As<ITelegramBotClient>()
          .SingleInstance();

        builder.Register(c =>
          {
            var bot = c.Resolve<ITelegramBotClient>();
            var accountQuery = c.Resolve<IAccountDbQueries>();
            var chatId = long.Parse(cfg["Telegram:AdminChatId"]!);
            var marketDbCommand = c.Resolve<IMarketDbCommand>();


            return new TgBotHandler(bot, chatId, accountQuery, marketDbCommand);
          })
          .As<ITgBotHandler>()
          .SingleInstance();
        //end

        builder.RegisterType<AnchorEventDispatcher>()
          .AsSelf()
          .InstancePerDependency();

        //builder.Register(ctx =>
        //  {
        //    var config = ctx.Resolve<IConfiguration>();

        //    var opt = new DbContextOptionsBuilder<P2PDbContext>()
        //      .UseNpgsql(config.GetConnectionString("PgDatabase"))
        //      .Options;

        //    return new P2PDbContext(opt);
        //  }).AsSelf()
        //  .InstancePerLifetimeScope();

        builder.RegisterType<WebHookController>().InstancePerDependency();
        builder.RegisterType<TelegramHookController>().InstancePerDependency();
        builder.RegisterType<AdminController>().InstancePerDependency();

        builder.RegisterType<PlatformController>().InstancePerDependency();
        builder.RegisterType<ChatController>().InstancePerDependency();
      })
      .ConfigureWebHostDefaults(webBuilder =>
      {
        var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
        webBuilder
          .UseUrls($"http://*:{port}")
          .Configure(app =>
          {
            //app.UseMiddleware<ErrorHandlingMiddleware>();
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseRouting();
            app.UseCors("AllowReactLocalhost8080");

            //app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
          });
      })
      .ConfigureServices((ctx, services) =>
      {
        var cfg = ctx.Configuration;

        services.AddControllers();

        services.AddCors(options =>
        {
          options.AddPolicy("AllowReactLocalhost8080", builder =>
          {
            builder
              .AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
          });
        });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
          c.SwaggerDoc("v1", new OpenApiInfo { Title = "api/Aswap", Version = "v1" });
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

        services.AddSingleton<IJsonSerializer, SystemTextJsonSerializer>();
        services.AddSingleton<OutboxSaveChangesInterceptor>();

        services.AddDbContext<P2PDbContext>((sp, opt) =>
          opt.UseNpgsql(cfg.GetConnectionString("PgDatabase"))
            .AddInterceptors(sp.GetRequiredService<OutboxSaveChangesInterceptor>()));

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
              NameClaimType = "userName"
            };
          });
        services.AddAuthorization();
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