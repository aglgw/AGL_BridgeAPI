using Quartz.Logging;

public class QuartzLogProvider : ILogProvider
{
    public Logger GetLogger(string name)
    {
        return (level, func, exception, parameters) => {
            if (level >= Quartz.Logging.LogLevel.Info && func != null)
            {
                Console.WriteLine("[" + DateTime.Now.ToLongTimeString() + "] [" + level + "] " + func(), parameters);
            }
            return true;
        };
    }

    public IDisposable OpenMappedContext(string key, object value, bool destructure)
    {
        throw new NotImplementedException();
    }

    public IDisposable OpenNestedContext(string message)
    {
        throw new NotImplementedException();
    }

}