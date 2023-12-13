using BiuBiuQ.Command;
using BiuBiuQ.Model.Video;
using BiuBiuQ.Service.Common;
using BiuBiuQ.Service.VideoList;
using BiuBiuQ.ViewModel.VideoDown;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace BiuBiuQ.ViewModel.VideoList
{
    public class VideoListViewModel: NotificationObject
    {
        public DelegateCommand LoadVideoCommand {  get; set; }
        public DelegateCommand AddToDowningCommand { get; set; }

        public DelegateCommand SelectAllCommand { get; set; }

        private string url;
        /// <summary>
        /// bilibili链接地址
        /// </summary>
        public string Url 
        { 
            get { return url; }
            set { url = value; }
        }

        private VideoCollectionModel videoCollection;
        /// <summary>
        /// 视频合集信息
        /// </summary>
        public VideoCollectionModel VideoCollection
        {
            get { return videoCollection; }
            set
            {
                videoCollection = value;
                this.RaisPropertyChanged("VideoCollection");
            }
        }
        private ObservableCollection<VideoListDataViewModel> videoInfos;
        /// <summary>
        /// 视频列表信息
        /// </summary>
        public ObservableCollection<VideoListDataViewModel> VideoInfos
        {
            get {
                if(videoInfos == null)
                    videoInfos = new ObservableCollection<VideoListDataViewModel>();

                return videoInfos; 
            }
            set 
            { 
                videoInfos = value;
                this.RaisPropertyChanged("VideoInfos");
            }
        }


        private bool isAllSelected;
        public bool IsAllSelected
        {
            get { return isAllSelected; }

            set
            {
                isAllSelected = value;
                this.RaisPropertyChanged("IsAllSelected");
            }
        }

        public VideoListViewModel()
        {
            LoadVideoCommand = new DelegateCommand();
            LoadVideoCommand.ExcuteAction = new Action<object>(LoadVideo);

            SelectAllCommand = new DelegateCommand();
            SelectAllCommand.ExcuteAction = new Action<object>(SelectAll);

            AddToDowningCommand = new DelegateCommand();
            AddToDowningCommand.ExcuteAction = new Action<object>(AddToDowning);

        }

        /// <summary>
        /// 获取视频列表
        /// </summary>
        /// <param name="parameter"></param>
        public void LoadVideo(object parameter)
        {
            if (IsAllSelected) IsAllSelected = false;

            if (string.IsNullOrEmpty(Url)) return;

            VideoListService apiUrlVideoListService = new VideoListService();
            VideoListType? videoList = apiUrlVideoListService.GetVideoList(Url);
            if (videoList != null && videoList.HasValue)
            {
                VideoCollection = videoList.Value.videoCollection;

                //VideoInfos = new ObservableCollection<VideoInfoViewMode>(videoList.Value.videoInfos);
                VideoInfos = new ObservableCollection<VideoListDataViewModel>();
                foreach (var  videoInfo in videoList.Value.videoInfos)
                {
                    VideoListDataViewModel videoInfoViewMode = new VideoListDataViewModel();
                    videoInfoViewMode.VideoInfoModel = videoInfo;
                    videoInfoViewMode.IsSelected = false;
                    VideoInfos.Add(videoInfoViewMode);
                }

                DBService.Instance.VideosSaveToDb((VideoListType)videoList);
            }
        }

        /// <summary>
        /// 全选视频列表
        /// </summary>
        /// <param name="parameter"></param>
        public void SelectAll(object parameter)
        {
            bool selected = (bool)parameter;
            foreach (var videoInfo in VideoInfos)
            {
                videoInfo.IsSelected = selected;
            }
        }

        /// <summary>
        /// 添加到下载列表
        /// </summary>
        /// <param name="parameter"></param>
        public void AddToDowning(object parameter)
        {
            Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
            Task.Factory.StartNew(() =>
            {
                if (parameter != null)
                {
                    //这个是单行操作，暂时不用
                    VideoListDataViewModel videoListData = (VideoListDataViewModel)parameter;
                    videoListData.Status = 1;
                    DBService.Instance.UpdateVideoStatus(videoListData.Cid, VideoStatus.DownWait);
                    VideoInfos.Remove(videoListData);
                    VideoDownWaitStaticViewModel.Instance.AddVideoDownWaitList(videoCollection, videoListData.VideoInfoModel);
                }
                else
                {
                    string updateSql = string.Empty;
                    //选中视频添加到下载列表
                    ObservableCollection<VideoListDataViewModel> videoListData = new ObservableCollection<VideoListDataViewModel>(VideoInfos);
                    foreach (var videoInfo in videoListData)
                    {
                        if (videoInfo.IsSelected)
                        {
                            videoInfo.Status = 1;
                            //这里用一次性插入，不要单个插入
                            if (videoInfo == videoListData.Last())
                                updateSql += $"cid='{videoInfo.Cid}'";
                            else
                                updateSql += $"cid='{videoInfo.Cid}' or ";

                            
                                dispatcher.Invoke(new Action(() =>
                                {
                                    //没有全选就一个一个移除，如果数量多的话会稍微有点慢
                                    if (!IsAllSelected)
                                    {
                                        VideoInfos.Remove(videoInfo);
                                    }
                                    VideoDownWaitStaticViewModel.Instance.AddVideoDownWaitList(videoCollection, videoInfo.VideoInfoModel);
                                }));
                            
                        }
                    }

                    if(!string.IsNullOrEmpty(updateSql))
                    {
                        updateSql = "set status=1 where " + updateSql;
                        DBService.Instance.UpdateVideoStatusAll(updateSql);
                    }  

                    //全选就直接清空
                    if (IsAllSelected)
                    {
                        dispatcher.Invoke(new Action(() => { VideoInfos.Clear(); }));
                    }
                    videoListData.Clear();
                }
            });
            
            
        }
    }
}
