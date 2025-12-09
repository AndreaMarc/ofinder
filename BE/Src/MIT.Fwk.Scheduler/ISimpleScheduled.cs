namespace MIT.Fwk.Scheduler
{
    public interface ISimpleScheduled : IScheduled
    {
        int WithIntervalInMinutes();

        bool RepeatForever();

        int WithRepeatCount();
    }
}
