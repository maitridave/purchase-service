using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMapper;
using CorrelationId.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MassTransit;

namespace AI.PurchaseService.Shared
{
    public static class DependencyInjectionExtension
    {
        public static IServiceCollection AddServiceExtension(this IServiceCollection services,
            IConfiguration configuration)
        {
            var mapperConfig = new MapperConfiguration(mc => { mc.AddMaps(Assembly.GetExecutingAssembly()); });
            var mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);

            services.RegisterCorrelationIdServices();

            services.RegisterHealthCheck();
            services.RegisterMessageBroker(configuration);
            return services;
        }

        public static IServiceCollection AddConfigurations(this IServiceCollection services,
            IConfiguration configuration)
        {
            // Set the JSON serializer options
            services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
            {
                options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
                options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.SerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower;
            });

            // Set the API Behavior options
            services.Configure<MvcOptions>(options =>
            {
                // Add any custom filters here if needed
            });

            // Set the API Behavior options
            services.Configure<ApiBehaviorOptions>(options =>
            {
                //ConfigureApiBehaviorOptions- To disable automatic model validation
                //[For stop 400 response with default message] and
                ////validate filed with fluent validation and show our custom message
                options.SuppressModelStateInvalidFilter = true;
                //SuppressMapClientErrors suppress 404 client error body,default false. 
                options.SuppressMapClientErrors = true;
            });

            return services;
        }

        public static IServiceCollection AddAuthentication(this IServiceCollection services,
            IConfiguration configuration)
        {
            // Temporarily disable JWT authentication to focus on core API functionality
            /*
            services.AddAuthentication()
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.Authority = configuration.GetValue<string>("IDENTITY_PROVIDER_SERVICE_HOST");
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = true,
                        ValidAudience = Constants.Audience.Universal
                    };
                });
            */
            return services;
        }

        private static void RegisterCorrelationIdServices(this IServiceCollection services)
        {
            services.AddCorrelationId(options =>
            {
                options.AddToLoggingScope = true;
                options.RequestHeader = Constants.CorrelationIdConfig.HeaderName;
                options.UpdateTraceIdentifier = true;
            }).WithGuidProvider();
        }

        private static void RegisterHealthCheck(this IServiceCollection services)
        {
            services.AddHealthChecks();
        }

        private static IServiceCollection RegisterMessageBroker(this IServiceCollection services, IConfiguration configuration)
        {
            var rabbitMqHostName = configuration.GetValue<string>("Configs:RabbitMQ:HostName");
            
            if (!string.IsNullOrEmpty(rabbitMqHostName))
            {
                services.AddMassTransit(x =>
                {
                    // Register consumers
                    x.AddConsumer<AI.PurchaseService.Domain.Consumers.OfferCreatedConsumer>();
                    x.AddConsumer<AI.PurchaseService.Domain.Consumers.OfferUpdatedConsumer>();
                    x.AddConsumer<AI.PurchaseService.Domain.Consumers.TransportCreatedConsumer>();
                    
                    x.UsingRabbitMq((context, cfg) =>
                    {
                        // Configure JSON serialization options to match API settings
                        cfg.UseRawJsonSerializer(RawSerializerOptions.AnyMessageType, isDefault: true);
                        cfg.ConfigureJsonSerializerOptions(options =>
                        {
                            options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
                            options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                            options.DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower;
                            return options;
                        });
                        
                        cfg.Host(rabbitMqHostName, configuration.GetValue<ushort>("Configs:RabbitMQ:PortNumber"), "/", h =>
                        {
                            h.Username(configuration.GetValue<string>("Secrets:RabbitMQ:Username") ?? "guest");
                            h.Password(configuration.GetValue<string>("Secrets:RabbitMQ:UserPassword") ?? "guest");
                        });
                        
                        // Configure individual endpoints with prefetch count = 1
                        cfg.ReceiveEndpoint("OfferCreated", e =>
                        {
                            e.PrefetchCount = 1;
                            
                            // Configure retry policy
                            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                            
                            // Configure error handling
                            e.DiscardFaultedMessages();
                            
                            e.ConfigureConsumer<AI.PurchaseService.Domain.Consumers.OfferCreatedConsumer>(context);
                        });
                        
                        cfg.ReceiveEndpoint("OfferUpdated", e =>
                        {
                            e.PrefetchCount = 1;
                            
                            // Configure retry policy
                            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                            
                            // Configure error handling
                            e.DiscardFaultedMessages();
                            
                            e.ConfigureConsumer<AI.PurchaseService.Domain.Consumers.OfferUpdatedConsumer>(context);
                        });
                        
                        cfg.ReceiveEndpoint("TransportCreated", e =>
                        {
                            e.PrefetchCount = 1;
                            
                            // Configure retry policy
                            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                            
                            // Configure error handling
                            e.DiscardFaultedMessages();
                            
                            e.ConfigureConsumer<AI.PurchaseService.Domain.Consumers.TransportCreatedConsumer>(context);
                        });
                    });
                });
            }
            
            return services;
        }
    }
}