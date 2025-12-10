using Microsoft.Extensions.DependencyInjection;
using System;

namespace MIT.Fwk.Scheduler
{
    public interface IScheduled
    {
        void ConfigureServices(IServiceCollection services);


        string ScheduleName();

        Type JobType();

        bool StartNow();

        DateTimeOffset StartAt();
    }
}
