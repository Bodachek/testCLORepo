using CLO365;
using CLO365.Forms;
using System;
using System.Windows.Forms;

namespace GitTest3
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new frmClone());
            Application.Run(new frmLogin());
            //Application.Run(new frmMain());
            //Application.Run(new frmPublishtoProduction());
        }
    }
}
