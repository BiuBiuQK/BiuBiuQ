# BiuBiuQ
B站bilibili视频下载工具
C#/.NET6/Wpf/MVVM
部分代码特意留了多种实现方法便于学习用，可能会有一些冗余
UI界面就一个基本的，MVVM方式实现所以要换UI还是比较方便的，无需改动后台代码，直接WPF前端设计好套用就可以了

B站视频下载说明

1.用户：先登录一下用户，这样就可以下载1080P的视频了，如果不登录只能下载480P的视频，登录需要手机端APP扫码登录

2.设置：设置保存目录和下载并发数

3.搜索：复制B站视频链接到搜索框，注意需要有Bvid的那种视频链接，下面示例链接的BV1iu411F7ix就是Bvid

示例：https://www.bilibili.com/video/BV1iu411F7ix?p=7&spm_id_from=pageDriver&vd_source=c8db1130312fa78834b75af4bd7f39d1

4.下载：获取视频列表，然后添加到下载，可以单个下载，也可以全部下载，下载完成后，可以打开对应目录


运行环境基于.NET6 需要安装.NET 6.0 Desktop Runtime

https://dotnet.microsoft.com/zh-cn/download/dotnet/thank-you/runtime-desktop-6.0.25-windows-x64-installer?cid=getdotnetcore

使用ffmpeg进行音视频合并，这里的ffmpeg用了一个旧版的文件小一点，功能够用，新版的文件比较大100多M了
