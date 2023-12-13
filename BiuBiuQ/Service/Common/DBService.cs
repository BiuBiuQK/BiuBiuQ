using BiuBiuQ.Model;
using BiuBiuQ.Model.Video;
using BiuBiuQ.Model.User;
using BiuBiuQ.Service.VideoList;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BiuBiuQ.Service.Common
{
    public enum VideoStatus
    {
        None,           //0
        DownWait,       //1等待下载或正在下载
        Downing,        //2等待下载或正在下载
        Downed          //3下载完成
    }

    /// <summary>
    /// 所有的数据库操作都放到这里了
    /// 可以看情况是否拆分为单独service
    /// </summary>
    public class DBService : IDisposable
    {
        private DBService()
        {
            if (connection == null)
                connection = new SqliteConnection(dataSource);

            if (connection != null && connection.State != ConnectionState.Open)
                connection.Open();

        }
        public static DBService Instance { get; private set; } = new DBService();

        private readonly string dataSource = "data source=data.db";
        private readonly string videoInfoTable = "video_info";
        private readonly string videoCollectionTable = "video_collection";
        private readonly string videoQnTable = "video_qn";
        private readonly string audioQnTable = "audio_qn";
        private readonly string userTable = "user";
        
        public SqliteConnection connection;


        #region 视频操作区块
        public void VideosSaveToDb(VideoListType videoListType)
        {
            if (videoListType.videoCollection != null)
            {
                if (IsDbNeedUpdate(videoListType.videoCollection.Bvid, videoListType.videoCollection.Pubdate))
                {
                    VideoCollectionSaveToDb(videoListType.videoCollection);

                    if (videoListType.videoInfos != null)
                    {
                        VideoInfoListSaveToDb(videoListType.videoInfos);
                    }
                }
            }
        }

        /// <summary>
        /// 合集信息保存到数据库
        /// </summary>
        /// <param name="videoCollectionModel"></param>
        /// <returns></returns>
        public int VideoCollectionSaveToDb(VideoCollectionModel videoCollectionModel)
        {
            int res = 0;

            //避免commandtext代码太长，保存到变量中
            VideoCollectionModel vc = videoCollectionModel;
            var command = connection.CreateCommand();
            command.CommandText = $"insert into {videoCollectionTable} (bvid,videos,tname,title,desc,pic,ctime,pubdate,ownermid,ownername,ownerface) " +
                $"values('{vc.Bvid}','{vc.Videos}','{vc.Tname}', '{vc.Title}','{vc.Desc}','{vc.Pic}','{vc.Ctime}','{vc.Pubdate}','{vc.OwnerMid}','{vc.OwnerName}','{vc.OwnerFace}')";

            try
            {
                res = command.ExecuteNonQuery();
            }
            catch (Exception)
            {
                Console.WriteLine("VideoCollectionSaveToDb 数据库插入出错");
                if (command.Transaction != null)
                    command.Transaction.Rollback();
                //throw;
            }
            return res;
        }
        public int SaveCollectionPath(string bvid, string path)
        {
            int res = 0;
            var command = connection.CreateCommand(); 
            command.CommandText = $"update {videoCollectionTable} set path='{path}' where bvid='{bvid}'";
            res = command.ExecuteNonQuery();

            return res;
        }

        /// <summary>
        /// 视频信息列表保存到数据库
        /// </summary>
        /// <param name="videoInfoModels"></param>
        /// <returns></returns>
        public int VideoInfoListSaveToDb(List<VideoInfoModel> videoInfoModels)
        {
            if (videoInfoModels.Count == 0) return 0;

            int res = 0;

            //构造insert 多条记录一次性插入 文本
            string cmdText = $"insert into {videoInfoTable} (bvid,cid,page,title,duration,createtime) values";
            long createTime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            //多条记录合并
            for (int i = 0; i < videoInfoModels.Count; i++)
            {
                cmdText += $"('{videoInfoModels[i].Bvid}','{videoInfoModels[i].Cid}','{videoInfoModels[i].Page}','{videoInfoModels[i].Title}','{videoInfoModels[i].Duration}', '{createTime}')";
                if (i == videoInfoModels.Count - 1)
                {
                    cmdText += ";";
                }
                else
                {
                    cmdText += ",";
                }

            }

            var command = connection.CreateCommand();
            command.CommandText = cmdText;
            try
            {
                res = command.ExecuteNonQuery();
            }
            catch (Exception)
            {
                Console.WriteLine("VideoInfoListSaveToDb 数据库插入出错");
                //throw;
            }

            return res;
        }

        public bool IsDbNeedUpdate(string bvid, long pubdate)
        {
            Console.WriteLine(Path.GetFullPath(dataSource));
            VideoCollectionModel? videoCollectionModel = GetVideoCollectionModel(bvid);
            if (videoCollectionModel != null)
            {
                if (pubdate == videoCollectionModel.Pubdate)
                {
                    //视频合集里有记录，但是视频信息表里没有记录或者视频数量不对
                    if (GetVideosCount(bvid) != videoCollectionModel.Videos)
                    {
                        DeleteVideoCollection(bvid);
                        return true;
                    }

                    return false;
                }
            }


            return true;
        }

        /// <summary>
        /// 获取视频合集信息
        /// </summary>
        /// <param name="bvid"></param>
        /// <returns></returns>
        public VideoCollectionModel? GetVideoCollectionModel(string bvid)
        {
            VideoCollectionModel? videoCollectionModel = null;

            var command = connection.CreateCommand();
            command.CommandText = $"select * from {videoCollectionTable} where bvid='{bvid}'";
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    videoCollectionModel = new VideoCollectionModel();
                    //videoCollectionModel.Bvid = bvid;
                    //videoCollectionModel.Tname = reader.GetFieldValue<string>("tname");
                    //videoCollectionModel.Videos = reader.GetFieldValue<int>("videos");
                    //videoCollectionModel.Title = reader.GetFieldValue<string>("title");
                    //videoCollectionModel.Desc = reader.GetFieldValue<string>("desc");
                    //videoCollectionModel.Ctime = reader.GetFieldValue<long>("ctime");
                    //videoCollectionModel.Pubdate = reader.GetFieldValue<long>("pubdate");
                    //videoCollectionModel.OwnerName = reader.GetFieldValue<string>("owner_name");
                    //videoCollectionModel.OwnerMid = reader.GetFieldValue<string>("owner_mid");
                    //videoCollectionModel.OwnerFace = reader.GetFieldValue<string>("owner_face");

                    //直接读取数据库记录到类对象属性
                    SetModelValue(videoCollectionModel, reader);
                }
            }

            return videoCollectionModel;
        }

        /// <summary>
        /// 数据库中读取对应合集bvid的视频数量
        /// </summary>
        /// <param name="bvid"></param>
        /// <returns></returns>
        public int GetVideosCount(string bvid)
        {
            int count = 0;

            var command = connection.CreateCommand();

            command.CommandText = $"select count(*) from {videoInfoTable} where bvid='{bvid}'";

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    count = reader.GetInt32(0);
                }
            }

            return count;

        }

        /// <summary>
        /// 删除视频合集信息
        /// </summary>
        /// <param name="bvid"></param>
        /// <returns></returns>
        public int DeleteVideoCollection(string bvid)
        {
            int res = 0;

            var command = connection.CreateCommand();
            command.CommandText = $"delete from {videoCollectionTable} where bvid='{bvid}'";
            try
            {
                res = command.ExecuteNonQuery();
            }
            catch (Exception)
            {
                Console.WriteLine("DeleteVideoCollection 数据库删除记录出错");

                if (command.Transaction != null)
                    command.Transaction.Rollback();

                //throw;
            }

            return res;
        }

        /// <summary>
        /// 根据视频合集bvid删除视频信息记录
        /// </summary>
        /// <param name="bvid"></param>
        /// <returns></returns>
        public int DeleteVideoInfoByBvid(string bvid)
        {
            int res = 0;

            var command = connection.CreateCommand();
            command.CommandText = $"delete from {videoInfoTable} wherer bvid='{bvid}'";
            try
            {
                res = command.ExecuteNonQuery();
            }
            catch (Exception)
            {
                Console.WriteLine("DeleteVideoInfoByBvid 数据库删除记录出错");

                if (command.Transaction != null)
                    command.Transaction.Rollback();

                //throw;
            }

            //视频/音频信息表里也对应删除
            DeleteAudioVedioByBvid(bvid);

            return res;
        }

        /// <summary>
        /// 删除视频信息
        /// </summary>
        /// <param name="cid"></param>
        /// <returns></returns>
        public int DeleteVideoInfoByCid(string cid)
        {
            return DeleteRecord(videoInfoTable, $"cid='{cid}'");
        }

        /// <summary>
        /// 删除视频画质/音频表记录
        /// </summary>
        /// <param name="bvid"></param>
        public void DeleteAudioVedioByBvid(string bvid)
        {
            DeleteRecord(audioQnTable, $"bvid='{bvid}'");
            DeleteRecord(videoQnTable, $"bvid='{bvid}'");
        }
        public void DeleteAudioVedioByCid(string cid)
        {
            DeleteRecord(audioQnTable, $"cid='{cid}'");
            DeleteRecord(videoQnTable, $"cid='{cid}'");
        }


        /// <summary>
        /// 获取待下载视频列表
        /// </summary>
        /// <returns></returns>
        public List<VideoInfoModel> GetDowningVideoList()
        {
            List<VideoInfoModel> list = GetVideoListByStatus(VideoStatus.DownWait);
            return list;
        }

        /// <summary>
        /// 获取已下载视频列表
        /// </summary>
        /// <returns></returns>
        public List<VideoInfoModel> GetDownedVideoList()
        {
            List<VideoInfoModel> list = GetVideoListByStatus(VideoStatus.Downed);
            return list;
        }

        /// <summary>
        /// 获取对应状态的视频信息列表
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public List<VideoInfoModel> GetVideoListByStatus(VideoStatus status)
        {
            List<VideoInfoModel> videoInfos = new List<VideoInfoModel>();

            //数据库中读取视频信息列表
            var command = connection.CreateCommand();
            command.CommandText = $"select * from {videoInfoTable} where status='{(int)status}'";
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var videoInfo = new VideoInfoModel();
                    //videoInfo.Bvid = reader.GetString(1);
                    //videoInfo.Bvid = reader["bvid"].ToString()!;
                    //videoInfo.Bvid = reader.GetFieldValue<string>("bvid");
                    //videoInfo.Cid = reader.GetFieldValue<string>("Cid");
                    //videoInfo.Title = reader.GetFieldValue<string>("title");
                    //videoInfo.Page = reader.GetFieldValue<string>("page");
                    //videoInfo.Duration = reader.GetFieldValue<int>("duration");
                    SetModelValue(videoInfo, reader);
                    videoInfos.Add(videoInfo);
                }
            }

            return videoInfos;
        }

        /// <summary>
        /// 更新视频状态
        /// </summary>
        /// <param name="cid"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public int UpdateVideoStatus(string cid, VideoStatus status)
        {
            int res = 0;

            var command = connection.CreateCommand();
            command.CommandText = $"update {videoInfoTable} set status={(int)status} where cid='{cid}'";
            try
            {
                res = command.ExecuteNonQuery();
            }
            catch (Exception)
            {
                Console.WriteLine("UpdateVideoStatus 数据库删除记录出错");
                return res;
                //throw;
            }
            return res;
        }
        /// <summary>
        /// 更新视频状态一次性插入
        /// </summary>
        /// <param name="cid"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public int UpdateVideoStatusAll(string updateSql)
        {
            int res = 0;

            var command = connection.CreateCommand();
            command.CommandText = $"update {videoInfoTable} {updateSql}";
            try
            {
                res = command.ExecuteNonQuery();
            }
            catch (Exception)
            {
                Console.WriteLine("UpdateVideoStatus 数据库删除记录出错");
                return res;
                //throw;
            }
            return res;
        }
        /// <summary>
        /// 更新设置视频状态和保存路径
        /// </summary>
        /// <param name="cid"></param>
        /// <param name="status"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public int UpdateVideoStatusAndPath(string cid, long size,  VideoStatus status, string filePath)
        {
            int res = 0;
            string cmdText = string.Empty;
            if(size >= 0)
            {
                cmdText += $"size={size},";
            }
            cmdText += $"status={(int)status},filepath='{filePath}'";

            var command = connection.CreateCommand();
            command.CommandText = $"update {videoInfoTable} set {cmdText} where cid='{cid}'";
            try
            {
                res = command.ExecuteNonQuery();
            }
            catch (Exception)
            {
                Console.WriteLine("UpdateVideoStatus 数据库删除记录出错");
                return res;
                //throw;
            }
            return res;
        }

        /// <summary>
        /// 根据状态获取视频信息列表【旧版】
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        [Obsolete]
        public List<VideoInfoModel> GetVideoListByStatusBack(VideoStatus status)
        {
            List<VideoInfoModel> videoInfos = new List<VideoInfoModel>();

            //数据库中读取视频信息列表
            using (var connection = new SqliteConnection(dataSource))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = $"select * from {videoInfoTable} where status='{(int)status}'";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var videoInfo = new VideoInfoModel();
                        //videoInfo.Bvid = reader.GetString(1);
                        //videoInfo.Bvid = reader["bvid"].ToString()!;
                        //videoInfo.Bvid = reader.GetFieldValue<string>("bvid");
                        //videoInfo.Cid = reader.GetFieldValue<string>("Cid");
                        //videoInfo.Title = reader.GetFieldValue<string>("title");
                        //videoInfo.Page = reader.GetFieldValue<string>("page");
                        //videoInfo.Duration = reader.GetFieldValue<int>("duration");

                        SetModelValue(videoInfo, reader);

                        videoInfos.Add(videoInfo);
                    }
                }

            }

            return videoInfos;
        }

        /// <summary>
        /// 获取视频画质信息
        /// </summary>
        /// <param name="bvid"></param>
        /// <param name="cid"></param>
        /// <param name="qn"></param>
        /// <returns></returns>
        public VideoQnModel GetVideoModel(string bvid, string cid, string qn)
        {
            VideoQnModel videoModel = new VideoQnModel();

            var command = connection.CreateCommand();
            command.CommandText = $"select * from {videoQnTable} where bvid='{bvid} and cid='{cid}' and qn='{qn}";
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    var videoInfo = new VideoInfoModel();
                    //videoInfo.Bvid = reader.GetString(1);
                    //videoInfo.Bvid = reader["bvid"].ToString()!;
                    //videoModel.Bvid = bvid;
                    //videoModel.Cid = cid;
                    //videoModel.Qn = qn;
                    //videoModel.CodeCs = reader.GetFieldValue<string>("codecs");
                    //videoModel.Size = reader.GetFieldValue<int>("size");

                    SetModelValue(videoModel, reader);
                }
            }

            return videoModel;
        }

        #endregion


        #region 用户操作区块

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public UserModel GetUser(string userName = "")
        {
            UserModel userModel = new UserModel();

            var command = connection.CreateCommand();
            if (!string.IsNullOrEmpty(userName))
            {
                command.CommandText = $"select * from {userTable} where username='{userName}' limit 1";
            }
            else
            {
                command.CommandText = $"select * from {userTable} limit 1";
            }
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    userModel = new UserModel();
                    //userModel.UserName = reader.GetFieldValue<string>("username");
                    //userModel.VipType = reader.GetFieldValue<int>("viptype");
                    //userModel.Cookie = reader.GetFieldValue<string>("cookie");

                    SetModelValue(userModel, reader);
                }
            }

            return userModel;
        }

        /// <summary>
        /// 保存用户信息
        /// </summary>
        /// <param name="userModel"></param>
        /// <returns></returns>
        public int SaveUser(UserModel userModel)
        {
            if (GetUser(userModel.UserName) != null)
            {
                DeleteUser(userModel.UserName);
            }
            long ctime = 0;
            var command = connection.CreateCommand();
            command.CommandText = $"insert into {userTable} (username,rank,viptype,cookie,logintime) " +
                $"values('{userModel.UserName}', '{userModel.Rank}','{userModel.VipType}', '{userModel.Cookie}', '{ctime}')";

            int res = command.ExecuteNonQuery();

            return res;
        }
        private bool IsUserNeedUpdate(UserModel userModel)
        {
            UserModel? user = GetUser(userModel.UserName);

            if (user == null || (user != null && user.VipType != userModel.VipType))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 删除用户信息
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public  int DeleteUser(string userName)
        {
            var command = connection.CreateCommand();
            command.CommandText = $"delete from {userTable} where username='{userName}'";
            var res = command.ExecuteNonQuery();

            return res;
        }

        #endregion

        
        #region 配置信息Config区块

        
        

        #endregion

        

        /// <summary>
        /// 读取数据库记录到类对象属性
        /// 注意：该方法需要[属性名.小写] = [数据库字段名]，且类属性和数据库字段要对应
        /// 在更改数据库和Model类后，在读取记录时就可以自动匹配，避免修改大量读取记录中的代码
        /// 这里算是简单版的，sqlite也可以根据 select name,type from pragma_table_info('table_name_xxx') 获取对应表的字段，然后根据属性和字段名进行意义匹配
        /// </summary>
        /// <param name="obj">类对象</param>
        /// <param name="reader">数据库读取记录</param>
        /// <returns></returns>
        private static void SetModelValue(object obj, SqliteDataReader reader, string tableName = "")
        {
            if (obj != null)
            {
                //遍历类对象属性，然后和读取的数据记录对应设置属性值
                foreach (var item in obj.GetType().GetProperties())
                {
                    if (reader.IsDBNull(item.Name.ToLower()))
                        continue;

                    if (item.PropertyType.Name == "String")
                    {
                        var value = reader.GetFieldValue<string>(item.Name.ToLower());
                        item.SetValue(obj, value);
                    }
                    else if (item.PropertyType.Name == "Int32" || item.PropertyType.Name.Contains("Int"))
                    {

                        var value = reader.GetFieldValue<int>(item.Name.ToLower());
                        item.SetValue(obj, value);
                    }
                }
            }
        }

        /// <summary>
        /// sqlite获取表字段
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private List<string> GetFieldNameList(string tableName)
        {
            var list = new List<string>();

            var command = connection.CreateCommand();
            //这里只获取了name,也可以获取type等，看自己需求
            command.CommandText = $"select name from pragma_table_info('{tableName}')";
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(reader.GetString(0));
                }
            }

            return list;
        }

        /// <summary>
        /// sqlite删除记录(通用)
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="condition">删除条件</param>
        /// <returns></returns>
        public int DeleteRecord(string tableName, string condition)
        {
            int res = 0;

            var command = connection.CreateCommand();
            command.CommandText = $"delete from {tableName} where {condition}";
            try
            {
                res = command.ExecuteNonQuery();
            }
            catch (Exception)
            {
                Console.WriteLine("数据库删除记录出错");

                if (command.Transaction != null)
                    command.Transaction.Rollback();
            }

            return res;
        }

        public void Dispose()
        {
            if (connection != null)
            {
                connection.Close();
                connection.Dispose();
            }
        }
    }
}
