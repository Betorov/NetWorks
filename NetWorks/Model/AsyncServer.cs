using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace NetWorks.Model
{
    public class AsyncServer
    {
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        string IP; int port;
        public AsyncServer(string IP, int port) { this.IP = IP; this.port = port; }

        public void StartListening()
        {
            // Data buffer for incoming data.
            byte[] bytes = new Byte[1024];
                                 
            IPHostEntry ipHostInfo = Dns.Resolve(IP);
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //Привяжем данный сокет к локальной конечной точке и будем ее слушать.
            try
            {
                listener.Bind(localEndPoint); listener.Listen(100);
                
                listener.Blocking = true;
                while (true)
                {
                    allDone.Reset();
                    
                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);

                    // Подождите, пока соединение не будет выполнено до продолжения
                    allDone.WaitOne();
                }
            } catch(Exception e)
            {
                MessageBox.Show(e.ToString());
            }           
        }

        public static void AcceptCallback(IAsyncResult arg)
        {
            //Сигнал о продолжении главного потока
            allDone.Set();

            Socket listener = (Socket)arg.AsyncState;
            Socket handler = listener.EndAccept(arg);

            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        public static void ReadCallback(IAsyncResult arg)
        {
            String content = String.Empty;

            StateObject state = (StateObject)arg.AsyncState;
            Socket handler = state.workSocket;

            int bytesRead = handler.EndReceive(arg);

            if(bytesRead > 0)
            {
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read
                // more data.
                content = state.sb.ToString();
                if(content.IndexOf("<EOF>") > -1)
                {                    
                    // Echo the data back to the client.
                    Send(handler, content);

                }
                else
                {
                    // Not all data received. Get more.
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }

            }
        }

        private static void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);             

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }


    }

    public class StateObject
    {
        /// <summary>
        /// Client Socket
        /// </summary>
        public Socket workSocket = null;

        public const int BufferSize = 1024;

        public byte[] buffer = new byte[BufferSize];

        public StringBuilder sb = new StringBuilder();
    }
}
