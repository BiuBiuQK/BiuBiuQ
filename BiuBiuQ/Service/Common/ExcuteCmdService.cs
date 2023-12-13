using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiuBiuQ.Service.Common
{
    public class ExcuteCmdService
    {
        private static bool result = true;
        public static bool Excute(string exe, string cmd)
        {
            result = true;//自定义的，根据外部程序执行输出里的结果是否有错误

            Process p = new Process();
            p.StartInfo.FileName = exe;
            p.StartInfo.Arguments = cmd;
            p.StartInfo.UseShellExecute = false;//不使用操作系统外壳程序启动线程(一定为FALSE,详细的请看MSDN)
            p.StartInfo.RedirectStandardError = true;//把外部程序错误输出写到StandardError流中(这个一定要注意,FFMPEG的所有输出信息,都为错误输出流,用StandardOutput是捕获不到任何消息的
            //p.StartInfo.RedirectStandardInput = true;//把外部程序输入重定向到StandardInput流中
            p.StartInfo.CreateNoWindow = true;//不要创建窗口
            p.ErrorDataReceived += new DataReceivedEventHandler(Output);//输出流产生的事件，调用自定义的Output委托处理
            p.Start();
            p.BeginErrorReadLine();//开始异步读取
            p.WaitForExit();//阻塞等待进程结束
            p.Close();
            p.Dispose();

            return result;
        }
        private static void Output(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Console.WriteLine(e.Data);

                //自定义错误判断字符串
                if ((e.Data.Contains("out#") || e.Data.Contains("in#0")) && e.Data.Contains("Error"))
                {
                    result = false;
                }
            }
        }
    }
}
