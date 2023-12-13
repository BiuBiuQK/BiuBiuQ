using BiuBiuQ.Command;
using BiuBiuQ.Model.Video;
using BiuBiuQ.Service.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;

namespace BiuBiuQ.ViewModel.VideoDown
{
    public class VideoDownedViewModel
    {
        public DelegateCommand OpenFilePathCommand { get; set; }
        public DelegateCommand DeleteDownedVideoCommand { get; set; }
        public DelegateCommand DeleteAllWithFileCommand { get; set; }
        public DelegateCommand DeleteAllWithOutFileCommand { get; set; }

        public VideoDownedViewModel() {

            OpenFilePathCommand = new DelegateCommand();
            OpenFilePathCommand.ExcuteAction = new Action<object>(OpenFilePath);

            DeleteDownedVideoCommand = new DelegateCommand();
            DeleteDownedVideoCommand.ExcuteAction = new Action<object>(DeleteDownedVideo);

            DeleteAllWithFileCommand = new DelegateCommand();
            DeleteAllWithFileCommand.ExcuteAction = new Action<object>(DeleteAllWithFile);

            DeleteAllWithOutFileCommand = new DelegateCommand();
            DeleteAllWithOutFileCommand.ExcuteAction = new Action<object>(DeleteAllWithOutFile);

            LoadVideoDownedList();
        }

        public void LoadVideoDownedList()
        {
            List<VideoInfoModel> videoInfos = DBService.Instance.GetDownedVideoList();
            for (int i = 0; i < videoInfos.Count; i++)
            {
               VideoCollectionModel videoCollectionModel = DBService.Instance.GetVideoCollectionModel(videoInfos[i].Bvid);
                VideoDownDataViewModel vdDatas = new VideoDownDataViewModel();
                vdDatas.VideoInfo = videoInfos[i];
                vdDatas.VideoCollection = videoCollectionModel;

                //下面设置颜色其实有点违背MVVM
                //前端可用Style方式，固定几个数据，比如error/waring/info等，这样前端可以自定义颜色了
                if (!File.Exists( vdDatas.VideoInfo.FilePath)) 
                {
                    vdDatas.Tips = "文件不存在，可能已被移动或者删除";
                    vdDatas.TipsColor = "#FF0000";
                    vdDatas.TipsShow = "Visible";
                }
                else
                {
                    vdDatas.TipsShow = "Hidden";
                }
                
                VideoDownedStaticViewModel.Instance.VideoDownedDatas.Add(vdDatas);
            }
            
        }

        public void OpenFilePath(object parameter)
        {
            if(parameter!=null)
            {
                bool flag = false;
                string path = string.Empty;
                VideoDownDataViewModel vd = (VideoDownDataViewModel)parameter;
                if(vd.VideoInfo.FilePath!=null)
                {
                    if(File.Exists(vd.VideoInfo.FilePath))
                    {
                        path = Path.GetDirectoryName(vd.VideoInfo.FilePath);
                        if (Directory.Exists(path))
                        {
                            flag = true;
                        }
                    }
                }
                if(flag)
                {
                    try
                    {
                        Process.Start("explorer.exe", path);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("文件不存在，可能已被删除或者移动");
                        MessageBox.Show("文件不存在，可能已被删除或者移动", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    Console.WriteLine("文件不存在，可能已被删除或者移动");
                    MessageBox.Show("文件不存在，可能已被删除或者移动", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                
            }
            

        }

        public void DeleteDownedVideo(object parameter)
        {
            if (parameter != null)
            {
                VideoDownDataViewModel vd = (VideoDownDataViewModel)parameter;
                if (File.Exists(vd.VideoInfo.FilePath))
                {
                    File.Delete(vd.VideoInfo.FilePath);
                }
                DBService.Instance.UpdateVideoStatusAndPath(vd.VideoInfo.Cid, -1, VideoStatus.None, "");
                VideoDownedStaticViewModel.Instance.VideoDownedDatas.Remove(vd);
            }
        }

        /// <summary>
        /// 清空列表并删除文件
        /// </summary>
        /// <param name="parameter"></param>
        public void DeleteAllWithFile(object parameter)
        {
            Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
            Task.Run(() =>
            {
                ObservableCollection<VideoDownDataViewModel> videoDowns = new ObservableCollection<VideoDownDataViewModel>(VideoDownedStaticViewModel.Instance.VideoDownedDatas);
                foreach(var videoDown in videoDowns)
                {
                    
                    if (File.Exists(videoDown.VideoInfo.FilePath))
                    {
                        string path = Path.GetDirectoryName(videoDown.VideoInfo.FilePath);
                        Directory.Delete(path, true);
                    }
                    DBService.Instance.UpdateVideoStatusAndPath(videoDown.VideoInfo.Cid, -1, VideoStatus.None, "");
                    dispatcher.Invoke(new Action(() =>
                    {
                        VideoDownedStaticViewModel.Instance.VideoDownedDatas.Remove(videoDown);
                    }));
                }
                //一次性清空
                //dispatcher.Invoke(new Action(() =>
                //{
                //    VideoDownedStaticViewModel.Instance.VideoDownedDatas.Clear();
                //}));
            });
        }

        public void DeleteAllWithOutFile(object parameter)
        {
            foreach (var videoDown in VideoDownedStaticViewModel.Instance.VideoDownedDatas)
            {
                DBService.Instance.UpdateVideoStatusAndPath(videoDown.VideoInfo.Cid, -1, VideoStatus.None, "");
            }
            VideoDownedStaticViewModel.Instance.VideoDownedDatas.Clear();
        }
    }
}
