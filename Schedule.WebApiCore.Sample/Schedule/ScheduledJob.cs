using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schedule.WebApiCore.Sample.Schedule
{
    public class ScheduledJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {


            //await Task.FromResult("");
            await Task.CompletedTask;
            
        }
    }
}
