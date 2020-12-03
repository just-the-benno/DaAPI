// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using DaAPI.Core.Common;
using DaAPI.Core.Common.DHCPv6;
using DaAPI.Core.FilterEngines.DHCPv6;
using DaAPI.Core.Notifications;
using DaAPI.Core.Notifications.Actors;
using DaAPI.Core.Notifications.Conditions;
using DaAPI.Core.Packets.DHCPv6;
using DaAPI.Core.Scopes;
using DaAPI.Core.Scopes.DHCPv6;
using DaAPI.Core.Services;
using DaAPI.Host.Identity;
using DaAPI.Host.Identity.Data;
using DaAPI.Host.Identity.Models;
using DaAPI.Host.Infrastrucutre;
using DaAPI.Infrastructure.FilterEngines.DHCPv6;
using DaAPI.Infrastructure.InterfaceEngines;
using DaAPI.Infrastructure.LeaseEngines.DHCPv6;
using DaAPI.Infrastructure.NotificationEngine;
using DaAPI.Infrastructure.ServiceBus;
using DaAPI.Infrastructure.ServiceBus.MessageHandler;
using DaAPI.Infrastructure.ServiceBus.Messages;
using DaAPI.Infrastructure.Services;
using DaAPI.Infrastructure.StorageEngine;
using DaAPI.Infrastructure.StorageEngine.DHCPv6;
using DaAPI.Shared.JsonConverters;
using IdentityServer4.Configuration;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DaAPI.Host
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        private AppSettings GetApplicationConfiguration(IServiceCollection services)
        {
            OpenIdConnectOptions openIdConnectOptions = Configuration.GetSection("OpenIdConnectOptions").Get<OpenIdConnectOptions>();

            services.AddSingleton(openIdConnectOptions);

            return new AppSettings
            {
                OpenIdConnectOptions = openIdConnectOptions,
            };
        }

        protected virtual void ConfigureAuthentication(IServiceCollection services, AppSettings appSettings)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Cookies";
                options.DefaultChallengeScheme = "oidc";
            }).AddJwtBearer("Bearer", options =>
            {
                options.Authority = $"{appSettings.OpenIdConnectOptions.AuthorityUrl}";
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters.ValidateIssuer = false;
                options.TokenValidationParameters.ValidateAudience = false;
            });
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var appSettings = GetApplicationConfiguration(services);

            var mvcBuilder = services.AddControllersWithViews()
                .AddNewtonsoftJson((options) =>
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.Converters.Add(new DHCPv6ScopePropertyRequestJsonConverter());
                    options.SerializerSettings.Converters.Add(new DUIDJsonConverter());
                });

#if DEBUG
            mvcBuilder.AddRazorRuntimeCompilation();
