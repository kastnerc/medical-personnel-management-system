using System.Configuration;
using System.Data;
using System.Globalization;
using System.Windows;

namespace GestionPersonnelMedical
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Définir la culture par défaut en français
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("fr");
            GestionPersonnelMedical.Properties.Resources.Culture = new CultureInfo("fr"); // Définir la ressource
        }
    }

}
