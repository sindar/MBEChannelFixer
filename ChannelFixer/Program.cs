using System;
using System.Windows.Forms;

namespace ChannelFixer
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            string configFile = Environment.CurrentDirectory + "\\settings.cfg";
            if (args.Length >= 1)
                configFile = args[0];

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(configFile));
        }
    }
}
