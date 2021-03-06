﻿using FreeSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace TestDemo01
{
    public class Startup
    {
        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            Configuration = configuration;

            Fsql = new FreeSql.FreeSqlBuilder()
                .UseConnectionString(FreeSql.DataType.Sqlite, @"Data Source=|DataDirectory|/document.db;Pooling=true;Max Pool Size=10")
                .UseAutoSyncStructure(true)
                .UseLazyLoading(true)

                .UseMonitorCommand(cmd => Trace.WriteLine(cmd.CommandText))
                .Build();

            Fsql.Aop.CurdBefore += (_, e) => Trace.WriteLine(e.Sql);
        }

        public IConfiguration Configuration { get; }
        public static IFreeSql Fsql { get; private set; }

        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllersWithViews();
            services.AddSingleton(Fsql);
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseHttpMethodOverride(new HttpMethodOverrideOptions { FormFieldName = "X-Http-Method-Override" });
            app.UseDeveloperExceptionPage();

            app.UseRouting();
            app.UseEndpoints(a => a.MapControllers());

            //可以配置子目录访问，如：/testadmin/
            app.UseFreeAdminLtePreview("/",
                typeof(TestDemo01.Entitys.Song),
                typeof(TestDemo01.Entitys.Tag),
                typeof(TestDemo01.Entitys.User),
                typeof(TestDemo01.Entitys.UserImage));
        }
    }

    public interface ISoftDelete
    {
        bool IsDeleted { get; set; }
    }
}
