using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Sniffer.Model
{
    public class UdpGenerators
    {
        private Socket socket;

        #region Ipheader
        public byte Version { get; set; }

        public byte InternetHeaderLength { get; set; }

        public byte TypeOfService { get; set; }

        public ushort TotalLength { get; set; }

        public ushort Identification { get; set; }

        public byte ByteFlags { get; set; }

        public string Flags
        {
            get
            {
                switch (ByteFlags)
                {
                    case 1:
                        return "(MF)";
                    case 2:
                        return "(DF)";
                    case 3:
                        return "(DF, MF)";
                    default:
                        return "";
                }
            }
        }

        public ushort FragmentOffset { get; set; }

        public byte TimeToLive { get; set; }

        public byte TransportProtocol { get; set; }

        public string TransportProtocolName
        {
            get
            {
                switch (TransportProtocol)
                {
                    case 1:
                        return "ICMP";
                    case 2:
                        return "IGMP";
                    case 6:
                        return "TCP";
                    case 17:
                        return "UDP";
                    default:
                        return "Unknown";
                }
            }
        }

        public short HeaderChecksum { get; set; }

        public IPAddress SourceIPAddress { get; set; }

        public IPAddress DestinationIPAddress { get; set; }
        #endregion

        #region UdpHeader
        public ushort SourcePort { get; set; }

        public ushort DestinationPort { get; set; }

        public ushort Length { get; set; }

        public short Checksum { get; set; }
        #endregion
        private byte[] byteBufferData;
        private UDPHeader udpPacket;
        
        public UdpGenerators()
        {
            byteBufferData = new byte[1024];
            socket = new Socket(AddressFamily.InterNetwork,
                SocketType.Dgram, ProtocolType.Udp);
            this.SourcePort = 200;
            this.DestinationPort = 200;
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 8);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

        }

        public void sendToSocket(string ip)
        {
            byte[] byteBufferData = new byte[1024];
            for(int i = 0; i < byteBufferData.Length; i++)
            {
                byteBufferData[i] = 0;
            }
            while (true)
            {
                Socket sockets = new Socket(AddressFamily.InterNetwork,
                SocketType.Raw, ProtocolType.Raw);
                
                EndPoint sendEndPoint = new IPEndPoint(IPAddress.Parse(ip), 80);
              
                //socket.SendTo(byteBufferData, byteBufferData.Length, SocketFlags.None, sendEndPoint);
                socket.Close();
            }
        }

        public void sendToSocket(string data, string ip)
        {
            try
            {
                this.SourceIPAddress = IPAddress.Parse(ip);
                this.DestinationIPAddress = IPAddress.Parse(ip);
                this.Length = Convert.ToUInt16(data.Length);
                for (int i = 0; i < data.Length; i++)
                {
                    byteBufferData[i] = Convert.ToByte(data[i]);
                }
                EndPoint sendEndPoint = new IPEndPoint(this.SourceIPAddress, this.SourcePort);
                socket.SendTo(byteBufferData, data.Length, SocketFlags.None, sendEndPoint);
                socket.Close();
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            
        }

    }
}
