using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace BiuBiuQ.ViewModel.Common
{
    public class TimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return DependencyProperty.UnsetValue;

            long timeStamp = 0;
            if (value is long)
            {
                timeStamp = (long)value;
            }
            else if (value is int)
            {
                timeStamp = (int)value;
            }
            
            if (timeStamp < 1000000)
            {
                //这个给播放时长用
                return GetTime((int)timeStamp);
            }
            else
            {
                DateTimeOffset time = DateTimeOffset.FromUnixTimeSeconds(timeStamp);
                return time.ToString("yyyy-MM-dd hh:mm:ss");
            }

            
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //throw new NotImplementedException();
            string timeString = (string)value;
            long timeStamp = 0;
            if(DateTimeOffset.TryParse(timeString, out DateTimeOffset dateTimeOffset))
            {
                timeStamp = dateTimeOffset.ToUnixTimeSeconds();
                
            }

            return timeStamp.ToString();
        }

        string GetTime(int time)
        {
            int hours = time / 3600;
            int minutes = (time - hours * 3600) / 60;
            int seconts = time - hours * 3600 - minutes * 60;

            return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconts);
        }
    }
}
