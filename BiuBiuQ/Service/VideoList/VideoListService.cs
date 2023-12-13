using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Text.Json.Nodes;
using System.IO;
using System.Net.Http;
using System.Windows.Controls;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Windows.Xps;
using BiuBiuQ.Service.Common;
using BiuBiuQ.Model.Video;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using Microsoft.VisualBasic.Devices;

namespace BiuBiuQ.Service.VideoList
{
    public struct VideoListType
    {
        public VideoCollectionModel videoCollection;
        public List<VideoInfoModel> videoInfos;

    }
    /// <summary>
    /// bilibili api获取视频信息
    /// </summary>
    public class VideoListService 
    {
        /// <summary>
        /// 从网页获取视频合集
        /// </summary>
        /// <returns></returns>
        public VideoListType? GetVideoList(string url, string cookie = "")
        {
            VideoListType videoList;
            
            //从网页实现获取视频合集
            string bvid = GetBvidFromUrl(url);
            if (string.IsNullOrEmpty(bvid)) return null;
            string videoCollectionJson = GetVideoListJsonFromApiUrl(bvid);
            videoList.videoCollection = GetVideoCollectionFromJsonResult(videoCollectionJson);
            videoList.videoInfos = GetVideoInfoListFromJsonResult(videoCollectionJson);
            
            return videoList;
        }

        /// <summary>
        /// 从bilibili网址中截取bvid
        /// url是固定格式的，所以截取还是比较方便的，这里就不用正则了
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string GetBvidFromUrl(string url)
        {
            //视频url都是以这个开头的，如果不是的话说明不是有效的视频链接，直接返回空
            string startStr = "https://www.bilibili.com/video/BV";
            string bvid = string.Empty;
            if (url.StartsWith(startStr))
            {
                string urlStr = url.Replace(startStr, "BV");
                int index = urlStr.IndexOf("/");
                if(index < 0)
                {
                    index = urlStr.IndexOf("?");
                }
                if (index != -1)
                {
                    bvid = urlStr.Substring(0, index);
                    Console.WriteLine($"bvid:{bvid}");
                }
                else
                {
                    bvid = urlStr.Substring(0, 12);//直接取12位长度bvid
                }
            }
            return bvid;
        }

        /// <summary>
        /// bilibili有专门的api可以直接获取视频以及合集信息
        /// 根据bilibili api获取视频合集列表json文本
        /// </summary>
        /// <param name="bvid"></param>
        /// <param name="cookie"></param>
        /// <returns></returns>
        public string GetJsonByApiUrl(string apiUrl, string cookie="")
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            dict.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36");
            if(!string.IsNullOrEmpty(cookie))
            {
                dict.Add("Cookie", cookie);
            }
            
            string res = HttpService.HttpGetContent(apiUrl, dict);

            return res;
        }
        public string GetVideoListJsonFromApiUrl(string bvid, string cookie="")
        {
            string apiUrl = $"https://api.bilibili.com/x/web-interface/view?bvid={bvid}";

            string result = GetJsonByApiUrl(apiUrl, cookie);

            return result;
        }

        /// <summary>
        /// 根据json文本获取合集信息
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public VideoCollectionModel GetVideoCollectionFromJsonResult(string jsonString)
        {
            VideoCollectionModel videoCollection = new VideoCollectionModel();
            try
            {
                JsonNode? jsonNode = JsonNode.Parse(jsonString);
                if(jsonNode != null )
                {
                    videoCollection.Bvid = jsonNode["data"]["bvid"].ToString();
                    videoCollection.Videos = Convert.ToInt32(jsonNode["data"]["videos"].ToString());
                    videoCollection.Tname = jsonNode["data"]["tname"].ToString();
                    videoCollection.Pic = jsonNode["data"]["pic"].ToString();
                    videoCollection.Title = jsonNode["data"]["title"].ToString();
                    videoCollection.Pubdate = Convert.ToInt64(jsonNode["data"]["pubdate"].ToString());
                    videoCollection.Ctime = Convert.ToInt64(jsonNode["data"]["ctime"].ToString());
                    videoCollection.Desc = jsonNode["data"]["desc"].ToString();
                    videoCollection.OwnerMid = jsonNode["data"]["owner"]["mid"].ToString();
                    videoCollection.OwnerName = jsonNode["data"]["owner"]["name"].ToString();
                    videoCollection.OwnerFace = jsonNode["data"]["owner"]["face"].ToString();
                }
                return videoCollection;
            }
            catch (Exception)
            {
                Console.WriteLine("json结果解析失败，或者json不是成功后的正常结果文本");
                return videoCollection;
            }
        }

