using BiuBiuQ.Command;
using BiuBiuQ.Model.Video;
using BiuBiuQ.Service.Common;
using BiuBiuQ.Service.VideoList;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Xps.Serialization;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using BiuBiuQ.Service.Config;
using BiuBiuQ.Model.Config;
using BiuBiuQ.ViewModel.Config;
using BiuBiuQ.ViewModel.User;

namespace BiuBiuQ.ViewModel.VideoDown
{
    public enum ChangeDownListStatus
    {
        DownWaitToDowning,
        DowningToDownWait,
        DowningToDowned,
        DownedToDownWait
    }
    public class VideoDownWaitViewModel:NotificationObject
    {
        public DelegateCommand DownLoadCommand { get; set; }
        public DelegateCommand DownLoadAllCommand { get; set; }
        public DelegateCommand DeleteDownWaitCommand { get; set; }
        public DelegateCommand DeleteDownWaitAllCommand { get; set; }

        public DelegateCommand LoadAudioVideoCommand { get; set; }
        public DelegateCommand SelectAudioCommand { get; set; }
        public DelegateCommand SelectVideoCommand { get; set; }

        
        private readonly object _deleteWaitAlllock = new object();


        public VideoDownWaitViewModel() {

            DownLoadCommand = new DelegateCommand();
            DownLoadCommand.ExcuteAction = new Action<object>(DownLoadOne);

            DownLoadAllCommand = new DelegateCommand();
            DownLoadAllCommand.ExcuteAction = new Action<object>(DownloadAll);

            DeleteDownWaitCommand = new DelegateCommand();
            DeleteDownWaitCommand.ExcuteAction = new Action<object>(DeleteDownWaitRecord);

            DeleteDownWaitAllCommand = new DelegateCommand();
            DeleteDownWaitAllCommand.ExcuteAction = new Action<object>(DeleteDownWaitAllRecord);

            LoadAudioVideoCommand = new DelegateCommand();
            LoadAudioVideoCommand.ExcuteAction = new Action<object> (LoadAudioVideo);

            SelectAudioCommand = new DelegateCommand();
            SelectAudioCommand.ExcuteAction = new Action<object>(SelectAudio);

            SelectVideoCommand = new DelegateCommand();
            SelectVideoCommand.ExcuteAction = new Action<object>(SelectVideo);

            

            LoadDowingList();

            LoadSavePath();
        }

        public void LoadDowingList()
        {
            Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
            Task.Run(() => {
                dispatcher.Invoke(new Action(() =>
                {
                    VideoDownWaitStaticViewModel.Instance.GetVideoDownWaitList();
                }));
                
            });
        }
        

        /// <summary>
        /// 数据库中读取全局保存目录的配置信息
        /// </summary>
        public void LoadSavePath()
        {
            ConfigStaticViewModel.Instance.SavePath = ConfigService.GetSavePath();
        }
        /// <summary>
        /// 以【全局设置的保存路径\合集名称】作为视频的保存路径，如果未设置，默认保存到程序目录
        /// </summary>
        /// <param name="bvidTitle"></param>
        /// <returns></returns>
        public string SetDownloadPath(string bvid, string bvidTitle)
        {
            string bvidPath = string.Empty;
            string bvidTitlePath = string.Empty;
            //避免合集目录名太长出问题
            if (bvidTitle.Length > 30)
            {
                bvidTitlePath = bvidTitle.Substring(0, 30);
            }
            else
            {
                bvidTitlePath = bvidTitle;
            }
            
            if (string.IsNullOrEmpty(ConfigStaticViewModel.Instance.SavePath))
            {
                bvidPath = Path.Combine(Directory.GetCurrentDirectory(), bvidTitlePath);
            }
            else
            {
                bvidPath = Path.Combine(ConfigStaticViewModel.Instance.SavePath, bvidTitlePath);
            }

            if (!Directory.Exists(bvidPath))
                Directory.CreateDirectory(bvidPath);

            DBService.Instance.SaveCollectionPath(bvid, bvidTitlePath);

            return bvidPath;
        }

