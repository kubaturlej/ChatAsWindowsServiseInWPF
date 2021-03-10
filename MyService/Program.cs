using System.ServiceProcess;

namespace MyService
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceBase.Run(new MojaUsluga());
        }
    }
}
