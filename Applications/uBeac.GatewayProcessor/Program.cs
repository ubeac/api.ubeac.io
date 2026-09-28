using uBeac.Hosted.Service;

namespace uBeac.GatewayProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceStarter.Run<App>(args);
        }
    }
}
