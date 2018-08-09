using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using Schedule.WebApiCore.Sample.Schedule;


namespace Schedule.WebApiCore.Sample
{
    public class Startup
    {

        public IConfiguration Configuration { get; }
        public IHostingEnvironment HostingEnvironment { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            this.HostingEnvironment = hostingEnvironment;
            this.Configuration = configuration;
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();

            services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{this.HostingEnvironment.EnvironmentName.ToLower()}.json")
                .Build());

            #region Configure Quartz DI

            services.Add(new ServiceDescriptor(typeof(ScheduledJob), typeof(ScheduledJob), ServiceLifetime.Singleton));
            services.AddSingleton<IJobFactory, ScheduledJobFactory>();
            services.AddSingleton<IJobDetail>(provider =>
            {
                return JobBuilder.Create<ScheduledJob>()
                  .WithIdentity("Sample.job", "group1")
                  .Build();
            });

            services.AddSingleton<ITrigger>(provider =>
            {
                return TriggerBuilder.Create()
                .WithIdentity($"Sample.trigger", "group1")
                .StartNow()
                .WithSimpleSchedule
                 (s =>
                    s.WithInterval(TimeSpan.FromSeconds(5))
                    .RepeatForever()
                 )
                 .Build();
            });

            services.AddSingleton<IScheduler>(provider =>
            {
                var schedulerFactory = new StdSchedulerFactory();
                var scheduler = schedulerFactory.GetScheduler().Result;
                scheduler.JobFactory = provider.GetService<IJobFactory>();
                scheduler.Start();
                return scheduler;
            });

            #endregion

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IScheduler scheduler)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            scheduler.ScheduleJob(app.ApplicationServices.GetService<IJobDetail>(), app.ApplicationServices.GetService<ITrigger>());

            app.UseMvc();
        }
    }
}
