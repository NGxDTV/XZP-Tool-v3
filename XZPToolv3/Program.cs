using System;
using System.IO;
using System.Windows.Forms;

namespace XZPToolv3
{
    static class Program
    {
        public static string StartupFile { get; private set; }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Check if file was passed as argument (for file associations)
            if (args.Length > 0 && File.Exists(args[0]))
            {
                StartupFile = args[0];
            }

            Application.Run(new MainForm());
        }
    }
}