        public void DownloadAll(object parameter)
        {
            //其实这里不用传参，直接用VideoDownWaitStaticViewModel.Instance.VideoDownWaitDatas
            if (parameter != null)
            {
                ObservableCollection<VideoDownDataViewModel> vdm = (ObservableCollection<VideoDownDataViewModel>)parameter;

                Dispatcher dispatcher = Dispatcher.CurrentDispatcher;

                Task.Run(() =>
                {
                    List<Task> taskList = new List<Task>();
                    TaskFactory factory = new TaskFactory();
                    int taskMaxCount = ConfigStaticViewModel.Instance.DownTaskCount;//同时运行线程数

                    while (true)
                    {
                        //待下载已全部下载
                        if (vdm.Count == 0)
                        {
                            //等待下载中线程全部结束，然后清理跳出循环
                            Task.WaitAll(taskList.ToArray());
                            taskList.Clear();
                            break;
                        }

                        //下载线程数不能大于下载列表数
                        if(taskMaxCount > vdm.Count)
                            taskMaxCount = vdm.Count;

                        //开启多线程并发下载
                        if (taskList.Count < taskMaxCount)
                        {
                            int tCount = taskList.Count;
                            for (int i = 0; i < taskMaxCount - tCount; i++)
                            {
                                VideoDownDataViewModel vdData = vdm[i];

                                taskList.Add(factory.StartNew(() => {
                                    DownLoadFile(dispatcher, vdData);//这里比较奇怪，如果taskList.Add();没有其它执行语句，i会超范围，不知道i是跑哪里执行了
                                }));
                                
                                Thread.Sleep(50);//加个延时吧
                            }
                        }

                        //移除线程列表中运行结束的线程，让下载线程列表空出位置
                        for (int k = 0; k < taskList.Count; k++)
                        {
                            if (taskList[k].IsCompleted)
                            {
                                taskList[k].Dispose();
                                taskList.Remove(taskList[k]);
                            }
                        }

                        Thread.Sleep(1000);
                    }
                });
            }
        }

        public void DownLoadOne(object parameter)
        {
            if (parameter != null)
            {
                //如果是在view类下，直接使用this.Dispatcher.Invoke就可以异步进行UI更新
                //但如果是在view绑定的viewmodel类中，是不能直接进行this.Dispatcher.Invoke的
                //我们可以先获取viewmodel线程（和view线程相等）中的dispatcher，之后再在异步任务中使用dispatcher
                Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
                Task.Run(() =>
                {
                    VideoDownDataViewModel vd = (VideoDownDataViewModel)parameter;
                    
                    DownLoadFile(dispatcher, vd);
                });
                
            }
        }

