using BiuBiuQ.Command;
using BiuBiuQ.Model.User;
using BiuBiuQ.Service.Common;
using BiuBiuQ.Service.User;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace BiuBiuQ.ViewModel.User
{
    public class UserViewModel
    {
        private string loginString = "点击登录";
        private string logoutString = "退出登录";
        private string logwaitString = "等待登录";
        private string refreshString = "刷新二维码";
        public DelegateCommand LoginCommand { get; set; }
        public UserViewModel() {

            LoginCommand = new DelegateCommand();
            LoginCommand.ExcuteAction = new Action<object>(LoginOption);

            SetUserProperty(DBService.Instance.GetUser());

            if (!string.IsNullOrEmpty(UserDataViewModel.Instance.Cookie))
                UserDataViewModel.Instance.LoginBtnContent = logoutString;
            else
                UserDataViewModel.Instance.LoginBtnContent = loginString;
        }

        private void SetUserProperty(UserModel userModel)
        {
            UserDataViewModel.Instance.UserName = userModel.UserName;
            UserDataViewModel.Instance.Rank = userModel.Rank;
            UserDataViewModel.Instance.VipType = userModel.VipType;
            UserDataViewModel.Instance.Cookie = userModel.Cookie;
            UserDataViewModel.Instance.LoginTime = userModel.LoginTime;
        }

        public void LoginOption(object parameter)
        {
            //点击登录或者刷新二维码
            if (UserDataViewModel.Instance.LoginBtnContent == loginString || 
                UserDataViewModel.Instance.LoginBtnContent == refreshString
                )
            {
                CodeLogin(parameter);
            }

            //退出登录
            if (UserDataViewModel.Instance.LoginBtnContent == logoutString)
            {
                Logout(parameter);
            }


        }
        public void CodeLogin(object parameter)
        {
            UserDataViewModel.Instance.QrCodeImage = UserService.CreateLoginQrCode();
            Task.Run(() =>
            {
                UserDataViewModel.Instance.LoginBtnContent = this.logwaitString;

                CheckLoginStatus();
            });
        }
        
        /// <summary>
        /// 登录状态轮询判断
        /// </summary>
        /// <returns></returns>
        public  bool CheckLoginStatus()
        {
            for(int i = 0;i<300;i++)
            {
                Thread.Sleep(1000);

                if (string.IsNullOrEmpty(UserService.qrcodeKey)) 
                    continue;

                string loginResult =UserService.GetLoginStatusAsync(UserService.qrcodeKey);
                Console.WriteLine("json结果：{0}", loginResult);
                try
                {
                    //二维码登录状态判断
                    int code = JsonDocument.Parse(loginResult).RootElement.GetProperty("code").GetInt32();
                    if(code == 0)
                    {
                        int dataCode = JsonDocument.Parse(loginResult).RootElement.GetProperty("data").GetProperty("code").GetInt32();

                        if (dataCode == 86101) //等待扫码
                        {
                            UserDataViewModel.Instance.LoginTip = "等待扫码";
                            continue;
                        }
                        else if (dataCode == 86038)
                        {
                            UserDataViewModel.Instance.LoginTip = "二维码已过期, 请重新执行登录指令.";
                            UserDataViewModel.Instance.LoginBtnContent = "刷新二维码";
                            break;
                        }
                        else if (dataCode == 86090) //等待确认
                        {
                            UserDataViewModel.Instance.LoginTip = "扫码成功, 等待确认登录...";
                        }
                        else if(dataCode == 0) //登录成功
                        {
                            UserDataViewModel.Instance.LoginTip = "登录成功";

                            //登录成功后进行的一些后续处理操作

                            string res = JsonDocument.Parse(loginResult).RootElement.GetProperty("data").GetProperty("url").ToString();

                            //导出cookie
                            string cookie = res[(res.IndexOf('?') + 1)..].Replace("&", ";");
                            Console.WriteLine(cookie);

                            UserDataViewModel.Instance.QrCodeImage = null;
                            UserDataViewModel.Instance.LoginBtnContent = logoutString;//按钮文本设置

                            //设置全局用户信息，并将信息保存到数据库
                            UserModel userModel = UserService.GetUserInfoFromApi(cookie);
                            SetUserProperty(userModel);
                            DBService.Instance.SaveUser(userModel);//用户信息保存到数据库

                            UserService.qrcodeKey = string.Empty;

                            return true;
                            //break;
                        }
                        else
                        {
                            UserDataViewModel.Instance.LoginTip = loginResult;
                        }
                    }
                    else
                    {
                        UserDataViewModel.Instance.LoginTip = loginResult;
                    }
                    
                }
                catch (Exception)
                {
                    UserDataViewModel.Instance.LoginTip = loginResult;
                    break;
                    //throw;
                }
                
            }

            UserDataViewModel.Instance.LoginBtnContent = "刷新二维码";
            return false;
        }
        
        public void Logout(object parameter)
        {
            //UserService.Logout(UserDataViewModel.Instance.User);

            //数据库删除记录
            DBService.Instance.DeleteUser(UserDataViewModel.Instance.UserName);

            //用户属性置空
            UserDataViewModel.Instance.UserName = "";
            UserDataViewModel.Instance.Rank = "";
            UserDataViewModel.Instance.Cookie = "";


            UserDataViewModel.Instance.LoginBtnContent = loginString;
        }
    }
}
