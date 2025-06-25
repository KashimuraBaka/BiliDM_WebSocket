using BilibiliDM_PluginFramework;
using BiliDM_WebSocket.Utils;
using BiliDM_WebSocket.Views.Windows;
using Newtonsoft.Json;
using System;
using System.IO;

namespace BiliDM_WebSocket
{
    public class Main : DMPlugin
    {
        public MainWindow AdminWindow { get; } = new MainWindow();
        public Request Request { get; } = new Request();

        public Main()
        {
            ENV.Plugin = this;

            PluginAuth = "Kashimura";
            PluginName = "WebSocket服务";
            PluginCont = "kashimura@qq.com";
            PluginVer = ENV.AppVersion;
            PluginDesc = "弹幕姬WebSocket服务端";

            ReceivedDanmaku += OnReceivedDanmaku;

            // 必须要启用插件, 不然无法接受到弹幕
            base.Start();
        }

        public override async void Inited()
        {
            Log("加载配置中...");
            if (!Directory.Exists(ENV.ConfigDirectory))
            {
                Log("未发现配置文件夹，尝试创建中");
                Directory.CreateDirectory(ENV.ConfigDirectory);
            }
            // 初始化服务
            await ENV.InitServices();
            Log("配置加载完毕!");
        }

        public override void Stop()
        {
            // 不做任何处理, 没必要关闭该插件. 直接卸载即可
        }

        public override void Admin()
        {
            AdminWindow.Show();
        }

        private async void OnReceivedDanmaku(object sender, ReceivedDanmakuArgs e)
        {
            var danmu = e.Danmaku.RawDataJToken.ToObject<DanmakuRawData>();
            if (!string.IsNullOrEmpty(danmu.Data.EmojiImageUrl))
            {
                var extension = Path.GetExtension(danmu.Data.EmojiImageUrl);
                var data = await Request.GetData(danmu.Data.EmojiImageUrl);
                danmu.Data.EmojiImageData = $"data:image/{extension.Substring(1)};base64,{Convert.ToBase64String(data)}";
            }
            if (!string.IsNullOrEmpty(danmu.Data.UserFace))
            {
                var extension = Path.GetExtension(danmu.Data.UserFace);
                var data = await Request.GetData(danmu.Data.UserFace);
                danmu.Data.UserFaceData = $"data:image/{extension.Substring(1)};base64,{Convert.ToBase64String(data)}";
            }
            ENV.WebSokcetServer?.SendAllMessage(JsonConvert.SerializeObject(danmu));
        }
    }
}
