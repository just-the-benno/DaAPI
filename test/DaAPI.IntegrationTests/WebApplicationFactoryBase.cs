using DaAPI.Infrastructure.StorageEngine;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DaAPI.IntegrationTests
{
    public  abstract class WebApplicationFactoryBase
    {
        protected static void RemoveServiceFromCollection(IServiceCollection services, Type type)
        {
            var descriptor = services.SingleOrDefault(
            d => d.ServiceType == type);

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }
        }

        protected static void ReplaceService<T>(IServiceCollection services, T implementation) where T : class
        {
            RemoveServiceFromCollection(services, typeof(T));
            services.AddSingleton(implementation);
        }

        protected static void ReplaceServiceWithMock<T>(IServiceCollection services) where T : class
        {
            RemoveServiceFromCollection(services, typeof(T));
            services.AddSingleton(Mock.Of<T>());
        }

        protected static void AddSqliteDatabase(IServiceCollection services, String filename)
        {
            DbContextOptionsBuilder<StorageContext> dbContextOptionsBuilder = new DbContextOptionsBuilder<StorageContext>();
            dbContextOptionsBuilder.UseSqlite($"Filename={filename}", options =>
            {
                options.MigrationsAssembly(typeof(StorageContext).Assembly.FullName);
            });

            DbContextOptions<StorageContext> contextOptions = dbContextOptionsBuilder.Options;

            ReplaceService(services, contextOptions);
        }

    }
}
