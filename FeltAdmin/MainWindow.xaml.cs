using FeltAdmin.Configuration;
using FeltAdmin.Diagnostics;
using FeltAdmin.Viewmodels;
using System;
using System.Windows;

namespace FeltAdmin
{
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    using FeltAdminCommon.Lisens;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            var model = this.InitViewModels();
            this.DataContext = model;
            this.InitializeComponent();

            var logfile = ConfigurationLoader.GetAppSettingsValue("LogFile");

            var Skytterlag = ConfigurationLoader.GetAppSettingsValue("Skytterlag");
            var Lisens = ConfigurationLoader.GetAppSettingsValue("Lisens");


          

             var LoggingLevelsString = ConfigurationLoader.GetAppSettingsValue("LoggingLevels");
            LoggingLevels enumLowestTrace = LoggingLevels.Info;
            if (!string.IsNullOrEmpty(LoggingLevelsString))
            {
                if (!Enum.TryParse(LoggingLevelsString, true, out enumLowestTrace))
                {
                    enumLowestTrace = LoggingLevels.Info;
                }
            }

            var fileAppsender = new FileAppender(logfile, enumLowestTrace, LoggingLevels.Trace);
            Log.AddAppender(fileAppsender);
            if (!LisensChecker.Validate(Skytterlag, DateTime.Now, Lisens))
            {
                Log.Error("Lisens not valid for {0}", Skytterlag);
                Application.Current.Shutdown();
            }

            Log.Info("FeltAdmin started");

            model.DisableDb += model_DisableDb;
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += CurrentDomainUnhandledException;
        }
        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }



        public static string ByteArrayToString(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex.Replace("-", "");
        }

        void model_DisableDb(object sender)
        {
            var index = sender as int?;
            if (index.HasValue)
            {
                tabControl.SelectedIndex = index.Value;
            }
            else
            {
                tabControl.SelectedIndex = 1;
            }
            
        }

        private static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Error(e.ExceptionObject as Exception, "Unhandled exception");
        }

        private MainViewModel InitViewModels()
        {
            var model = new MainViewModel();
            ////var leonPersons = new List<LeonPerson>();
            ////leonPersons.Add(new LeonPerson() { Name = "Roald Parelius", Class = "4", ClubName = "Bodø Østre", Range = 2, ShooterId = 1234, SumIn = 24, Target = 13, Team = 1 });
            ////leonPersons.Add(new LeonPerson() { Name = "Lars-Håkon Nohr Nystad", Class = "5", ClubName = "Bodø Østre", Range = 2, ShooterId = 1235, SumIn = 23, Target = 14, Team = 1 });
            ////model.Leon = new LeonViewModel { LeonRegistrations = new ObservableCollection<LeonPerson>(leonPersons) };
            return model;
        }
       
    }
}
