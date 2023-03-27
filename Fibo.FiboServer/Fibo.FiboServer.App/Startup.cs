using EasyNetQ;
using Fibo.FiboServer.App.Config;
using Fibo.FiboServer.App.Services;

namespace Fibo.FiboServer.App
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ServicesConfiguration>(Configuration.GetSection("Services"));
            var messageBusConnectionString = Configuration.GetSection("Services:MessageBusConnectionString").Get<string>();

            services
                .AddScoped<IFiboService, FiboService>()
                .AddScoped<IBus>((provider) => RabbitHutch.CreateBus(messageBusConnectionString)); // на каждый запрос к серверу, чтобы избежать отваливающегося коннекта к Message Bus

            services.AddControllers();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();

            //if (app.Environment.IsDevelopment())
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.EnableDeepLinking();
            });
            //}

            app.UseRouting();

            // app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
