using uBeac.Hosted.Service;

namespace uBeac.DeviceProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceStarter.Run<App>(args);
        }
    }
}
