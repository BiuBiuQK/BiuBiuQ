using BiuBiuQ.Command;
using BiuBiuQ.Service.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BiuBiuQ.ViewModel.Config
{
    public class ConfigViewModel
    {
        //public DelegateCommand SetSavePathCommand { get; set; }
        public DelegateCommand SelectSavePathCommand { get; set; }
        public DelegateCommand SetDownTaskCountCommand { get; set; }
        public ConfigViewModel() {

            //SetSavePathCommand = new DelegateCommand();
            //SetSavePathCommand.ExcuteAction = new Action<object>(SetSavePath);

            SelectSavePathCommand = new DelegateCommand();
            SelectSavePathCommand.ExcuteAction = new Action<object>(SelectSavePath);

            SetDownTaskCountCommand = new DelegateCommand();
            SetDownTaskCountCommand.ExcuteAction = new Action<object>(SetDownTaskCount);
        }

        /// <summary>
        /// 弃用，设置保存目录命令委托函数
        /// </summary>
        /// <param name="parameter"></param>
        [Obsolete]
        public void SetSavePath(object parameter)
        {
            if (parameter != null)
            {
                string savePath = (string)parameter;
                ConfigStaticViewModel.Instance.SavePath = savePath;
                ConfigService.SetSavePath(savePath);//保存到数据库
            }
        }

        /// <summary>
        /// 选择保存目录
        /// </summary>
        /// <param name="parameter"></param>
        public void SelectSavePath(object parameter)
        {
            //FolderBrowserDialog 需引用Winfrom，双击项目，在配置文件中的<PropertyGroup>节点 加入<UseWindowsForms>true</UseWindowsForms>
            //参考文章 https://blog.csdn.net/YUNAN_ZHANG/article/details/124689596

            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                ConfigStaticViewModel.Instance.SavePath = dialog.SelectedPath;
                ConfigService.SetSavePath(dialog.SelectedPath);//保存到数据库
            }

        }

        /// <summary>
        /// 设置下载并发数命令委托函数
        /// </summary>
        /// <param name="parameter"></param>
        public void SetDownTaskCount(object parameter)
        {
            if (parameter != null)
            {
                string dcountStr = (string)parameter;
                int dcount;
                if(int.TryParse(dcountStr, out dcount))
                {
                    if(dcount<=0)
                    {
                        dcount = 1;
                    }
                }
                else
                    dcount = 1;

                ConfigStaticViewModel.Instance.DownTaskCount = dcount;
                ConfigService.SetDownTaskCount(dcount.ToString());//保存到数据库
            }
        }
    }
}
