using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;
using Confluent.Kafka;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Jenner.Agendamento.API.Services.Consumer;
using MongoDB.Driver;
using Jenner.Comum;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System;

namespace Jenner.Agendamento.API
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
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Jenner.Agendamento.API", Version = "v1" });
            });
            
            AddKafkaServices(services);
            AddMongoServices(services);

            services.AddHealthChecks()
                .AddMongoDb(Configuration.GetConnectionString(Constants.MongoConnectionString))
                .AddKafka(new ProducerConfig
                {
                    BootstrapServers = Configuration.GetConnectionString(Constants.KafkaBootstrapKey),
                });

            services.AddScoped(c =>
            {
                var config = new ConsumerConfig
                {
                    BootstrapServers = Configuration.GetConnectionString(@"KafkaBootstrap"),
                    GroupId = "agendar-worker",
                    AutoOffsetReset = AutoOffsetReset.Earliest,
                    AllowAutoCreateTopics = true
                };
                return new ConsumerBuilder<string, byte[]>(config).Build();
            });

            services.AddHostedService<AgendarWorker>();
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
                MongoClientSettings settings = MongoClientSettings.FromConnectionString(Configuration.GetConnectionString(Constants.MongoConnectionString));
                settings.WaitQueueSize = int.MaxValue;
                settings.WaitQueueTimeout = new TimeSpan(0, 4, 0);
                settings.MinConnectionPoolSize = 0;
                settings.MaxConnectionPoolSize = 1000;

                return new MongoClient(settings);
                //return new MongoClient(Configuration.GetConnectionString(Constants.MongoConnectionString));
            });
            services.AddSingleton(sp =>
            {
                MongoClient mongoClient = sp.GetRequiredService<MongoClient>();
                return mongoClient.GetDatabase(Constants.MongoAgendamentoDatabase);
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
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Jenner.Agendamento.API v1"));
            //app.UseForwardedHeaders();

            //if (!Configuration.GetValue<bool>("DOTNET_RUNNING_IN_CONTAINER"))
            //{
            //    app.UseHttpsRedirection();
            //}

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health/startup");
                endpoints.MapHealthChecks("/healthz", new HealthCheckOptions { Predicate = _ => false });
                endpoints.MapHealthChecks("/ready", new HealthCheckOptions { Predicate = _ => false });
                endpoints.MapControllers();
            });
        }
    }
}