#endif

            services.AddDbContext<DaAPIIdentityDbContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("IdentityDb")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<DaAPIIdentityDbContext>()
                .AddDefaultTokenProviders();

            services.AddScoped<ILocalUserService, LocalUserManagerService>();

            var builder = services.AddIdentityServer(options =>
                {
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;

                    options.UserInteraction = new UserInteractionOptions()
                    {
                        LogoutUrl = "/Identity/Account/Logout",
                        LoginUrl = "/Identity/Account/Login",
                        LoginReturnUrlParameter = "returnUrl"
                    };
                })
                .AddInMemoryIdentityResources(Config.Ids)
                .AddInMemoryApiScopes(Config.Apis)
                .AddInMemoryClients(Config.Clients)
                .AddAspNetIdentity<ApplicationUser>();

            // not recommended for production - you need to store your key material somewhere secure
            builder.AddDeveloperSigningCredential();

            services.AddAuthentication();

            ConfigureAuthentication(services, appSettings);

            services.AddAuthorization(options =>
            {
                options.AddPolicy("DaAPI-API", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("scope", "daapi");
                });
            });

            services.AddDbContext<StorageContext>((x) => x.UseSqlite(Configuration.GetConnectionString("DaAPIDb"), option =>
    option.MigrationsAssembly(typeof(StorageContext).Assembly.FullName)), ServiceLifetime.Singleton, ServiceLifetime.Singleton);

            services.AddSingleton<DHCPv6RootScope>(sp =>
            {
                var storageEngine = sp.GetRequiredService<IDHCPv6StorageEngine>();
                var scope = storageEngine.GetRootScope(sp.GetRequiredService<IScopeResolverManager<DHCPv6Packet, IPv6Address>>()).GetAwaiter().GetResult();
                return scope;
            });

            services.AddTransient<IDHCPv6ServerPropertiesResolver, DatabaseDHCPv6ServerPropertiesResolver>();
            services.AddTransient<ISerializer, JSONBasedSerializer>();
            services.AddSingleton<IScopeResolverManager<DHCPv6Packet, IPv6Address>, DHCPv6ScopeResolverManager>();
            services.AddSingleton<IServiceBus, MediaRBasedServiceBus>();
            services.AddSingleton<IDHCPv6PacketFilterEngine, SimpleDHCPv6PacketFilterEngine>();
            services.AddSingleton<IDHCPv6InterfaceEngine, DHCPv6InterfaceEngine>();
            services.AddTransient<IDHCPv6LeaseEngine, DHCPv6LeaseEngine>();
            services.AddSingleton<IDHCPv6StorageEngine, DHCPv6StorageEngine>();
            services.AddSingleton<IDHCPv6ReadStore, StorageContext>();
            services.AddSingleton<IDHCPv6EventStore, StorageContext>();

            services.AddTransient<INotificationHandler<DHCPv6PacketArrivedMessage>>(sp => new DHCPv6PacketArrivedMessageHandler(
               sp.GetRequiredService<IServiceBus>(), sp.GetRequiredService<IDHCPv6PacketFilterEngine>(), sp.GetService<ILogger<DHCPv6PacketArrivedMessageHandler>>()));

            services.AddTransient<INotificationHandler<ValidDHCPv6PacketArrivedMessage>>(sp => new ValidDHCPv6PacketArrivedMessageHandler(
              sp.GetRequiredService<IServiceBus>(), sp.GetRequiredService<IDHCPv6LeaseEngine>(), sp.GetService<ILogger<ValidDHCPv6PacketArrivedMessageHandler>>()));

            services.AddTransient<INotificationHandler<InvalidDHCPv6PacketArrivedMessage>>(sp => new InvalidDHCPv6PacketArrivedMessageHandler(
           sp.GetRequiredService<IDHCPv6StorageEngine>(), sp.GetService<ILogger<InvalidDHCPv6PacketArrivedMessageHandler>>()));

            services.AddTransient<INotificationHandler<DHCPv6PacketFileteredMessage>>(sp => new DHCPv6PacketFileteredMessageHandler(
              sp.GetRequiredService<IDHCPv6StorageEngine>(), sp.GetService<ILogger<DHCPv6PacketFileteredMessageHandler>>()));

            services.AddTransient<INotificationHandler<DHCPv6PacketReadyToSendMessage>>(sp => new DHCPv6PacketReadyToSendMessageHandler(
              sp.GetRequiredService<IDHCPv6InterfaceEngine>(), sp.GetService<ILogger<DHCPv6PacketReadyToSendMessageHandler>>()));

            services.AddTransient<INotificationHandler<NewTriggerHappendMessage>>(sp => new NewTriggerHappendMessageHandler(
              sp.GetRequiredService<INotificationEngine>(), sp.GetService<ILogger<NewTriggerHappendMessageHandler>>()));

            services.AddTransient<DHCPv6RateLimitBasedFilter>();
            services.AddTransient<DHCPv6PacketConsistencyFilter>();

            services.AddSingleton<INotificationEngine, NotificationEngine>();
            services.AddSingleton<INotificationActorFactory, ServiceProviderBasedNotificationActorFactory>();
            services.AddSingleton<INotificationConditionFactory, ServiceProviderBasedNotificationConditionFactory>();
            services.AddTransient<INxOsDeviceConfigurationService, HttpBasedNxOsDeviceConfigurationService>();

            services.AddTransient<NxOsStaticRouteUpdaterNotificationActor>();
            services.AddTransient<DHCPv6ScopeIdNotificationCondition>();

            services.AddSingleton(sp =>
            {
                var storageEngine = sp.GetRequiredService<IDHCPv6StorageEngine>();
                var scope = storageEngine.GetRootScope(sp.GetRequiredService<IScopeResolverManager<DHCPv6Packet, IPv6Address>>()).GetAwaiter().GetResult();
                return scope;
            });

            services.AddSingleton<ServiceFactory>(p => p.GetService);
            services.AddLogging();

            services.AddHttpContextAccessor();
            services.AddScoped<IUserIdTokenExtractor, HttpContextBasedUserIdTokenExtractor>();

            services.AddMediatR(typeof(Startup).Assembly);

            services.AddHostedService<HostedService.LeaseTimerHostedService>();
            services.AddHostedService<HostedService.CleanupDatabaseTimerHostedService>();
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }

            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{area}/{controller}/{action=Index}/{id?}");
                endpoints.MapFallback("api/{**slug}", (httpContext) =>
                {
                    httpContext.Response.StatusCode = (Int32)System.Net.HttpStatusCode.NotFound;
                    return Task.FromResult(0);
                });

                endpoints.MapFallbackToFile("index.html");
            });

            using (var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var provider = scope.ServiceProvider;
                var context = provider.GetService<DaAPIIdentityDbContext>();
                if (context.Database.IsSqlite() == true)
                {
                    context.Database.Migrate();
                }

                var storageContext = provider.GetService<StorageContext>();
                if (storageContext.Database.IsSqlite() == true)
                {
                    storageContext.Database.Migrate();
                }
#if DEBUG
                //var seeder = new DatabaseSeeder();
                //seeder.SeedDatabase(false, storageContext).GetAwaiter().GetResult();
#endif
                IDHCPv6PacketFilterEngine packetFilterEngine = provider.GetService<IDHCPv6PacketFilterEngine>();
                packetFilterEngine.AddFilter(provider.GetService<DHCPv6RateLimitBasedFilter>());
                packetFilterEngine.AddFilter(provider.GetService<DHCPv6PacketConsistencyFilter>());

                var storage = provider.GetService<IDHCPv6ReadStore>();
                var serverproperties = storage.GetServerProperties().GetAwaiter().GetResult();
                if (serverproperties != null && serverproperties.ServerDuid != null)
                {
                    packetFilterEngine.AddFilter(new DHCPv6PacketServerIdentifierFilter(serverproperties.ServerDuid, provider.GetService<ILogger<DHCPv6PacketServerIdentifierFilter>>()));
                }

                var interfaceEngine = provider.GetService<IDHCPv6InterfaceEngine>();
                interfaceEngine.Initialize().GetAwaiter().GetResult();

                var notificationEngine = provider.GetService<INotificationEngine>();
                notificationEngine.Initialize().GetAwaiter().GetResult();
            }
        }
    }
}