        /// <summary>
        /// 转换下载状态列表
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <param name="vd"></param>
        /// <param name="type">-1下载中->待下载， 1待下载->下载中，-2下载完成->待下载，2下载中->下载完成，</param>
        public void ChangeDownListDispatcher(Dispatcher? dispatcher, VideoDownDataViewModel vd, ChangeDownListStatus type)
        {
            if(dispatcher != null)
            {
                dispatcher.Invoke(new Action(() => { ChangeDownList(vd, type); }));
            }
            else
            {
                ChangeDownList(vd, type);
            }
        }
        private void ChangeDownList(VideoDownDataViewModel vd, ChangeDownListStatus type)
        {
            switch (type)
            {
                case ChangeDownListStatus.DowningToDownWait:
                    VideoDowningStaticViewModel.Instance.VideoDowningDatas.Remove(vd);
                    VideoDownWaitStaticViewModel.Instance.VideoDownWaitDatas.Add(vd);
                    break;
                case ChangeDownListStatus.DownWaitToDowning:
                    VideoDownWaitStaticViewModel.Instance.VideoDownWaitDatas.Remove(vd);
                    VideoDowningStaticViewModel.Instance.VideoDowningDatas.Add(vd);
                    break;
                case ChangeDownListStatus.DownedToDownWait:
                    VideoDownedStaticViewModel.Instance.VideoDownedDatas.Remove(vd);
                    VideoDownWaitStaticViewModel.Instance.VideoDownWaitDatas.Add(vd);
                    break;
                case ChangeDownListStatus.DowningToDowned:
                    VideoDowningStaticViewModel.Instance.VideoDowningDatas.Remove(vd);
                    VideoDownedStaticViewModel.Instance.VideoDownedDatas.Add(vd);
                    break;
            }
        }
        public bool DownLoadFile(Dispatcher dispatcher, VideoDownDataViewModel vd)
        {
            //将待下载列表移到下载中列表
            ChangeDownListDispatcher(dispatcher, vd, ChangeDownListStatus.DownWaitToDowning);

            //如果没设置过音视频下载链接，设置一下
            if (string.IsNullOrEmpty(vd.CurrentAudioBaseUrl) || string.IsNullOrEmpty(vd.CurrentVideoBaseUrl))
                LoadAudiosVideos(dispatcher, vd);

            if (!string.IsNullOrEmpty(vd.CurrentVideoBaseUrl) && !string.IsNullOrEmpty(vd.CurrentAudioBaseUrl))
            {
                //设置文件保存路径
                string bvidSavePath = SetDownloadPath(vd.VideoCollection.Bvid, vd.VideoCollection.Title);

                try
                {
                    //设置视频缓存m4s文件名
                    string videoQnName = ConfigStaticViewModel.Instance.VideoQnName[vd.CurrentVideoQn] + "_" + vd.CurrentVideoCodeCs;
                    string videoExt = HttpService.GetUrlFileExtension(vd.CurrentVideoBaseUrl);
                    if (string.IsNullOrEmpty(videoExt)) 
                        videoExt = ".m4s";
                    string videoFileName = vd.VideoInfo.Page + "_" + vd.VideoInfo.Title + "_vedio_" + videoQnName + "_" + videoExt;
                    string videoFile = Path.Combine(bvidSavePath, videoFileName);

                    //设置音频缓存m4s文件名
                    string audioQnName = ConfigStaticViewModel.Instance.AudioQnName[vd.CurrentAudioQn] + "_" + vd.CurrentAudioCodeCs;
                    string audioExt = HttpService.GetUrlFileExtension(vd.CurrentAudioBaseUrl);
                    if (string.IsNullOrEmpty(audioExt))
                        audioExt = ".m4s";
                    string audioFileName = vd.VideoInfo.Page + "_"+ vd.VideoInfo.Title + "_audio_" + audioQnName + "_" + audioExt;
                    string audioFile = Path.Combine(bvidSavePath, audioFileName);

                    string mp4FileName = vd.VideoInfo.Page + "_" + vd.VideoInfo.Title + "_" +  videoQnName   + ".mp4";
                    string mp4File = Path.Combine(bvidSavePath, mp4FileName);

                    Console.WriteLine("Thread ID:" + Thread.CurrentThread.ManagedThreadId.ToString());
                    Console.WriteLine(videoFileName);
                    Console.WriteLine(audioFileName);

                    bool updateFlag = false;

                    //如果已存在视频文件，则无需重复下载
                    if (!File.Exists(mp4File))
                    {
                        bool vres = DownLoadAudioVideo(vd, vd.CurrentVideoBaseUrl, videoFileName, bvidSavePath).Result;//下载视频m4s
                        bool ares = DownLoadAudioVideo(vd, vd.CurrentAudioBaseUrl, audioFileName, bvidSavePath).Result;//下载音频m4s

                        if (vres && ares)
                        {
                            //合并音视频
                            int rest = FFmpegService.MegerVideoAudio(audioFile, videoFile, mp4File);
                            if (rest != -1)
                            {
                                //合并完成，删除m4s文件
                                File.Delete(audioFile);
                                File.Delete(videoFile);
                                updateFlag = true;
                            }
                        }
                        else
                        {
                            Console.WriteLine("下载完成失败" + videoFileName);
                        }
                    }
                    else
                    {
                        updateFlag = true;
                    }

                    if (updateFlag && File.Exists(mp4File))
                    {
                        //vd.FilePath = mp4File;
                        FileInfo fileInfo = new FileInfo(mp4File);
                        vd.VideoInfo.Size = fileInfo.Length;

                        //这两个更新操作都涉及到UI线程
                        dispatcher.Invoke(new Action(() => {
                            VideoDowningStaticViewModel.Instance.VideoDowningDatas.Remove(vd);//更新[下载中]列表
                            VideoDownedStaticViewModel.AddVideoDownedList(vd.VideoCollection, vd.VideoInfo);//更新[已下载]列表
                        }));

                        vd.VideoInfo.FilePath = mp4File;
                        //更新数据库状态
                        DBService.Instance.UpdateVideoStatusAndPath(vd.VideoInfo.Cid, vd.VideoInfo.Size, VideoStatus.Downed, mp4File);
                    }
                    else
                    {
                        Console.WriteLine("");
                        //将下载中列表移到待下载列表
                        ChangeDownListDispatcher(dispatcher, vd, ChangeDownListStatus.DowningToDownWait);
                    }
                }
                catch (Exception)
                {
                    ChangeDownListDispatcher(dispatcher, vd, ChangeDownListStatus.DowningToDownWait);
                    Console.WriteLine("下载过程出现异常");
                    return false;
                }

                return true;
            }
            else
            {
                Console.WriteLine("未获取到下载链接");
            }

            //将下载中列表移到待下载列表
            ChangeDownListDispatcher(dispatcher, vd, ChangeDownListStatus.DowningToDownWait);

            return false;
        }

