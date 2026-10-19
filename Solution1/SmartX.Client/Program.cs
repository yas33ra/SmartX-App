using SmartX.Client.Navigation;

namespace SmartX.Client
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Navigator.Start();
        }
    }
}