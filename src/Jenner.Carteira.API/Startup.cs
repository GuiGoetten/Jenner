using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;
using Confluent.Kafka;
using Jenner.Carteira.API.Services.Consumer;
using Jenner.Comum;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;

namespace Jenner.Carteira.API
{
    public class Startup
    {

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddMediatR(GetType().Assembly);

            //services.Configure<ReverseProxySettings>(Configuration.GetSection("ReverseProxy"));
            
            services.Configure<ForwardedHeadersOptions>(fwh =>
            {
                fwh.ForwardedHeaders = ForwardedHeaders.All;
            });


            services.AddSingleton(provider => provider.GetRequiredService<IOptions<ReverseProxySettings>>().Value);

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Jenner.Carteira.API", Version = "v1" });
            });

            AddKafkaServices(services);
            AddMongoServices(services);
            
            services.AddHealthChecks();

            services.AddScoped(c =>
            {
                var config = new ConsumerConfig
                {
                    BootstrapServers = Configuration.GetConnectionString(@"KafkaBootstrap"),
                    GroupId = "carteira-worker",
                    AutoOffsetReset = AutoOffsetReset.Earliest
                };
                return new ConsumerBuilder<string, byte[]>(config).Build();
            });

            services.AddHostedService<CarteiraAgendadaWorker>();
            services.AddHostedService<CarteiraAplicadaWorker>();
        }

        private void AddKafkaServices(IServiceCollection services)
        {
            services.AddSingleton(c =>
            {
                var config = new ProducerConfig
                {
                    BootstrapServers = Configuration.GetConnectionString(@"KafkaBootstrap")
                };
                return new ProducerBuilder<string, byte[]>(config).Build();
            });
            services.AddSingleton<CloudEventFormatter>(new JsonEventFormatter());
        }

        private void AddMongoServices(IServiceCollection services)
        {
            services.AddSingleton(_ =>
            {
                return new MongoClient(Configuration.GetConnectionString(Constants.MongoConnectionString));
            });
            services.AddSingleton(sp =>
            {
                MongoClient mongoClient = sp.GetRequiredService<MongoClient>();
                return mongoClient.GetDatabase(Constants.MongoCarteiraDatabase);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //var reverseProxy = app.ApplicationServices.GetRequiredService<ReverseProxySettings>();
            //if (reverseProxy.IsConfigured)
            //{
            //    app.Use(async (ctx, next) =>
            //    {
            //        if (!string.IsNullOrEmpty(reverseProxy.Scheme))
            //        {
            //            ctx.Request.Scheme = reverseProxy.Scheme;
            //        }
            //        if (!string.IsNullOrEmpty(reverseProxy.Host))
            //        {
            //            ctx.Request.Host = new HostString(reverseProxy.Host);
            //        }
            //        await next();
            //    });
            //    if (!string.IsNullOrEmpty(reverseProxy.PathBase))
            //    {
            //        app.UsePathBase(reverseProxy.PathBase);
            //    }
            //}

            app.UseForwardedHeaders();


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
        
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Jenner.Carteira.API v1"));

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
            });
        }
    }
}
