using NetWorks.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace NetWorks.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string iPSend;
        private string portSend;

        private string iPGet;
        private string portGet;

        /// <summary>
        /// Команда запуска сервера для приема файлов
        /// </summary>
        public ICommand startServer
        {
            get
            {
                return new delegateCommand(new Action(() =>
                {
                    try
                    {
                        AsyncServer serv = new AsyncServer(iPGet, Convert.ToInt32(portGet));
                        Thread tr = new Thread(new ThreadStart(serv.StartListening));
                        tr.Start();
                    } catch(Exception e)
                    {
                        MessageBox.Show(e.ToString());
                    }
                }));
            }
        }

        /// <summary>
        /// Отправка файлов на веденный IP,Port
        /// </summary>
        public ICommand sendFile
        {
            get
            {
                return new delegateCommand(new Action(() =>
                {

                }));
            }
        }

        public string IPSend { get => iPSend; set => iPSend = value; }
        public string IPGet { get => iPGet; set => iPGet = value; }

        public string PortSend { get => portSend; set => portSend = value; }
        public string PortGet { get => portGet; set => portGet = value; }
    }
}
