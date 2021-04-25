using AudioPlayer.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AudioPlayer.IOC
{
    public static class DependencyInjectionContainer
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services)
        {
            ////services.AddDbContext<IDatabaseContext, DataBase.Services.AppContext>();
            //if (!string.IsNullOrEmpty(App.ROOTFOLDER)) services.AddScoped<IDatabaseContext>(x => new DataBase.Services.AppContext(Path.Combine(App.ROOTFOLDER, "App.db3")));
            ////services.AddDbContext<DataBase.Services.AppContext>().AddEntityFrameworkSqlite();
            //services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
            //services.AddScoped<IUnitOfWork, UnitOfWork>();
            //services.AddScoped<ItemsViewModel>();
            //services.AddScoped<NewItemViewModel>();            // Add Logger
            //services.AddLogging(configure =>
            //{
            //    configure.ClearProviders();
            //    configure.SetMinimumLevel(LogLevel.Error);
            //    configure.AddSerilog(new LoggerConfiguration()
            //        .MinimumLevel.Error()
            //        .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
            //        .CreateLogger(),
            //        true
            //        );
            //});

            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<App>();
            foreach (var implementationType in Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.Namespace != null && t.Namespace.EndsWith(".ViewModels") && t.Name.EndsWith("Model")))
            {

                services.AddSingleton(implementationType);
            }

            foreach (var implementationType in Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.Namespace != null && t.Namespace.EndsWith(".Views") && t.Name.EndsWith("View")))
            {

                services.AddSingleton(implementationType);
            }



            return services;
        }

    }
}