        public void LoadAudioVideo(object parameter)
        {
            if(parameter!=null)
            {
                Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
                Task.Run(() =>
                {
                    LoadAudiosVideos(dispatcher, (VideoDownDataViewModel)parameter);
                });

            }

        }

        public void LoadAudiosVideos(Dispatcher dispatcher, VideoDownDataViewModel vd)
        {
            if(vd.Audios == null || ( vd.Audios.Count == 0 && vd.Videos.Count == 0))
            {
                VideoListService videoListService = new VideoListService();
                bool loadResult = videoListService.GetVideoAndAudioList(dispatcher, vd.VideoInfo, vd.Audios, vd.Videos, UserDataViewModel.Instance.Cookie);
                if (loadResult)
                {
                    //设置默认的下载链接
                    if (vd.Audios.Count > 0)
                    {
                        vd.CurrentAudioBaseUrl = vd.Audios[0].BaseUrl;
                        vd.CurrentAudioSize = vd.Audios[0].Size;
                        vd.CurrentAudioQn = vd.Audios[0].Qn;
                        vd.CurrentAudioCodeCs = vd.Audios[0].CodeCs;
                    }

                    if (vd.Videos.Count > 0)
                    {
                        vd.CurrentVideoBaseUrl = vd.Videos[0].BaseUrl;
                        vd.CurrentVideoSize = vd.Videos[0].Size;
                        vd.CurrentVideoQn = vd.Videos[0].Qn;
                        vd.CurrentVideoCodeCs = vd.Videos[0].CodeCs;
                    }

                }

            }
            
        }

        public void SelectAudio(object parameter)
        {
            Console.WriteLine("SelectAudio");
            object[] parameterArr = (object[])parameter;
            VideoDownDataViewModel vd = (VideoDownDataViewModel)parameterArr[0];
            AudioQnModel audioQnModel = (AudioQnModel)parameterArr[1];
            vd.CurrentAudioBaseUrl = audioQnModel.BaseUrl;
            vd.CurrentAudioSize = audioQnModel.Size;
            vd.CurrentAudioQn = audioQnModel.Qn;

        }
        public void SelectVideo(object parameter)
        {
            Console.WriteLine("SelectVideo");
            object[] parameterArr = (object[])parameter;
            VideoDownDataViewModel vd = (VideoDownDataViewModel)parameterArr[0];
            VideoQnModel videoQnModel = (VideoQnModel)parameterArr[1];
            vd.CurrentVideoBaseUrl = videoQnModel.BaseUrl;
            vd.CurrentVideoSize = videoQnModel.Size;
            vd.CurrentVideoQn = videoQnModel.Qn;

        }

