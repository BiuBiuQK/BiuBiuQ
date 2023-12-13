using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiuBiuQ.Model.Video
{
    /// <summary>
    /// 视频合集信息
    /// </summary>
    public class VideoCollectionModel
    {

        /// <summary>
        /// 视频合集id
        /// </summary>
        public string Bvid { get; set; }

        /// <summary>
        /// 合集中视频数量
        /// </summary>
        public int Videos { get; set; }

        /// <summary>
        /// 视频合集所属标签
        /// </summary>
        public string Tname { get; set; }

        /// <summary>
        /// 视频合集展示图片，图片链接
        /// </summary>
        public string Pic { get; set; }

        /// <summary>
        /// 合集标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 发布时间
        /// </summary>
        public long Ctime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public long Pubdate { get; set; }

        /// <summary>
        /// 备注描述
        /// </summary>
        public string Desc { get; set; }

        /// <summary>
        /// 发布者id ["data"]["owner"]["mid"]
        /// </summary>
        public string OwnerMid { get; set; }

        /// <summary>
        /// 发布者昵称 ["data"]["owner"]["name"]
        /// </summary>
        public string OwnerName { get; set; }

        /// <summary>
        /// 发布者头像 ["data"]["owner"]["face"]
        /// </summary>
        public string OwnerFace { get; set; }

    }
}
