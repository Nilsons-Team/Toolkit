namespace Toolkit_Server
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Thread.CurrentThread.Name = "Main";
            var server = new Server(args);
            server.Start();
        }
    }
}
