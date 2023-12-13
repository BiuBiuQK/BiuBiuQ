using BiuBiuQ.Model.Video;
using BiuBiuQ.Service.Common;
using BiuBiuQ.Service.VideoList;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiuBiuQ.ViewModel.VideoDown
{
    /// <summary>
    /// 这个在获取视频，下载页面都需要用到，用静态单例
    /// </summary>
    public class VideoDownWaitStaticViewModel : NotificationObject
    {
        private VideoDownWaitStaticViewModel()
        {
            if (videoDownWaitDatas == null)
            {
                videoDownWaitDatas = new ObservableCollection<VideoDownDataViewModel>();
            }
        }
        public static VideoDownWaitStaticViewModel Instance { get; set; } = new VideoDownWaitStaticViewModel();

        private ObservableCollection<VideoDownDataViewModel> videoDownWaitDatas;
        public ObservableCollection<VideoDownDataViewModel> VideoDownWaitDatas
        {
            get { return videoDownWaitDatas; }
            set
            {

                videoDownWaitDatas = value;
                RaisPropertyChanged(nameof(VideoDownWaitDatas));
            }
        }


        /// <summary>
        /// 数据库中获取待下载列表
        /// </summary>
        public void GetVideoDownWaitList()
        {
            VideoListService videoListService = new VideoListService();
            List<VideoInfoModel> videoInfos = DBService.Instance.GetVideoListByStatus(VideoStatus.DownWait);
            for (int i = 0; i < videoInfos.Count; i++)
            {
                VideoCollectionModel? videoCollectionModel = DBService.Instance.GetVideoCollectionModel(videoInfos[i].Bvid);
                if (videoCollectionModel != null)
                {
                    //ObservableCollection<VideoQnModel> videoModels = new ObservableCollection<VideoQnModel>();
                    //ObservableCollection<AudioQnModel> audioModels = new ObservableCollection<AudioQnModel>();

                    VideoDownDataViewModel vdDatas = new VideoDownDataViewModel();
                    vdDatas.VideoCollection = videoCollectionModel;
                    vdDatas.VideoInfo = videoInfos[i];
                    //vdDatas.Videos = videoModels;
                    //vdDatas.Audios = audioModels;

                    Instance.videoDownWaitDatas.Add(vdDatas);
                }

            }
        }

        /// <summary>
        /// 新增视频信息到下载列表
        /// </summary>
        /// <param name="videoCollectionModel"></param>
        /// <param name="videoInfo"></param>
        public void AddVideoDownWaitList(VideoCollectionModel videoCollectionModel, VideoInfoModel videoInfo)
        {
            //这里要判断一下是否已经存在该项
            if (IsContainsItem(videoInfo))
                return;

            if (videoCollectionModel != null && videoInfo != null)
            {
                //ObservableCollection<VideoQnModel> videoModels = new ObservableCollection<VideoQnModel>();
                //ObservableCollection<AudioQnModel> audioModels = new ObservableCollection<AudioQnModel>();

                VideoDownDataViewModel vdDatas = new VideoDownDataViewModel();
                vdDatas.VideoCollection = videoCollectionModel;
                vdDatas.VideoInfo = videoInfo;
                //vdDatas.Videos = videoModels;
                //vdDatas.Audios = audioModels;
                //SetDescTips(vdDatas);
                vdDatas.TipsShow = "Hidden";

                Instance.videoDownWaitDatas.Add(vdDatas);
            }

        }
        public void SetDescTips(VideoDownDataViewModel vdDatas)
        {
            vdDatas.Tips = "简介：" + vdDatas.VideoCollection.Desc.Replace("\n", " ").Substring(0, 50) + "...";
            vdDatas.TipsColor = "#666666";
            vdDatas.TipsShow = "Visible";
        }

        /// <summary>
        /// 判断是否已经包含该视频cid
        /// </summary>
        /// <param name="videoInfo"></param>
        /// <returns></returns>
        public bool IsContainsItem(VideoInfoModel videoInfo)
        {
            foreach (var item in Instance.videoDownWaitDatas)
            {
                if (item.VideoInfo.Cid == videoInfo.Cid)
                    return true;

            }
            return false;
        }

    }
}
