using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schedule.WebApiCore.Sample.Schedule
{
    public static class QuartzExtensions
    {
        public static void AddQuartz(this IServiceCollection services , IJobDetail jobDetail, ITrigger trigger) {
            services.AddSingleton<IScheduler>(provider =>
            {
                var schedulerFactory = new StdSchedulerFactory();
                var scheduler = schedulerFactory.GetScheduler().Result;

               //IJobDetail jobDetail = JobBuilder.Create<ScheduledJob>()
               //     .WithIdentity("FiveSecondsJob", "group1")
               //     .Build();

                //ITrigger trigger = TriggerBuilder.Create()
                //.WithIdentity($"FiveSecondsJob.trigger", "group1")
                //.StartNow()
                //.WithSimpleSchedule
                // (s =>
                //    s.WithInterval(TimeSpan.FromSeconds(3))
                //    .RepeatForever()
                // )
                // .Build();

                var jobSchedule = scheduler.ScheduleJob(jobDetail, trigger).Result;
                
                return scheduler;

            });

        }
    }
}
