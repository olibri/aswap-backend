using System.Net.Mime;
using System.Text;
using App.Chat;
using App.CoinJelly;
using App.Db;
using App.Metrics.Api;
using App.Metrics.BackgroundWorker;
using App.Metrics.TaskMetrics;
using App.Parsing;
using App.Services.Accounts;
using App.Services.Auth;
using App.Services.Auth.NetworkVerifier;
using App.Services.CoinPrice;
using App.Services.CoinPrice.Jobs;
using App.Services.CoinPrice.Jupiter;
using App.Services.CoinPrice.Planner;
using App.Services.CoinPrice.Swap;
using App.Services.CoinPrice.TokenRepo;
using App.Services.CoinPrice.Workers;
using App.Services.Notification;
using App.Services.Order;
using App.Services.PaymentMethod;
using App.Services.PaymentMethod.BackgroundWorker;
using App.Services.Sessions;
using App.Strategy;
using App.Strategy.EventsHandler;
using App.Telegram;
using App.Utils.Web.IP;
using Aswap_back.Controllers;
using Aswap_back.Middleware;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Domain.Interfaces;
using Domain.Interfaces.Chat;
using Domain.Interfaces.CoinJelly;
using Domain.Interfaces.Database.Command;
using Domain.Interfaces.Database.Queries;
using Domain.Interfaces.Hooks.Parsing;
using Domain.Interfaces.Metrics;
using Domain.Interfaces.Services;
using Domain.Interfaces.Services.Account;
using Domain.Interfaces.Services.Auth;
using Domain.Interfaces.Services.CoinService;
using Domain.Interfaces.Services.CoinService.Jobs;
using Domain.Interfaces.Services.CoinService.Jupiter;
using Domain.Interfaces.Services.CoinService.Planner;
using Domain.Interfaces.Services.CoinService.TokenRepo;
using Domain.Interfaces.Services.IP;
using Domain.Interfaces.Services.Notification;
using Domain.Interfaces.Services.Order;
using Domain.Interfaces.Services.PaymentMethod;
using Domain.Interfaces.Strategy;
using Domain.Interfaces.TelegramBot;
using Domain.Models;
using Domain.Models.Api.Auth;
using Domain.Models.Api.CoinPrice;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Telegram.Bot;
using Unchase.Swashbuckle.AspNetCore.Extensions.Extensions;

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
        builder.RegisterType<NonceService>().As<INonceService>().InstancePerDependency();
        builder.RegisterType<SignatureVerifier>().As<ISignatureVerifier>().InstancePerDependency();

        builder.RegisterType<EscrowInitializedHandler>().As<IAnchorEventHandler>().InstancePerDependency();
        builder.RegisterType<OfferInitializedHandler>().As<IAnchorEventHandler>().InstancePerDependency();

        builder.RegisterType<AnchorAnchorEventParser>().As<IAnchorEventParser>().InstancePerDependency();

        builder.RegisterType<OrderMetricsTask>().As<IPeriodicTask>().InstancePerLifetimeScope();
        builder.RegisterType<OutboxProcessorTask>().As<IPeriodicTask>().InstancePerLifetimeScope();
        builder.RegisterType<TvlSnapshotTask>().As<IPeriodicTask>().InstancePerLifetimeScope();
        builder.RegisterType<OrderStatusSnapshotTask>().As<IPeriodicTask>().InstancePerLifetimeScope();
        builder.RegisterType<TradeMetricsTask>().As<IPeriodicTask>().InstancePerLifetimeScope();
        builder.RegisterType<UserMetricsDailyTask>().As<IPeriodicTask>().InstancePerLifetimeScope();
        builder.RegisterType<SessionCleanupTask>().As<IPeriodicTask>().InstancePerLifetimeScope();
        builder.RegisterType<RatingService>().As<IRatingService>().InstancePerLifetimeScope();
        builder.RegisterType<BestPriceService>().As<IBestPriceService>().InstancePerLifetimeScope();
        builder.RegisterType<AdminMetricsService>().As<IAdminMetricsService>().InstancePerLifetimeScope();
        builder.RegisterType<CoinJellyService>().As<ICoinJellyService>().InstancePerLifetimeScope();
        builder.RegisterType<CoinService>().As<ICoinService>().InstancePerLifetimeScope();
        builder.RegisterType<SwapService>()
          .As<ISwapService>()
          .InstancePerLifetimeScope();

        builder.RegisterType<NotificationService>()
          .As<INotificationService>()
          .InstancePerLifetimeScope();

        builder.RegisterType<ChildOffersService>()
          .As<IChildOffersService>()
          .InstancePerLifetimeScope();

        builder.RegisterType<CoinGeckoPriceValidatorService>()
          .As<IPriceValidatorService>()
          .SingleInstance();

        builder.Register(ctx =>
            ctx.Resolve<IDbContextFactory<P2PDbContext>>().CreateDbContext())
          .As<P2PDbContext>()
          .InstancePerLifetimeScope();

        // Repositories
        builder.RegisterType<TokenRepository>()
          .As<ITokenRepository>()
          .InstancePerLifetimeScope();

        builder.RegisterType<PriceSnapshotRepository>()
          .As<IPriceSnapshotRepository>()
          .InstancePerLifetimeScope();

        builder.RegisterType<AppLockService>()
          .As<IAppLockService>()
          .InstancePerLifetimeScope();

        builder.RegisterType<PriceBatchPlanner>()
          .As<IPriceBatchPlanner>()
          .SingleInstance();

        // Jobs
        builder.RegisterType<TokenBootstrapJob>()
          .As<ITokenBootstrapJob>()
          .InstancePerDependency();

        builder.RegisterType<MinutePriceIngestJob>()
          .As<IMinutePriceIngestJob>()
          .InstancePerDependency();

        builder.RegisterType<DailyPriceRetentionJob>()
          .As<IDailyPriceRetentionJob>()
          .InstancePerDependency();

        // Facade
        builder.RegisterType<PriceIngestFacade>()
          .As<IPriceIngestFacade>()
          .InstancePerLifetimeScope();
        builder.RegisterType<RefreshTokenService>()
          .As<IRefreshTokenService>()
          .InstancePerLifetimeScope();


        builder.RegisterType<SolSignatureVerifier>()
          .As<INetworkVerifier>()
          .SingleInstance();

        builder.RegisterType<CurrencyCatalog>().As<ICurrencyCatalog>().SingleInstance();

        builder.RegisterType<EthSignatureVerifier>()
          .As<INetworkVerifier>()
          .SingleInstance();


        builder.RegisterType<AccountService>().As<IAccountService>().InstancePerLifetimeScope();
        builder.RegisterType<SessionService>().As<ISessionService>().InstancePerLifetimeScope();

        builder.RegisterType<HttpContextIpAccessor>()
          .As<IClientIpAccessor>()
          .SingleInstance();

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

        builder.RegisterType<GeoIpService>()
          .As<IGeoIpService>()
          .SingleInstance();

        builder.RegisterType<PaymentCatalog>()
          .As<IPaymentCatalog>()
          .As<IHostedService>()
          .SingleInstance();

        builder.RegisterType<PopularityCounter>()
          .As<IPopularityCounter>()
          .As<IHostedService>()
          .SingleInstance();

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
        builder.RegisterType<NotificationController>().InstancePerDependency();
        builder.RegisterType<CoinJellyController>().InstancePerDependency();
        builder.RegisterType<TelegramHookController>().InstancePerDependency();
        builder.RegisterType<AdminController>().InstancePerDependency();
        builder.RegisterType<SessionPingController>().InstancePerDependency();
        builder.RegisterType<AuthController>().InstancePerDependency();
        builder.RegisterType<RatingController>().InstancePerDependency();
        builder.RegisterType<PaymentController>().InstancePerDependency();
        builder.RegisterType<OrderController>().InstancePerDependency();

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
        services.AddMemoryCache();
        services.Configure<TokenOptions>(cfg.GetSection("Jwt"));
        services.AddSingleton<ITokenService, TokenService>();
        services.AddSingleton<TimeProvider>(TimeProvider.System);
        services.AddHttpContextAccessor();

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
          c.EnableAnnotations();
          c.AddEnumsWithValuesFixFilters(o =>
          {
            o.ApplySchemaFilter = true;
            o.ApplyParameterFilter = true;
            o.ApplyDocumentFilter = true;
            o.IncludeDescriptions = true;
            // o.XEnumNamesAlias = "x-enum-varnames"; // якщо треба інший ключ
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
        var priceCfg = new PriceIngestConfig(
          ctx.Configuration["PriceIngest:Quote"] ?? "USDC",
          ctx.Configuration.GetValue<int?>("PriceIngest:PollEveryMinutes") ?? 5,
          ctx.Configuration.GetValue<int?>("PriceIngest:MaxIdsPerRequest") ?? 50,
          ctx.Configuration.GetValue<int?>("PriceIngest:RequestsPerWindow") ?? 60,
          TimeSpan.FromSeconds(ctx.Configuration.GetValue<int?>("PriceIngest:WindowSeconds") ?? 60)
        );
        services.AddSingleton(priceCfg);

        // --- Typed HttpClients
        services.AddHttpClient<IJupTokenClient, JupTokenClient>(c =>
        {
          c.BaseAddress = new Uri("https://lite-api.jup.ag");
          c.Timeout = TimeSpan.FromSeconds(10);
        });

        services.AddHttpClient<IJupPriceClient, JupPriceClient>(c =>
        {
          c.BaseAddress = new Uri("https://lite-api.jup.ag");
          c.Timeout = TimeSpan.FromSeconds(10);
        });

        services.AddHttpClient<IJupiterSwapApi, JupSwap>(c =>
        {
          c.BaseAddress = new Uri("https://lite-api.jup.ag/swap/v1/");
          c.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        });
        services.AddSingleton<IJsonSerializer, SystemTextJsonSerializer>();
        services.AddSingleton<OutboxSaveChangesInterceptor>();
        services.AddHostedService<TokenBootstrapHostedService>();
        services.AddHostedService<MinutePriceIngestHostedService>();
        services.AddHostedService<DailyPriceRetentionHostedService>();

        services.AddDbContextFactory<P2PDbContext>((sp, opt) =>
          opt.UseNpgsql(cfg.GetConnectionString("PgDatabase"))
            .AddInterceptors(sp.GetRequiredService<OutboxSaveChangesInterceptor>()));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
          .AddJwtBearer(options =>
          {
            var jwtOpt = cfg.GetSection("Jwt").Get<TokenOptions>()!;
            options.TokenValidationParameters = new TokenValidationParameters
            {
              ValidIssuer = jwtOpt.Issuer,
              ValidAudience = jwtOpt.Audience,
              IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOpt.SymmetricKey)),
              ValidateIssuer = true,
              ValidateAudience = true,
              ValidateLifetime = true,
              ValidateIssuerSigningKey = true,
              ClockSkew = TimeSpan.FromSeconds(30)
            };
          });
        services.AddAuthorization();
      })
      .Build();

    using var scope = host.Services.CreateScope();
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<P2PDbContext>>();
    using var db = factory.CreateDbContext();

    try
    {
      db.Database.Migrate();
    }
    catch (Exception e)
    {
      //Log.Error(e, "An error occurred while migrating the database.");
      throw;
    }

    return host;
  }
}