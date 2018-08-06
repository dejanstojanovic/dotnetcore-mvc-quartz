using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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


            services.AddSingleton<IJobFactory, ScheduledJobFactory>();

            //services.Add(jobs.Select(jobType => new ServiceDescriptor(jobType, jobType, ServiceLifetime.Singleton)));

            services.AddSingleton(async provider =>
            {
                var schedulerFactory = new StdSchedulerFactory();
                var scheduler = await schedulerFactory.GetScheduler();

                IJobDetail job = JobBuilder.Create<ScheduledJob>()
                    .WithIdentity("FiveSecondsJob", "group1") // name "myJob", group "group1"
                    .Build();

                ITrigger trigger = TriggerBuilder.Create()
                 .WithIdentity($"FiveSecondsJob.trigger", "group1")
                 .StartNow()
                 .WithSimpleSchedule
                  (s =>
                     s.WithInterval(TimeSpan.FromSeconds(3))
                     .RepeatForever()
                  )
                  .Build();

                await scheduler.ScheduleJob(job, trigger);
                await scheduler.Start();


                return scheduler;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
