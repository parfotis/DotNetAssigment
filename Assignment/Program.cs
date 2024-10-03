using Hangfire;
using Hangfire.SqlServer;
using static Microsoft.AspNetCore.Http.StatusCodes;
using Microsoft.EntityFrameworkCore;
using Assignment.Configurations;
using Assignment.Data;
using Assignment.Services;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        //Swagger
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        //Hangfire
        var ipUpdateJobConfiguration = new IpUpdateJobConfiguration();
        builder.Configuration.GetSection("IpUpdateJob").Bind(ipUpdateJobConfiguration);
        builder.Services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(builder.Configuration.GetConnectionString("IntegrationDBConnection"), new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = ipUpdateJobConfiguration.CommandBatchMaxTimeout,
                SlidingInvisibilityTimeout = ipUpdateJobConfiguration.SlidingInvisibilityTimeout,
                QueuePollInterval = ipUpdateJobConfiguration.QueuePollInterval,
                UseRecommendedIsolationLevel = ipUpdateJobConfiguration.UseRecommendedIsolationLevel,
                DisableGlobalLocks = ipUpdateJobConfiguration.DisableGlobalLocks
            }
        ));
        builder.Services.AddHangfireServer();

        //Controllers
        builder.Services.AddControllers();

        //Configurations
        builder.Services.Configure<IpUpdateJobConfiguration>(builder.Configuration.GetSection("IpUpdateJob"));
        builder.Services.Configure<IpLookupServiceConfiguration>(builder.Configuration.GetSection("IpLookupService"));

        //Database
        var databaseConfiguration = new DatabaseConfiguration();
        builder.Configuration.GetSection("Database").Bind(databaseConfiguration);
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("IPDBConnection"),
                sqlOptions => { sqlOptions.CommandTimeout(databaseConfiguration.CommandTimeout); }
            ));

        //Services
        builder.Services.AddMemoryCache();
        builder.Services.AddHttpClient<ExternalIpLookupService>();
        builder.Services.AddScoped<IIpLookupService, ExternalIpLookupService>();
        builder.Services.AddScoped<DbIpUpdateService>();
        builder.Services.AddScoped<IIpLookupServiceFactory, IpLookupServiceFactory>();
        builder.Services.AddScoped<IIpReportService, IpReportService>();

        //HTTPS
        builder.WebHost.ConfigureKestrel(serverOptions =>
        {
            serverOptions.ListenAnyIP(5258);
            serverOptions.ListenAnyIP(5259, listenOptions =>
            {
                listenOptions.UseHttps();
            });
        });
        builder.Services.AddHttpsRedirection(options => 
            {
                options.RedirectStatusCode = Status307TemporaryRedirect;
                options.HttpsPort = 5259;
            }
        );
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAllOrigins", builder =>
            {
                builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        var app = builder.Build();

        app.UseStaticFiles();
        app.UseRouting(); 
        app.UseHttpsRedirection();
        app.UseCors("AllowAllOrigins");

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                c.RoutePrefix = "swagger";
            });
        }

        app.UseHangfireDashboard();

        app.MapControllers();

        //IP Update Scheduled Job setup
        RecurringJob.AddOrUpdate<DbIpUpdateService>(
            "update-ip-addresses",
            service => service.UpdateIpInfo(),
            ipUpdateJobConfiguration.ExecutionInterval
        );

        app.Run();
    }
}