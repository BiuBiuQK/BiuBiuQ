using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiuBiuQ.Service.Common
{
    /// <summary>
    /// 已弃用，对文件名识别问题很大，有的文件名无法识别直接导致合并失败
    /// 用mp4box将音视频合并成视频文件
    /// mp4box文件较小只有6M，便于打包复制下载
    /// </summary>
    [Obsolete]
    public class Mp4boxService
    {
        private static string exeDefaultPath = "Tools";
        public static bool MegerVideoAudio(string videoFile, string audioFile, string outputFile)
        {
            if(File.Exists(outputFile))
            {
                Console.WriteLine($"文件已存在");
                return false;
            }

            //mp4box.exe -add video1.m4s -add audio.m4s output.mp4
            string exeName = "mp4box.exe";
            string exePath = Path.Combine(Directory.GetCurrentDirectory(), exeDefaultPath);
            string exe = Path.Combine(exePath, exeName);
            string cmd = $" -add {videoFile} -add {audioFile} {outputFile}";
            return ExcuteCmdService.Excute(exe, cmd);
        }
    }
}
