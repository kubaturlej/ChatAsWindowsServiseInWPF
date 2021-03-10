using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace MyService
{
    [RunInstaller(true)]
    public class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            ServiceProcessInstaller serviceProcessInstaller =
                   new ServiceProcessInstaller();
            ServiceInstaller serviceInstaller = new ServiceInstaller();

            //serviceProcessInstaller.Account = ServiceAccount.LocalService;
            //serviceProcessInstaller.Account = ServiceAccount.User;
            serviceProcessInstaller.Account = ServiceAccount.LocalSystem;

            serviceInstaller.ServiceName = MojaUsluga.NazwaUslugi;
            serviceInstaller.StartType = ServiceStartMode.Manual;
            serviceInstaller.DisplayName = "Mój Chat";
            serviceInstaller.Description = "Chat - usługa - stworzona \"od zera\"";

            this.Installers.Add(serviceProcessInstaller);
            this.Installers.Add(serviceInstaller);
        }
    }
}
