using BiuBiuQ.Service.Common;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BiuBiuQ.Service.Config
{
    public class ConfigService
    {

        private static readonly string configTable = "config";

        private static readonly string savePathName = "savepath";
        private static readonly string downTaskCountName = "downtaskcount";


        /// <summary>
        /// 保存配置信息
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int SaveConfig(string name, string value)
        {
            int res = 0;
            var command = DBService.Instance.connection.CreateCommand();
            command.CommandText = $"select * from {configTable} where name='{name}'";
            using (var reader = command.ExecuteReader())
            {
                var updateCommand = DBService.Instance.connection.CreateCommand();
                if (reader.Read())
                {
                    updateCommand.CommandText = $"update {configTable} set value='{value}' where name='{name}'";
                }
                else
                {
                    updateCommand.CommandText = $"insert into {configTable} (`name`,`value`) values('{name}', '{value}')";
                }

                res = updateCommand.ExecuteNonQuery();
            }

            return res;
        }


        /// <summary>
        /// 读取配置值，全部以字符串形式
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string ReadConfig(string name)
        {
            string value = string.Empty;
            var command = DBService.Instance.connection.CreateCommand();
            command.CommandText = $"select * from {configTable} where name='{name}'";
            try
            {
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        value = reader.GetFieldValue<string>("value");
                    }
                }
            }
            catch (Exception)
            {

                return value;
            }


            return value;
        }

        /// <summary>
        /// 读取配置值
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T? ReadConfig<T>(string name)
        {
            T? value = default(T);
            var command = DBService.Instance.connection.CreateCommand();
            command.CommandText = $"select * from {configTable} where name='{name}'";
            try
            {
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        value = reader.GetFieldValue<T>("value");
                    }
                }
            }
            catch (Exception)
            {

                return value;
            }


            return value;
        }

        /// <summary>
        /// 获取全局保存路径
        /// </summary>
        /// <returns></returns>
        public static string GetSavePath()
        {
            return ReadConfig(savePathName);
        }
        public static int SetSavePath(string value)
        {
            return SaveConfig(savePathName, value);
        }

        /// <summary>
        /// 获取下载线程并发数
        /// </summary>
        /// <returns></returns>
        public static int GetDownTaskCount()
        {
            string downTaskCountStr = ReadConfig(downTaskCountName);
            int downTaskCount;
            if (int.TryParse(downTaskCountStr, out downTaskCount))
            {
                if (downTaskCount <= 0)
                    downTaskCount = 1;
            }
            else
            {
                downTaskCount = 1;
            }


            return downTaskCount;
        }

        public static int SetDownTaskCount(string value)
        {
            return SaveConfig(downTaskCountName, value);
        }
    }
}
