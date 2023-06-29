
using Npgsql;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using WebApi.Diagnostics;

namespace WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            Action<ResourceBuilder> appResourceBuilder = resource => resource.AddService(DiagnosticsConfig.ServiceName);

            builder.Services.AddOpenTelemetry()
                .ConfigureResource(resourceBuilder =>
                    resourceBuilder
                        .AddService(DiagnosticsConfig.ServiceName)
                        .AddAttributes(new List<KeyValuePair<string, object>>
                        {
                            new("my-attribute", "my-value")
                        })
                )
                .WithTracing(configure =>
                {
                    configure
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddSqlClientInstrumentation()
                        .AddNpgsql()
                        .AddEntityFrameworkCoreInstrumentation()
                        .AddSource(DiagnosticsConfig.ActivitySource.Name)
                        .ConfigureResource(appResourceBuilder)
                        .AddConsoleExporter();

                    configure.AddOtlpExporter();
                })
                .WithMetrics(configure =>
                {
                    configure
                        .AddMeter(DiagnosticsConfig.Meter.Name)
                        .ConfigureResource(appResourceBuilder)
                        .AddOtlpExporter()
                        .AddConsoleExporter();
                });

            builder.Services.AddLogging(l =>
            {
                l.AddOpenTelemetry(configure =>
                {
                    var resourceBuilder = ResourceBuilder.CreateDefault();
                    appResourceBuilder(resourceBuilder);
                    configure
                        .SetResourceBuilder(resourceBuilder)
                        .AddOtlpExporter()
                        .AddConsoleExporter();
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}