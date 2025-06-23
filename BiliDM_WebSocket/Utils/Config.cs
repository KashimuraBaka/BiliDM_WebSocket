using BiliDM_WebSocket.Utils.Structs;
using Newtonsoft.Json;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BiliDM_WebSocket.Utils
{
    public class Config : ObservableObject
    {
        [JsonIgnore]
        private bool Loaded { get; set; }

        private string webSocketUrl = "localhost";
        public string WebSocketUrl
        {
            get => webSocketUrl;
            set => SetProperty(ref webSocketUrl, value);
        }
        private string webSocketPort = "9600";
        public string WebSocketPort
        {
            get => webSocketPort;
            set => SetProperty(ref webSocketPort, value);
        }

        public Config()
        {
            // 如果参数属性发生变动
            PropertyChanged += OnPropertyChanged;
        }

        protected async void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Loaded && !string.IsNullOrEmpty(ENV.ConfigFileName))
            {
                await SaveAsync();
            }
        }

        public async Task LoadAsync()
        {
            if (File.Exists(ENV.ConfigFileName))
            {
                using (var fs = new FileStream(ENV.ConfigFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var reader = new StreamReader(fs, Encoding.UTF8))
                {
                    var configString = await reader.ReadToEndAsync();
                    var config = JsonConvert.DeserializeObject<Config>(configString);
                    if (config != null)
                    {
                        WebSocketUrl = config.WebSocketUrl;
                        WebSocketPort = config.WebSocketPort;
                    }
                }
            }
            Loaded = true;
        }

        public async Task SaveAsync()
        {
            using (var fs = new FileStream(ENV.ConfigFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            using (var sw = new StreamWriter(fs, Encoding.UTF8))
            {
                fs.SetLength(0);
                await sw.WriteAsync(JsonConvert.SerializeObject(this, Formatting.Indented));
            }
        }
    }
}
