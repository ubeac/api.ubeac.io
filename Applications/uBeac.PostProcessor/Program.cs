using uBeac.Hosted.Service;

namespace uBeac.PostProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceStarter.Run<App>(args);
        }
    }
}