        public List<VideoInfoModel> GetVideoInfoListFromJsonResult(string jsonString)
        {
            List<VideoInfoModel> videoInfoList = new List<VideoInfoModel>();
            try
            {
                JsonNode? jsonNode = JsonNode.Parse(jsonString);
                if (jsonNode != null)
                {
                    var pagesNode = jsonNode["data"]["pages"];
                    //节点是数组，转为json数组遍历
                    JsonArray pages = pagesNode.AsArray();
                    for (int i = 0; i < pages.Count; i++)
                    {
                        VideoInfoModel videoInfoModel = new VideoInfoModel();
                        videoInfoModel.Bvid = jsonNode["data"]["bvid"].ToString(); ;
                        videoInfoModel.Cid = pages[i]["cid"].ToString();
                        videoInfoModel.Page = pages[i]["page"].ToString();
                        videoInfoModel.Title = pages[i]["part"].ToString();
                        videoInfoModel.Duration = Convert.ToInt32(pages[i]["duration"].ToString());

                        videoInfoList.Add(videoInfoModel);
                    }
                }

            }
            catch (Exception)
            {
                return videoInfoList;
                throw;
            }

            return videoInfoList;
        }

        /// <summary>
        /// 根据bvid和cid获取 视频/音频 信息列表
        /// </summary>
        /// <param name="bvid"></param>
        /// <param name="cid"></param>
        /// <returns></returns>
        public bool GetVideoAndAudioList(Dispatcher dispatcher, VideoInfoModel videoInfo, ObservableCollection<AudioQnModel> audios, ObservableCollection<VideoQnModel> videos, string cookie="")
        {
            string bvid = videoInfo.Bvid;
            string cid = videoInfo.Cid;
            string page = videoInfo.Page;

            string jsonString = GetVideoInfoJsonMoreFromApiUrl(bvid, cid, cookie);
            //Console.WriteLine(jsonString);
            try
            {
                //数组解析
                JsonNode json = JsonNode.Parse(jsonString);

                //音频信息列表
                var audioNode = json["data"]["dash"]["audio"];
                JsonArray audioArr = audioNode.AsArray();
                for (int i = 0; i < audioArr.Count; i++)
                {
                    AudioQnModel audio = new AudioQnModel();
                    audio.Bvid = bvid;
                    audio.Cid = cid;
                    audio.Qn = audioArr[i]["id"].ToString();
                    audio.CodeCs = audioArr[i]["codecs"].ToString();
                    audio.BaseUrl = audioArr[i]["baseUrl"].ToString();
                    //Task.Run(() => {
                         audio.Size = GetHttpFileSize(audio.BaseUrl, bvid, page); 
                    //});

                    dispatcher.Invoke(new Action(() => { audios.Add(audio); }));
                }

                //视频信息列表
                var videoNode = json["data"]["dash"]["video"];
                //节点是数组，转为json数组遍历
                JsonArray videoArr = videoNode.AsArray();
                for (int i = 0; i < videoArr.Count; i++)
                {
                    VideoQnModel video =new VideoQnModel();
                    video.Bvid = bvid;
                    video.Cid = cid;
                    video.Qn = videoArr[i]["id"].ToString();
                    video.CodeCs = videoArr[i]["codecs"].ToString();
                    video.BaseUrl = videoArr[i]["baseUrl"].ToString();
                    //Task.Run(() => {
                        
                        video.Size = GetHttpFileSize(video.BaseUrl, bvid, page);
                    //});
                    dispatcher.Invoke(new Action(() => { videos.Add(video); }));
                    
                }

                return true;
            }
            catch (Exception)
            {
                
                Console.WriteLine("获取视频/音频信息列表出错");
                return false;
                //throw;
                
            }
            
        }


        /// <summary>
        /// 获取到视频直链信息，音视频合在一起的mp4文件，不过这个获取到的最高好像只有720p的
        /// </summary>
        /// <param name="bvid"></param>
        /// <param name="cid"></param>
        /// <param name="qn"></param>
        /// <param name="cookie"></param>
        /// <returns></returns>
        public string GetVideoInfoJsonFromApiUrl(string bvid, string cid, int qn, string cookie="")
        {
            string apiUrl = $"https://api.bilibili.com/x/player/playurl?otype=json&bvid={bvid}&cid={cid}&qn={qn.ToString()}";

            string result =  GetJsonByApiUrl(apiUrl, cookie);

            return result;
        }

        /// <summary>
        /// 获取视频所有分辨率直链链接，视频/音频是分开的，需要用ffmpeg转
        /// ffmpeg.exe -i video.m4s -i audio.m4s -codec copy Output.mp4
        /// 不带cookie只能获取到360p的
        /// </summary>
        /// <param name="bvid"></param>
        /// <param name="cid"></param>
        /// <param name="qn"></param>
        /// <param name="cookie"></param>
        /// <returns></returns>
        public string GetVideoInfoJsonMoreFromApiUrl(string bvid, string cid, string cookie = "", string qn="120" )
        {
            string apiUrl = $"https://api.bilibili.com/x/player/playurl?bvid={bvid}&cid={cid}&qn={qn}&fourk=1&fnval=16";

            string result = GetJsonByApiUrl(apiUrl, cookie);

            return result;
        }


        /// <summary>
        /// 获取文件大小
        /// </summary>
        /// <param name="fileUrl"></param>
        /// <returns></returns>
        private long GetHttpFileSize(string fileUrl, string bvid, string page)
        {
            Dictionary<string,string> headers = new Dictionary<string,string>();
            
            string referer = $"https://www.bilibili.com/video/{bvid}?p={page}";
            headers.Add("Referer", referer);

            long size = HttpService.GetHttpFileSize(fileUrl, headers);

            return size;
        }

        
    }
}
