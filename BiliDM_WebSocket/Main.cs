using BilibiliDM_PluginFramework;
using BiliDM_WebSocket.Utils;
using BiliDM_WebSocket.Views.Windows;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

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
            var rawData = e.Danmaku.RawDataJToken;
            if (rawData.Value<string>("cmd") is "LIVE_OPEN_PLATFORM_DM")
            {
                var danmu = e.Danmaku.RawDataJToken.ToObject<DanmakuRawData>();
                // 获取表情 Base64 数据
                if (!string.IsNullOrEmpty(danmu.Data.EmojiImageUrl))
                {
                    danmu.Data.EmojiImageData = await GetImageBase64Data(danmu.Data.EmojiImageUrl);
                }
                // 获取头像 Base64 数据
                if (!string.IsNullOrEmpty(danmu.Data.UserFace))
                {
                    danmu.Data.UserFaceData = await GetImageBase64Data(danmu.Data.UserFace);
                }
                ENV.WebSokcetServer?.SendAllMessage(JsonConvert.SerializeObject(danmu));
            }
            else
            {
                ENV.WebSokcetServer?.SendAllMessage(rawData.ToString());
            }
        }

        private async Task<string> GetImageBase64Data(string url)
        {
            var extension = Path.GetExtension(url);
            var data = await Request.GetData(url);
            return $"data:image/{extension.Substring(1)};base64,{Convert.ToBase64String(data)}";
        }
    }
}
