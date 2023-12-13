using BiuBiuQ.Model.Video;
using BiuBiuQ.ViewModel.VideoDown;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BiuBiuQ.Service.Common
{
    public class HttpService
    {

        /// <summary>
        /// 网页访问GET
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public static string HttpGetContent(string url, Dictionary<string, string> headers)
        {
            string result=string.Empty;

            RestClient client = new RestClient(url);

            //如果未设置User-Agent，设置一个默认的
            if(headers.ContainsKey("User-Agetn"))
                headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36");
            
            client.AddDefaultHeaders(headers);

            RestRequest request = new RestRequest();
            request.Method = Method.Get;

            var response =  client.ExecuteAsync(request);

            if(response.Result.Content != null)
            {
                result = response.Result.Content;
            }

            return result;
        }

        /// <summary>
        /// 获取http网络文件链接的文件扩展名
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetUrlFileExtension(string url)
        {
            string name = url.Substring(url.LastIndexOf('.'));
            name = name.Substring(0, name.IndexOf('?'));

            return name;
        }

        /// <summary>
        /// 获取http网络文件大小
        /// </summary>
        /// <param name="fileUrl"></param>
        /// <returns></returns>
        public static long GetHttpFileSize(string fileUrl, Dictionary<string, string> headers)
        {
            long size = 0;

            using (var httpClient = new HttpClient())
            {
                //如果未设置User-Agent，设置一个默认的
                if (!headers.ContainsKey("User-Agetn"))
                    headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36");

                foreach (var header in headers) {
                    httpClient.DefaultRequestHeaders.Add(header.Key, header.Value   );
                }

                try
                {
                    // 发送GET请求，并等待响应
                    var response = httpClient.GetAsync(new Uri(fileUrl), HttpCompletionOption.ResponseHeadersRead).Result;
                    // 判断请求是否成功,如果失败则返回 false
                    if (response.IsSuccessStatusCode)
                    {
                        if (response.Content.Headers.ContentLength != null)
                        {
                            Console.WriteLine("ContentLength:" + response.Content.Headers.ContentLength);
                            size = response.Content.Headers.ContentLength.Value;
                        }
                    }
                }
                catch (Exception)
                {

                    //throw;
                }

            }

            return size;
        }

    }
}
