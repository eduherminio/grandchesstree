
using GrandChessTree.Api.Database;
using GrandChessTree.Api.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Console;

namespace GrandChessTree.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(5032); // Listens on all available network interfaces on port 5032
            });

            // Add services to the container.
            var services = builder.Services;
            services.AddSingleton<TimeProvider>(TimeProvider.System);

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddLogging(b =>
            {
                b.SetMinimumLevel(LogLevel.Information);
                b.AddSimpleConsole(c =>
                {
                    c.IncludeScopes = false;
                    c.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
                    c.ColorBehavior = LoggerColorBehavior.Enabled;
                });
            });

            var app = builder.Build();
            app.UseMiddleware<RequestTimingMiddleware>();

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
