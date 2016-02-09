using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Windows;

namespace WpfApplication1
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow window = new WpfApplication1.MainWindow();
            string filename = "../../../PerfAnalysis.xml";
            string filecontent = "<Performance> <Clients> </Clients> <Server> </Server> </Performance> ";
            File.WriteAllText(filename, filecontent);
            int r = int.Parse(e.Args[0]);
            int w = int.Parse(e.Args[1]);

            for (int i = 0; i < r; i++)
            {
                window.addRdClient();
            }

            for (int i = 0; i < w; i++)
            {
                window.addWrClient();
            }
            
            window.Show();
        }
    }
}
