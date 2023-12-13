using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiuBiuQ.ViewModel.VideoDown
{
    /// <summary>
    /// 下载中全局类 静态单例模式
    /// </summary>
    public class VideoDowningStaticViewModel:NotificationObject
    {
        private VideoDowningStaticViewModel() 
        {
            if(videoDowningDatas == null)
            {
                videoDowningDatas = new ObservableCollection<VideoDownDataViewModel> (); 
            }
        }

        /// <summary>
        /// 静态单例
        /// </summary>
        public static VideoDowningStaticViewModel Instance { get; private set; } = new VideoDowningStaticViewModel();

        /// <summary>
        /// 下载中列表
        /// </summary>
        private ObservableCollection<VideoDownDataViewModel> videoDowningDatas;
        public ObservableCollection<VideoDownDataViewModel> VideoDowningDatas {
            get {  return videoDowningDatas; }
            set
            {
                videoDowningDatas = value;
                this.RaisPropertyChanged(nameof(VideoDowningDatas));   
            }
        }
    }
}
