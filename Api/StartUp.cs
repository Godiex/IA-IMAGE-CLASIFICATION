using Api.Filters;
using Infrastructure.Context;
using Infrastructure.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.ML;
using Prometheus;
using Serilog;
using System.Reflection;

namespace Api
{
    public static class StartUp
    {
        public static WebApplication StartApp(string[] args) 
        {
            var builder = WebApplication.CreateBuilder(args);
            
            ConfigureServices(builder);
            var app = builder.Build();
            Configure(app, builder.Environment);
            return app;
        }

        private static void ConfigureServices(WebApplicationBuilder builder) 
        {
            var config = builder.Configuration;
            builder.Services.AddControllers(opts =>
            {
                opts.Filters.Add(typeof(AppExceptionFilterAttribute));
            });

            builder.Services.AddSingleton(_ =>
            {
                MLContext mlContext = new MLContext(seed: 0);
                return mlContext;
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddMediatR(Assembly.Load("Application"), typeof(StartUp).Assembly);
            builder.Services.AddAutoMapper(Assembly.Load("Application"));

            builder.Services.AddDbContext<PersistenceContext>(opt =>
            {
                opt.UseSqlServer(config.GetConnectionString("database"), sqlopts =>
                {
                    sqlopts.MigrationsHistoryTable("_MigrationHistory", config.GetValue<string>("SchemaName"));
                });
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });

            builder.Services.AddHealthChecks().AddSqlServer(config["ConnectionStrings:database"]);

            builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

            builder.Services.AddPersistence(config).AddDomainServices().AddRabbitSupport(config);

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "Block Api", Version = "v1" });
            });

        }

        private static void Configure(WebApplication app, IWebHostEnvironment env) 
        {
            Log.Logger = new LoggerConfiguration().Enrich.FromLogContext()
                .WriteTo.Console().CreateLogger();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Block Api"));
            }

            app.UseCors("CorsPolicy");

            app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(env.ContentRootPath, "Data")),
                RequestPath = "/Data"
            });

            app.UseDirectoryBrowser(new DirectoryBrowserOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(env.ContentRootPath, "Data")),
                RequestPath = "/Data"
            });

            app.UseRouting().UseHttpMetrics().UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/IA/blockversion", () => new { version = 1.0, by = "Diego Villa" });
                endpoints.MapMetrics();
                endpoints.MapHealthChecks("/health");
            });

            app.UseHttpLogging();
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }

    }
}
