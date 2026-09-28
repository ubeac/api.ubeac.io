using uBeac.Hosted.Service;

namespace uBeac.Processor
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceStarter.Run<App>(args);
        }
    }
}
