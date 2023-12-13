using BiuBiuQ.Model.User;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BiuBiuQ.ViewModel.User
{
    public class UserDataViewModel:NotificationObject
    {
        private UserDataViewModel() {
            if(user == null)
            {
                user = new UserModel();
            }
        }
        public static UserDataViewModel Instance { get; private set; } = new UserDataViewModel();

        private UserModel user;

        public string UserName
        {
            get { return user.UserName; }
            set { 
                user.UserName = value; 
                this.RaisPropertyChanged(nameof(UserName));
            }
        }
        public string Rank
        {
            get { return user.Rank; }
            set
            {
                user.Rank = value;
                this.RaisPropertyChanged(nameof(Rank));
            }
        }
        public int VipType
        {
            get { return user.VipType; }
            set
            {
                user.VipType = value;
                this.RaisPropertyChanged(nameof(VipType));
            }
        }

        public string Cookie {
            get { return user.Cookie; }
            set
            {
                user.Cookie = value;
                this.RaisPropertyChanged(nameof(Cookie));
            }
        }

        public long LoginTime
        {
            get { return user.LoginTime; }
            set
            {
                user.LoginTime = value;
                this.RaisPropertyChanged(nameof(LoginTime));
            }
        }


        private BitmapImage qrCodeImage;
        /// <summary>
        /// 登录二维码
        /// </summary>
        public BitmapImage QrCodeImage 
        { 
            get { return qrCodeImage; } 
            set
            {
                qrCodeImage = value;
                this.RaisPropertyChanged(nameof(QrCodeImage));
            } 
        }

        private string loginBtnContent;
        public string LoginBtnContent
        {
            get => loginBtnContent;
            set
            {
                loginBtnContent = value;
                this.RaisPropertyChanged(nameof(LoginBtnContent));
            }
        }

        private string loginTip;
        public string LoginTip
        {
            get { return loginTip; }
            set
            {
                loginTip = value;
                this.RaisPropertyChanged(nameof(LoginTip));
            }
        }
    }
}
