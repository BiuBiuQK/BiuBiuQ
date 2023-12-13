using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiuBiuQ.Model.User
{
    public class UserModel
    {
        public string UserName { get; set; }
        public string Rank { get; set; }
        public int VipType { get; set; }
        public string Cookie {  get; set; }

        public long LoginTime { get; set; }

    }
}
