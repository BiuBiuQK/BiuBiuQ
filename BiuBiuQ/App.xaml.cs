using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

namespace BiuBiuQ
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region 引入控制台
        [DllImport("kernel32")]
        public static extern bool AllocConsole();
        [DllImport("kernel32")]
        public static extern bool FreeConsole();
        #endregion

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
#if DEBUG
            AllocConsole();
#endif
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
#if DEBUG
            FreeConsole();
#endif
        }
    }
}
