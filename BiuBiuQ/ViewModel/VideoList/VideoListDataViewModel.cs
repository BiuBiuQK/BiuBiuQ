using BiuBiuQ.Model.Video;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiuBiuQ.ViewModel.VideoList
{
    public class VideoListDataViewModel:NotificationObject
    {
        /// <summary>
        /// 视频信息
        /// </summary>
        private VideoInfoModel videoInfoModel;
        public VideoInfoModel VideoInfoModel
        {
            get { 
                if(videoInfoModel == null)
                    videoInfoModel = new VideoInfoModel();

                return videoInfoModel; 
            }
            set
            {
                videoInfoModel = value;
                this.RaisPropertyChanged(nameof(VideoInfoModel));
            }
        }


        /// <summary>
        /// 视频合集id
        /// </summary>
        public string Bvid {
            get {return videoInfoModel.Bvid; } 
            set
            {
                videoInfoModel.Bvid = value;
                this.RaisPropertyChanged(nameof(Bvid));
            }
        }

        /// <summary>
        /// 视频id
        /// </summary>
        public string Cid {
            get { return videoInfoModel.Cid; }
            set
            {
                videoInfoModel.Cid = value;
                this.RaisPropertyChanged(nameof(Cid));
            }
        }

        /// <summary>
        /// 合集中的p，就是合集中的第p集
        /// </summary>
        public string Page {
            get { return videoInfoModel.Page; }
            set
            {
                videoInfoModel.Page = value;
                this.RaisPropertyChanged(nameof(Page));
            }
        }
        /// <summary>
        /// 视频标题
        /// </summary>
        public string Title {
            get { return videoInfoModel.Title; }
            set
            {
                videoInfoModel.Page = value;
                this.RaisPropertyChanged(nameof(Title));
            }
        }


        /// <summary>
        /// 视频时长 秒
        /// </summary>
        public int Duration {
            get { return videoInfoModel.Duration; }
            set
            {
                videoInfoModel.Duration = value;
                this.RaisPropertyChanged(nameof(Duration));
            }
        }

        /// <summary>
        /// 下载完成后，视频和音频合成后的文件大小，保存到数据库中用
        /// </summary>
        public long Size {
            get { return videoInfoModel.Size; }
            set
            {
                videoInfoModel.Size = value;
                this.RaisPropertyChanged(nameof(Size));
            }
        }

        /// <summary>
        /// 视频状态：0:无，1:待下载，2:已下载
        /// </summary>
        public int Status {
            get { return videoInfoModel.Status; }
            set
            {
                videoInfoModel.Status = value;
                this.RaisPropertyChanged(nameof(Status));
            }
        }

        /// <summary>
        /// 数据库中保存创建该视频信息的时间 自定义
        /// </summary>
        public long CreateTime {
            get { return videoInfoModel.CreateTime; }
            set
            {
                videoInfoModel.CreateTime = value;
                this.RaisPropertyChanged(nameof(CreateTime));
            }
        }

        /// <summary>
        /// 数据库中保存下载完该视频的事件 自定义
        /// </summary>
        public long DownTime {
            get { return videoInfoModel.DownTime; }
            set
            {
                videoInfoModel.DownTime = value;
                this.RaisPropertyChanged(nameof(DownTime));
            }
        }
        

        //是否选中
        private bool isSelected;
        public bool IsSelected 
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
                this.RaisPropertyChanged(nameof(isSelected));
            }
        }
    }
}
