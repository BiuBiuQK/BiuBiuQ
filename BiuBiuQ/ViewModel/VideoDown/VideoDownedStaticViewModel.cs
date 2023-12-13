using BiuBiuQ.Model.Video;
using BiuBiuQ.Service.Common;
using BiuBiuQ.Service.VideoList;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace BiuBiuQ.ViewModel.VideoDown
{
    public class VideoDownedStaticViewModel:NotificationObject
    {
        private VideoDownedStaticViewModel() 
        { 
            if(videoDownedDatas == null)
            {
                videoDownedDatas = new ObservableCollection<VideoDownDataViewModel>();
            }
        }
        public static VideoDownedStaticViewModel Instance { get; set; } = new VideoDownedStaticViewModel();


        private ObservableCollection<VideoDownDataViewModel> videoDownedDatas;
        public ObservableCollection<VideoDownDataViewModel> VideoDownedDatas
        {
            get { return videoDownedDatas; }
            set
            {

                videoDownedDatas = value;
                this.RaisPropertyChanged(nameof(VideoDownedDatas));
            }
        }


        public static void GetVideoDownedList()
        {
            VideoListService videoListService = new VideoListService();
            List<VideoInfoModel> videoInfos = DBService.Instance.GetVideoListByStatus(VideoStatus.Downed);
            for (int i = 0; i < videoInfos.Count; i++)
            {
                VideoCollectionModel? videoCollectionModel = DBService.Instance.GetVideoCollectionModel(videoInfos[i].Bvid);
                if (videoCollectionModel != null)
                {
                    VideoDownDataViewModel vdData = new VideoDownDataViewModel();
                    vdData.VideoCollection = videoCollectionModel;
                    vdData.VideoInfo = videoInfos[i];
                    Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
                    videoListService.GetVideoAndAudioList(dispatcher,videoInfos[i], vdData.Audios, vdData.Videos);

                    Instance.videoDownedDatas.Add(vdData);
                }

            }
        }

        /// <summary>
        /// 新增视频信息到下载列表
        /// </summary>
        /// <param name="videoCollectionModel"></param>
        /// <param name="videoInfo"></param>
        public static void AddVideoDownedList(VideoCollectionModel videoCollectionModel, VideoInfoModel videoInfo)
        {
            //这里要判断一下是否已经存在该项
            if (IsContainsItem(videoInfo))
                return;

            if (videoCollectionModel != null && videoInfo != null)
            {
                VideoDownDataViewModel vdData = new VideoDownDataViewModel();
                vdData.VideoCollection = videoCollectionModel;
                vdData.VideoInfo = videoInfo;

                Instance.videoDownedDatas.Add(vdData);
            }

        }

        /// <summary>
        /// 判断是否已经包含该视频cid
        /// </summary>
        /// <param name="videoInfo"></param>
        /// <returns></returns>
        public static bool IsContainsItem(VideoInfoModel videoInfo)
        {
            foreach (var item in Instance.videoDownedDatas)
            {
                if (item.VideoInfo.Cid == videoInfo.Cid)
                    return true;
            }
            return false;
        }
    }
}