        public void DeleteDownWaitRecord(object parameter)
        {
            if (parameter != null)
            {
                VideoDownDataViewModel vd = (VideoDownDataViewModel)parameter;
                vd.VideoInfo.Status = (int)VideoStatus.None;
                VideoDownWaitStaticViewModel.Instance.VideoDownWaitDatas.Remove(vd);
                DBService.Instance.UpdateVideoStatus(vd.VideoInfo.Cid, VideoStatus.None);
            }
        }

        /// <summary>
        /// 清空待下载列表
        /// </summary>
        /// <param name="parameter"></param>
        public void DeleteDownWaitAllRecord(object parameter)
        {
            //VideoDownDataViewModel vd = (VideoDownDataViewModel)parameter;

            Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
            Task.Run(() =>
            {
                lock(_deleteWaitAlllock)
                {
                    foreach (var vd in VideoDownWaitStaticViewModel.Instance.VideoDownWaitDatas)
                    {
                        dispatcher.Invoke(new Action(() => { vd.VideoInfo.Status = (int)VideoStatus.None; }));
                        DBService.Instance.UpdateVideoStatus(vd.VideoInfo.Cid, VideoStatus.None);
                    }
                    dispatcher.Invoke(new Action(() => { VideoDownWaitStaticViewModel.Instance.VideoDownWaitDatas.Clear(); }));
                }
                
            });
            
            
        }

        /// <summary>
        /// 下载视频文件
        /// </summary>
        /// <param name="vd"></param>
        /// <param name="savePath"></param>
        /// <returns></returns>
        public async Task<bool> DownLoadAudioVideo(VideoDownDataViewModel vd,string fileUrl, string fileName,  string savePath)
        {
            bool res = false;
            if (string.IsNullOrEmpty(fileUrl)) 
                return false;

            string referer = $"https://www.bilibili.com/video/{vd.VideoInfo.Bvid}?p={vd.VideoInfo.Page}";
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Referer", referer);
            try
            {
                res = await DownLoadHttpFile(vd, fileUrl, headers, fileName, savePath);
            }
            catch (Exception)
            {

                throw;
            }
            
            return res;
        }

        /// <summary>
        /// http文件下载
        /// 因为这里用到了VideoDownWaitDataViewModel，所以不放在Service了
        /// </summary>
        public  async Task<bool> DownLoadHttpFile(VideoDownDataViewModel vd, string fileUrl, Dictionary<string, string> headers, string fileName, string savePath)
        {

            string filePath = Path.Combine(savePath, fileName);


            bool resFlag = false;
            // 创建HttpClient实例
            using (HttpClient client = new HttpClient())
            {
                if(!headers.ContainsKey("User-Agent"))
                {
                    headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36");
                }
                foreach(var header in  headers)
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }

                
                // 发送GET请求并获取响应流
                using (HttpResponseMessage response = await client.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead))
                using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                {
                    // 获取文件总大小
                    long? totalBytes = response.Content.Headers.ContentLength;
                    if (totalBytes.HasValue && totalBytes > 0)
                    {
                        try
                        {
                            using (FileStream destinationStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                            {
                                byte[] buffer = new byte[4096];
                                long totalBytesRead = 0;
                                int bytesRead;
                                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                                {
                                    // 写入目标文件
                                    await destinationStream.WriteAsync(buffer, 0, bytesRead);

                                    // 更新进度条
                                    totalBytesRead += bytesRead;

                                    long progressRead = totalBytesRead * 100;//进度条是0-100的，所以这里 下载大小*100/总大小
                                    int progressPercentage = (int)(progressRead / totalBytes);
                                    vd.ProgressValue = progressPercentage;

                                    // 可以在这里添加其他处理逻辑，比如取消按钮的检查
                                    //TODO:
                                }
                                if (totalBytesRead == totalBytes)
                                {
                                    resFlag = true;
                                }
                            }
                        }
                        catch (Exception)
                        {

                            throw;
                        }
                        // 创建目标文件流
                        
                    }
                }
            }

            Console.WriteLine("文件下载完成");

            return resFlag;

        }

    }
}
