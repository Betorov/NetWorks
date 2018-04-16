using HttpComReaderWPF.Command;
using HttpComReaderWPF.Model;
using HttpComReaderWPF.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HttpComReaderWPF.ViewModel
{
    class MainWindowView : BaseViewModel
    {
        private string htmlBody;
        public string HtmlBody { get => htmlBody; set { htmlBody = value; OnPropertyChanged("HtmlBody"); } }

        private BaseViewModel currentViewModel;
        internal BaseViewModel CurrentViewModel
        {
            get => currentViewModel;
            set { currentViewModel = value; OnPropertyChanged("CurrentViewModel"); }
        }

        private Uri remoteAddress;
        public Uri RemoteAddress { get => remoteAddress; set { remoteAddress = value; OnPropertyChanged("RemoteAddress"); } }

        private int port;
        public int Port { get => port; set { port = value; OnPropertyChanged("Port"); } }

        public bool isDownloading;
        public bool IsDownloading { get => isDownloading; set { isDownloading = value; OnPropertyChanged("IsDownloading"); } }

        public MainWindowView()
        {
            IsDownloading = false;
            HtmlBody = string.Empty;
            Port = 80;
            RemoteAddress = new Uri("http://google.ru");           
        }

        private RelayCommand _downloadCommand;
        public RelayCommand DownloadCommand
        {
            get
            {
                return _downloadCommand ?? (_downloadCommand = new RelayCommand(async obj =>
                {
                    HttpPage pageViewModel = new HttpPage();
                    CurrentViewModel = pageViewModel.httpContext;
                    Task downloadTask = new Task(new Action(async () => { DownloadPage(); }));
                    downloadTask.Start();
                }));
            }
        }

        private async Task DownloadPage()
        {
            IsDownloading = true;
            htmlBody = string.Empty;
            Socket pageSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            string resolvingString = string.Empty;
            if (RemoteAddress.OriginalString.Contains("http"))
            {
                resolvingString = RemoteAddress.Host;
            }
            else
            {
                resolvingString = RemoteAddress.OriginalString;
            }

            try
            {
                IPHostEntry hosts = Dns.GetHostEntry(resolvingString);
                pageSocket.Connect(hosts.AddressList, Port);

                using (var stream = new NetworkStream(pageSocket))
                {
                    StreamWriter streamWriter = new StreamWriter(stream);
                    StreamReader streamReader = new StreamReader(stream);

                    string request = string.Empty;
                    request += "GET / HTTP/1.0\r\n";
                    request += $"Host: {resolvingString}\r\n";
                    request += "\r\n";

                    await streamWriter.WriteAsync(request);
                    await streamWriter.FlushAsync();

                    int readLen = 0;
                    do
                    {
                        char[] buffer = new char[4096];
                        readLen = await streamReader.ReadAsync(buffer, 0, buffer.Length);
                        htmlBody += string.Concat(buffer);
                    } while (readLen != 0);
                }
                if (HtmlBody.Length == 0)
                    throw new Exception("Пустое сообщение");
                

                OnPropertyChanged("HtmlBody");
                pageSocket.Shutdown(SocketShutdown.Both);
                pageSocket.Close();
            }
            catch (Exception e)
            {
                if (e.InnerException is null)
                {
                    MessageBox.Show($"Ошибка: {e.Message}");
                }
                else
                {
                    MessageBox.Show($"Ошибка: {e.InnerException}");
                }

                pageSocket.Shutdown(SocketShutdown.Both);
                pageSocket.Close();
               
            }
            finally
            {
                IsDownloading = false;
            }
        }
    }
}
