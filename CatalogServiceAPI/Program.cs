using CatalogService.Configurations;
using CatalogService.Repositories;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using NLog;
using NLog.Web;
using CatalogService.Services;

    var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
    logger.Debug("Init main");

    try
    {
    var builder = WebApplication.CreateBuilder(args);

    // Hent miljøvariable
    var token = Environment.GetEnvironmentVariable("VAULT_TOKEN");
    if (string.IsNullOrEmpty(token))
    {
        throw new ApplicationException("VAULT_TOKEN er ikke sat som miljøvariabel.");
    }

    var endPoint = Environment.GetEnvironmentVariable("VaultEndPoint");
    if (string.IsNullOrEmpty(endPoint))
    {
        throw new ApplicationException("VaultEndPoint er ikke sat som miljøvariabel.");
    }

    Console.WriteLine($"VAULT_TOKEN sat til {token}");
    Console.WriteLine($"VaultEndPoint sat til {endPoint}");

    // Hent ConnectionString fra Vault
    var vaultRepository = new VaultRepository(endPoint, token);
    var connectionString = await vaultRepository.GetSecretAsync("ConnectionString");
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new ApplicationException("ConnectionString blev ikke fundet i Vault.");
    }
    Console.WriteLine($"ConnectionString er: {connectionString}");

    // Tilføj ConnectionString til konfigurationen
    builder.Configuration.AddInMemoryCollection(new[]
    {
        new KeyValuePair<string, string>("MongoDbSettings:ConnectionString", connectionString)
    });

    builder.Services.Configure<MongoDbSettings>(
        builder.Configuration.GetSection("MongoDbSettings"));

    builder.Services.AddSingleton<IMongoClient>(sp =>
    {
        var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
        return new MongoClient(settings.ConnectionString);
    });

    builder.Services.AddScoped(typeof(MongoRepository<>)); // Registrer repository før controllerne
    builder.Services.AddHostedService<RabbitMQReceiver>();
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseAuthorization();
    app.MapControllers();
    app.Run();
    }
    catch (Exception ex)
    {
    // Log fejl og afslut programmet
        logger.Error(ex, "Programmet stoppede på grund af en uventet fejl.");
    throw;
    }
    finally
    {
    // Sørg for at rydde op i loggeren
        NLog.LogManager.Shutdown();
    }
