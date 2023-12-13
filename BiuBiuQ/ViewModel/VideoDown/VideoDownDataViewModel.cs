using BiuBiuQ.Model.Video;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiuBiuQ.ViewModel.VideoDown
{
    /// <summary>
    /// 视频下载信息
    /// </summary>
    public class VideoDownDataViewModel: NotificationObject
    {
        /// <summary>
        /// 视频对应的合集信息
        /// </summary>
        public VideoCollectionModel VideoCollection { get; set; }

        /// <summary>
        /// 视频信息
        /// </summary>
        public VideoInfoModel VideoInfo { get; set; }


        private ObservableCollection<VideoQnModel> videos;
        /// <summary>
        /// 不同画质信息列表
        /// </summary>
        public ObservableCollection<VideoQnModel> Videos 
        {
            get {
                if (videos == null)
                    videos = new ObservableCollection<VideoQnModel>();
                return videos; }
            set
            {
                videos = value;
                this.RaisPropertyChanged(nameof(Videos));
            }
        }

        private ObservableCollection<AudioQnModel> audios;
        /// <summary>
        /// 不同音频信息列表
        /// </summary>
        public ObservableCollection<AudioQnModel> Audios 
        {
            get {
                if (audios == null)
                    audios = new ObservableCollection<AudioQnModel>();
                return audios; } 
            set
            {
                audios = value;
                this.RaisPropertyChanged(nameof(Audios));
            }
        }

        

        private string currentAudioBaseUrl;
        /// <summary>
        /// 当前下载的音频链接
        /// </summary>
        public string CurrentAudioBaseUrl
        {
            get { return currentAudioBaseUrl; }
            set
            {
                currentAudioBaseUrl = value;
                this.RaisPropertyChanged(nameof(CurrentAudioBaseUrl));
            }
        }

        
        private string currentVideoBaseUrl;
        /// <summary>
        /// 当前下载的视频链接
        /// </summary>
        public string CurrentVideoBaseUrl
        {
            get { return currentVideoBaseUrl; }
            set
            {
                currentVideoBaseUrl = value;
                this.RaisPropertyChanged(nameof(CurrentVideoBaseUrl));
            }
        }

        
        private long currentAudioSize;
        /// <summary>
        /// 当前下载音频m4s大小
        /// </summary>
        public long CurrentAudioSize {
            get { return currentAudioSize; }
            set
            {
                currentAudioSize = value;
                this.RaisPropertyChanged(nameof(CurrentAudioSize));
            }
        }
        private long currentVideoSize;
        /// <summary>
        /// 当前下载视频m4s大小
        /// </summary>
        public long CurrentVideoSize
        {
            get { return currentVideoSize; }
            set
            {
                currentVideoSize = value;
                this.RaisPropertyChanged(nameof(CurrentVideoSize));
            }
        }

        private string currentAudioQn;
        /// <summary>
        /// 当前下载的音频音质
        /// </summary>
        public string CurrentAudioQn
        {
            get { return currentAudioQn; }
            set
            {
                currentAudioQn = value;
                this.RaisPropertyChanged(nameof(CurrentAudioQn));
            }
        }
        private string currentAudioCodeCs;
        /// <summary>
        /// 当前下载的音频编码
        /// </summary>
        public string CurrentAudioCodeCs
        {
            get { return currentAudioCodeCs; }
            set
            {
                currentAudioCodeCs = value;
                this.RaisPropertyChanged(nameof(CurrentAudioCodeCs));
            }
        }

        private string currentVideoQn;
        /// <summary>
        /// 当前下载的视频画质
        /// </summary>
        public string CurrentVideoQn
        {
            get { return currentVideoQn; }
            set
            {
                currentVideoQn = value;
                this.RaisPropertyChanged(nameof(currentVideoQn));
            }
        }
        private string currentVideoCodeCs;
        /// <summary>
        /// 当前下载的视频编码
        /// </summary>
        public string CurrentVideoCodeCs
        {
            get { return currentVideoCodeCs; }
            set
            {
                currentVideoCodeCs = value;
                this.RaisPropertyChanged(nameof(CurrentVideoCodeCs));
            }
        }

        private int progressValue;
        /// <summary>
        /// 下载进度
        /// </summary>
        public int ProgressValue {
            get { return progressValue; }
            set
            {
                progressValue = value;
                this.RaisPropertyChanged(nameof(ProgressValue));
            }
        }

        private string tips;
        public string Tips
        {
            get { return tips; }
            set
            {
                tips = value;
                this.RaisPropertyChanged("Tips");
            }
        }

        private string tipsColor;
        public string TipsColor
        {
            get {
                if (tipsColor == null)
                    tipsColor = "#000000";

                return tipsColor; 
            }
            set
            {
                tipsColor = value;
                this.RaisPropertyChanged("TipsColor");
            }
        }

        private string tipsShow;
        public string TipsShow
        {
            get {
                if (tipsShow == null)
                    tipsShow = "Hidden";

                return tipsShow; 
            }
            set
            {
                tipsShow = value;
                this.RaisPropertyChanged("TipsShow");
            }
        }


        //private string filePath;
        ///// <summary>
        ///// 下载完成后文件路径
        ///// </summary>
        //public string FilePath
        //{
        //    get { return filePath; }
        //    set
        //    {
        //        filePath = value;

        //    }
        //}
    }
}
