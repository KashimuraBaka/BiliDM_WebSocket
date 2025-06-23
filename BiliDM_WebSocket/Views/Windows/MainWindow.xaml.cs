using BiliDM_WebSocket.Utils;
using BiliDM_WebSocket.Utils.Structs;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace BiliDM_WebSocket.Views.Windows
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string serverUrl;
        public string ServerUrl
        {
            get => serverUrl;
            set => SetProperty(ref serverUrl, value);
        }
        private string serverPort;
        public string ServerPort
        {
            get => serverPort;
            set => SetProperty(ref serverPort, value);
        }

        public ICommand SaveCommand => new RelayCommand(Save);


        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ServerUrl = ENV.Config.WebSocketUrl;
            ServerPort = ENV.Config.WebSocketPort;
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true; // 阻止窗口关闭
            Hide(); // 隐藏窗口而不是关闭
        }

        private void Save()
        {
            if (string.IsNullOrEmpty(ServerUrl) || string.IsNullOrEmpty(ServerPort))
            {
                MessageBox.Show("服务器地址/端口 不能为空!");
                return;
            }
            ENV.Config.WebSocketUrl = ServerUrl;
            ENV.Config.WebSocketPort = ServerPort;
            ENV.WebSokcetServer.Close();
            ENV.WebSokcetServer.Start(ServerUrl, ServerPort);
            MessageBox.Show("保存完毕");
        }

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            NotifyPropertyChanged(propertyName);
            return true;
        }
    }
}
