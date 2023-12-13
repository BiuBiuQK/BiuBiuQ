using BiuBiuQ.ViewModel.Config;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace BiuBiuQ.ViewModel.Common
{
    public class VideoQnConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null)
                return DependencyProperty.UnsetValue;

            foreach (object value in values)
            {
                if (value == null || value == DependencyProperty.UnsetValue) return DependencyProperty.UnsetValue;
            }

            string qn = (string)values[0];
            string codeCs = (string)values[1];

            return ConfigStaticViewModel.Instance.VideoQnName[qn] +"/"+ codeCs;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            string qnCodeCs = (string)value;
            string[] arr = qnCodeCs.Split('/', StringSplitOptions.RemoveEmptyEntries);
            return arr;
        }
    }
}
