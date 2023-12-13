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
    public class FileSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return DependencyProperty.UnsetValue;

            long size = (long)value;
            return  GetFormattedFileSize(size);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private string GetFormattedFileSize(long fileSizeInBytes)
        {
            if (fileSizeInBytes <= 0)
                return "0 Bytes";

            const int GB = 1024 * 1024 * 1024;
            const int MB = 1024 * 1024;
            const int KB = 1024;

            string size = "";

            if (fileSizeInBytes >= GB)
            {
                size = ((double)fileSizeInBytes / GB).ToString("F2") + " GB";
            }
            else if (fileSizeInBytes >= MB)
            {
                size = ((double)fileSizeInBytes / MB).ToString("F2") + " MB";
            }
            else if (fileSizeInBytes >= KB)
            {
                size = ((double)fileSizeInBytes / KB).ToString("F2") + " KB";
            }
            else
            {
                size = fileSizeInBytes.ToString("F2") + " Bytes";
            }

            return size;
        }
    }
}
