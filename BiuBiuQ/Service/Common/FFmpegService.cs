using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiuBiuQ.Service.Common
{
    /// <summary>
    /// 用ffmpeg将音视频合并成视频文件
    /// 暂时调用外部exe方式，后面可考虑采用FFmpeg.AutoGen
    /// 不过这两者需要引入的文件都比较大上百M，可根据原版ffmpeg自行裁剪所需功能
    /// </summary>
    public class FFmpegService
    {
        private static string exeDefaultPath = "Tools";
        public static int MegerVideoAudio(string videoFile, string audioFile, string outputFile)
        {
            if (File.Exists(outputFile))
            {
                Console.WriteLine($"文件已存在");
                return 0;
            }

            //ffmpeg.exe -i video.m4s -i audio.m4s -codec copy output.mp4
            string exeName = "ffmpeg.exe";
            string exePath = Path.Combine(Directory.GetCurrentDirectory(), exeDefaultPath);
            string exe = Path.Combine(exePath, exeName);
            //这里特别注意文件路径要加双引号，如果文件路径有空格就会出现找不到文件的情况
            string cmd = $" -i \"{videoFile}\"  -i  \"{audioFile}\"  -codec copy  \"{outputFile}\" ";
            bool res = ExcuteCmdService.Excute(exe, cmd);
            if (res)
            {
                return 1;
            }
            return -1;
        }
    }
}
