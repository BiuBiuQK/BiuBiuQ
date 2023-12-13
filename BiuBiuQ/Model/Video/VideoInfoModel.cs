using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiuBiuQ.Model.Video
{
    /// <summary>
    /// 单个视频信息
    /// </summary>
    public class VideoInfoModel
    {

        /// <summary>
        /// 视频合集id
        /// </summary>
        public string Bvid { get; set; }

        /// <summary>
        /// 视频id
        /// </summary>
        public string Cid { get; set; }

        /// <summary>
        /// 合集中的p，就是合集中的第p集
        /// </summary>
        public string Page { get; set; }
        /// <summary>
        /// 视频标题
        /// </summary>
        public string Title { get; set; }


        /// <summary>
        /// 视频时长 秒
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// 下载完成后，视频和音频合成后的文件大小，保存到数据库中用
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// 视频状态：0:无，1:待下载，2:正在下载，3已下载
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 文件全路径
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// 数据库中保存创建该视频信息的时间 自定义
        /// </summary>
        public long CreateTime { get; set; }

        /// <summary>
        /// 数据库中保存下载完该视频的事件 自定义
        /// </summary>
        public long DownTime { get; set; }

    }
}
