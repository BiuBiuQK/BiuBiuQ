using BiuBiuQ.Command;
using BiuBiuQ.Model.Config;
using BiuBiuQ.Service.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BiuBiuQ.ViewModel.Config
{
    public class ConfigStaticViewModel:NotificationObject
    {
        private ConfigStaticViewModel() 
        {
            SavePath = ConfigService.GetSavePath();
            DownTaskCount = ConfigService.GetDownTaskCount();
        }
        public static ConfigStaticViewModel Instance { get; set; } = new ConfigStaticViewModel();

        private ConfigModel configModel = new ConfigModel();

        public string SavePath
        {
            get { return configModel.SavePath; }
            set
            {
                configModel.SavePath = value;
                this.RaisPropertyChanged(nameof(SavePath));
            }
        }

        public int DownTaskCount 
        { 
            get { return configModel.DownTaskCount; } 
            set
            {
                configModel.DownTaskCount = value;
                this.RaisPropertyChanged(nameof(DownTaskCount));
            }
        }

        public Dictionary<string, string> VideoQnName = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            { "16","320P"},
            { "32","320P"},
            { "64","480P"},
            { "74","720P60"},
            { "80","1080P"},
            { "112","1080P+"},
            { "116","1080P60"},
            { "120","4K"}
        };

        public Dictionary<string, string> AudioQnName = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            {"30216","64K" },
            {"30232","128K" },
            {"30280","192K" },
        };
    }
}
