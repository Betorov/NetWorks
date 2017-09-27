using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NetWorks.Model
{
    class SyncServer1
    {
        private string ip = "192.168.0.102";
        private int port = 43;

        public SyncServer1()
        {
            //ip = Ip;
            //try { this.port = Convert.ToInt32(port); }
            //catch (Exception e) { Console.WriteLine(e.ToString()); }
        }

        public static string data = null;

        public void StartListening()
        {

            IPAddress ipAddress = IPAddress.Parse(ip);
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

            Socket listener = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);
                while (true)
                {
                    Console.WriteLine("Waiting connecting");
                    // Program is suspended while waiting for an incoming connection.  
                    Socket handler = listener.Accept();
                    Console.WriteLine("Client connecting");

                    Byte[] Receive = new Byte[256];
                    //Читать сообщение будем в поток
                    using (MemoryStream MessageR = new MemoryStream())
                    {

                        //Количество считанных байт
                        Int32 ReceivedBytes;
                        Int32 Firest256Bytes = 0;
                        String FilePath = "";
                        do
                        {//Собственно читаем
                            ReceivedBytes = handler.Receive(Receive, Receive.Length, 0);
                            //Разбираем первые 256 байт
                            if (Firest256Bytes < 256)
                            {
                                Firest256Bytes += ReceivedBytes;
                                Byte[] ToStr = Receive;
                                //Учтем, что может возникнуть ситуация, когда они не могу передаться "сразу" все
                                if (Firest256Bytes > 256)
                                {
                                    Int32 Start = Firest256Bytes - ReceivedBytes;
                                    Int32 CountToGet = 256 - Start;
                                    Firest256Bytes = 256;
                                    //В случае если было принято >256 байт (двумя сообщениями к примеру)
                                    //Остаток (до 256 ) записываем в "путь файла" 
                                    ToStr = Receive.Take(CountToGet).ToArray();
                                    //А остальную часть - в будующий файл
                                    Receive = Receive.Skip(CountToGet).ToArray();
                                    MessageR.Write(Receive, 0, ReceivedBytes);
                                }

                                //Накапливаем имя файла
                                FilePath += Encoding.Default.GetString(ToStr);
                            }
                            else
                                //и записываем в поток
                                MessageR.Write(Receive, 0, ReceivedBytes);
                            //Читаем до тех пор, пока в очереди не останется данных
                        } while (ReceivedBytes == Receive.Length);
                        String resFilePath = FilePath.Substring(0, FilePath.IndexOf("\0"));
                        using (var File = new FileStream(resFilePath, FileMode.Create))
                        {//Записываем в файл
                            File.Write(MessageR.ToArray(), 0, MessageR.ToArray().Length);
                        }//Уведомим пользователя
                        handler.Shutdown(SocketShutdown.Both);
                        handler.Close();
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }

    class SyncClient
    {
        private string ip = "192.168.0.102";
        private int port = 43;

        public SyncClient()
        {

        }

        public void StartClient()
        {
            byte[] bytes = new byte[1024];

            try
            {
                IPHostEntry ipHostInfo = Dns.Resolve(ip);
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                Socket sender = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    sender.Connect(remoteEP);
                    string FileName = "G:\\r.txt";
                    string filePath = "G:\\r.txt";
                    //Выделяем имя файла
                    int index = FileName.Length - 1;
                    while (FileName[index] != '\\' && FileName[index] != '/')
                    {
                        index--;
                    }
                    //Получаем имя файла
                    String resFileName = "";
                    for (int i = index + 1; i < FileName.Length; i++)
                        resFileName += FileName[i];
                    //Записываем в лист
                    List<Byte> First256Bytes = Encoding.Default.GetBytes(resFileName).ToList();
                    Int32 Diff = 256 - First256Bytes.Count;
                    //Остаток заполняем нулями
                    for (int i = 0; i < Diff; i++)
                        First256Bytes.Add(0);
                    //Начинаем отправку данных
                    Byte[] ReadedBytes = new Byte[256];
                    using (var FileStream = new FileStream(filePath, FileMode.Open))
                    {
                        using (var Reader = new BinaryReader(FileStream))
                        {
                            Int32 CurrentReadedBytesCount;
                            //Вначале отправим название файла
                            sender.Send(First256Bytes.ToArray());
                            do
                            {
                                //Затем по частям - файл
                                CurrentReadedBytesCount = Reader.Read(ReadedBytes, 0, ReadedBytes.Length);
                                sender.Send(ReadedBytes, CurrentReadedBytesCount, SocketFlags.None);
                            }
                            while (CurrentReadedBytesCount == ReadedBytes.Length);
                        }
                    }

                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();

                }
                catch (ArgumentNullException ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                catch (SocketException ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
