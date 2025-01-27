﻿using Hangfire;
using Hangfire.Console;
using Hangfire.Console.Extensions;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Newtonsoft.Json;
using Shared.Configurations;
using System.Security.Authentication;

namespace Infrastructure.ScheduledJobs;

public static class HangfireExtensions
{
    public static IServiceCollection AddHangfireService(this IServiceCollection services) 
    {
        var settings = services.GetOptions<HangfireSettings>("HangfireSettings");
        if (settings == null || settings.Storage == null || string.IsNullOrEmpty(settings.Storage.ConnectionString))
            throw new Exception("HangfireSettings is not configured properly!");

        services.ConfigureHangfireServices(settings);
        services.AddHangfireServer(serverOptions => { serverOptions.ServerName = settings.ServerName; });

        return services;
    }

    private static IServiceCollection ConfigureHangfireServices(this IServiceCollection services, HangfireSettings settings)
    {
        switch (settings.Storage.DBProvider.ToLower())
        {
            case "mongodb":
                var mongoUrlBuilder = new MongoUrlBuilder(settings.Storage.ConnectionString)
                {
                    AuthenticationSource = "admin"
                };

                var mongoClientSettings = MongoClientSettings.FromUrl(new MongoUrl(settings.Storage.ConnectionString));
                mongoClientSettings.SslSettings = new SslSettings
                {
                    EnabledSslProtocols = SslProtocols.Tls12
                };

                var mongoClient = new MongoClient(mongoClientSettings);

                var mongoStorageOptions = new MongoStorageOptions
                {
                    MigrationOptions = new MongoMigrationOptions
                    {
                        MigrationStrategy = new MigrateMongoMigrationStrategy(),
                        BackupStrategy = new CollectionMongoBackupStrategy()
                    },
                    CheckConnection = true,
                    Prefix = "ScheduledQueue",
                    CheckQueuedJobsStrategy = CheckQueuedJobsStrategy.TailNotificationsCollection
                };

                services.AddHangfire((provider, config) =>
                {
                    config.UseSimpleAssemblyNameTypeSerializer()
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                    .UseRecommendedSerializerSettings()
                    //.UseConsole()
                    .UseMongoStorage(mongoClient, mongoUrlBuilder.DatabaseName, mongoStorageOptions);

                    var jsonSettings = new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All
                    };
                    config.UseSerializerSettings(jsonSettings);
                });

                services.AddHangfireConsoleExtensions();
                break;

            case "postgresql":
                break;

            case "mssql":
                break;

            default:
                throw new Exception($"Hangfire Storage Provider {settings.Storage.DBProvider} is not supported.");
        }

        return services;
    }
}
