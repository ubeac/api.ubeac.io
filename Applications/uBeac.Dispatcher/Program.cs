using uBeac.Hosted.Service;

namespace uBeac.Dispatcher
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceStarter.Run<App>(args);
        }
    }
}