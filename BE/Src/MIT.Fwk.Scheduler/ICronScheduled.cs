namespace MIT.Fwk.Scheduler
{
    public interface ICronScheduled : IScheduled
    {
        string GetCronOptions();
    }
}
