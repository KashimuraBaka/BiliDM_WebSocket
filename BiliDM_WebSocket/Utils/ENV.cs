using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace BiliDM_WebSocket.Utils
{
    public static class ENV
    {
        public static Main Plugin { get; set; }
        public static Window AppWindow => Application.Current?.MainWindow;
        public static Assembly AppAssembly { get; } = Assembly.GetExecutingAssembly();
        public static string AppVersion => AppAssembly.GetName().Version.ToString();
        public static string AppDllFileName => AppAssembly.Location;
        public static string AppDllFilePath { get; } = new FileInfo(AppDllFileName).DirectoryName;
        public static string ConfigDirectory { get; } = Path.Combine(AppDllFilePath, "WebSocket");
        public static string ConfigFileName { get; } = Path.Combine(ConfigDirectory, "config.json");
        public static Config Config { get; } = new Config();
        public static WebSokcetServer WebSokcetServer { get; } = new WebSokcetServer();

        public static async Task InitServices()
        {
            await Config.LoadAsync();
            WebSokcetServer.Start(Config.WebSocketUrl, Config.WebSocketPort);
        }

        public static void Log(string message) => Plugin.Log(message);
    }
}
