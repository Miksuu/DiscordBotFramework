public abstract class Singleton<T> where T : new()
{
    public static T Instance
    {
        get
        {
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new T();
                }
                return instance;
            }
        }
        set
        {
            instance = value;
        }
    }

    private static T instance;
    private static readonly object padlock = new object();
}