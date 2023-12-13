using BiuBiuQ.Model.User;
using BiuBiuQ.Service.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace BiuBiuQ.Service.User
{
    /// <summary>
    /// Api接口获取用户信息类
    /// 数据库操作用户信息在Common\DBService里
    /// </summary>
    public class UserService
    {


        //获取登录接口的Api，可以获取qrcode_key和带该参数创建二维码的Api
        private static string loginApiUrl = "https://passport.bilibili.com/x/passport-login/web/qrcode/generate?source=main-fe-header";

        public static string qrcodeKey = string.Empty;

        //二维码登录状态查询接口，示例：$"https://passport.bilibili.com/x/passport-login/web/qrcode/poll?qrcode_key={qrcodeKey}&source=main-fe-header"
        public static string loginQrCodeApiUrl = string.Empty;

        //账号信息接口
        private static string accountApiUrl = "https://api.bilibili.com/x/member/web/account";

        //用户信息接口
        private static string userInfoApiUrl = "https://api.bilibili.com/x/vip/web/user/info";

        /// <summary>
        /// 在登录后带Cookie访问接口 获取用户信息，主要获取用户名和会员等级，
        /// </summary>
        /// <param name="cookie"></param>
        /// <returns></returns>
        public static UserModel GetUserInfoFromApi(string cookie) 
        {
            //https://api.bilibili.com/x/member/web/account
            //{"code":0,"message":"0","ttl":1,"data":{"mid":49220xxxx,"uname":"xxxxxx","userid":"bili_9264059xxxx","sign":"","birthday":"2000-01-01","sex":"保密","nick_free":false,"rank":"注册会员"}}

            //https://api.bilibili.com/x/vip/web/user/info
            //{"code":0,"message":"0","ttl":1,"data":{"mid":49220xxxx,"vip_type":0,"vip_status":0,"vip_due_date":0,"vip_pay_type":0,"theme_type":0,"label":{"text_color":"","bg_style":1,"bg_color":"","border_color":"","use_img_label":true,"img_label_uri_hans":"","img_label_uri_hant":"","img_label_uri_hans_static":"https://i0.hdslb.com/bfs/vip/d7b702ef65a976b20ed854cbd04cb9e27341bb79.png","img_label_uri_hant_static":"https://i0.hdslb.com/bfs/activity-plat/static/20220614/e369244d0b14644f5e1a06431e22a4d5/KJunwh19T5.png"},"avatar_subscript":0,"avatar_subscript_url":"","nickname_color":"","is_new_user":true,"tip_material":null}}

            //{"code": -101, "message": "账号未登录", "ttl": 1}

            var user = new UserModel();
            user.Rank = string.Empty;
            user.UserName = string.Empty;
            user.Cookie = string.Empty;
            user.VipType = 0;

            if(!string.IsNullOrEmpty(cookie) )
            {
                string accoutnInfo = GetUserInfoByApiUrl(accountApiUrl, cookie);
                string userInfo = GetUserInfoByApiUrl(userInfoApiUrl, cookie);

                try
                {
                    user.UserName = JsonDocument.Parse(accoutnInfo).RootElement.GetProperty("data").GetProperty("uname").ToString();
                    user.Rank = JsonDocument.Parse(accoutnInfo).RootElement.GetProperty("data").GetProperty("rank").ToString();
                    user.VipType = JsonDocument.Parse(userInfo).RootElement.GetProperty("data").GetProperty("vip_type").GetInt32();
                    user.Cookie = cookie;
                }
                catch (Exception)
                {
                    
                }
                

            }

            return user;
        }

        private static string GetUserInfoByApiUrl(string apiUrl, string cookie)
        {
            Dictionary<string, string> headersDict = new Dictionary<string, string>();

            headersDict.Add("Cookie", cookie);

            string jsonString = HttpService.HttpGetContent(apiUrl, headersDict);

            return jsonString;
        }

        private static void SetLoginQrCodeApiUrl(string qrcodeKey)
        {
            loginQrCodeApiUrl = $"https://passport.bilibili.com/x/passport-login/web/qrcode/poll?qrcode_key={qrcodeKey}&source=main-fe-header";
        }
        public static string GetLoginStatusAsync(string qrcodeKey)
        {
            string queryUrl = $"https://passport.bilibili.com/x/passport-login/web/qrcode/poll?qrcode_key={qrcodeKey}&source=main-fe-header";
            return HttpService.HttpGetContent(queryUrl, new Dictionary<string, string>());
        }

        /// <summary>
        /// 生成登录二维码
        /// </summary>
        /// <returns></returns>
        public static BitmapImage CreateLoginQrCode()
        {
            string loginUrl = loginApiUrl;
            string jsonString = HttpService.HttpGetContent(loginUrl, new Dictionary<string, string>());
            string url = GetQrcodeData(jsonString, "url");
            Console.WriteLine(url);
            qrcodeKey = GetQrcodeData(jsonString, "qrcode_key");
            
            BitmapImage image = new BitmapImage();
            if (!string.IsNullOrEmpty(url) )
            {
                image = QrCodeService.QrCodeCreate(url);//生成二维码图片
            }

            return image;
        }

        /// <summary>
        /// 获取二维码登录所需的url或者qrcode_key
        /// 访问登录接口获取返回数据示例：
        /// {"code": 0,"message": "0","ttl": 1,"data": {"url": "https://passport.bilibili.com/h5-app/passport/login/scan?navhide=1&qrcode_key=68a8af6a47cd56d16670432fc35602c5&from=main-fe-header","qrcode_key": "68a8af6a47cd56d16670432fc35602c5"}}
        /// </summary>
        /// <param name="jsonString"></param>
        /// <param name="key">url或者qrcode_key</param>
        /// <returns>根据参数key返回对应文本字符串</returns>
        public static string GetQrcodeData(string jsonString, string key)
        {
            string valueStr = string.Empty;
            try
            {
                JsonNode json = JsonNode.Parse(jsonString);
                var value = json["data"][key];
                valueStr = value.ToString();
            }
            catch (Exception)
            {
                return valueStr;
            }

            return valueStr;
        }

        /// <summary>
        /// 退出登录
        /// </summary>
        /// <param name="user"></param>
        public static void Logout(UserModel user)
        {
            //数据库删除记录
            DBService.Instance.DeleteUser(user.UserName);

            //用户属性置空
            foreach (var item in user.GetType().GetProperties())
            {
                if (item.PropertyType.Name == "String")
                {
                    item.SetValue(user, string.Empty);
                }
            }
        }

    }
}
