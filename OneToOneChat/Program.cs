using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ITandDDP_Lab1
{
    class Program
    {
        private const string host = "127.0.0.1";
        private static int listeningPort;
        private static int connectionPort;
        private static Socket? socket;
        private static string userName = "";

        static void Main(string[] args)
        {
            Console.WriteLine("What's your nickname?");
            while (userName == null || userName.Trim() == string.Empty)
            {
                userName = Console.ReadLine();
            }
            GetPorts();
            Connect();
            CreateDialog();
            Console.ReadLine();
        }

        private static void GetPorts()
        {
            while (true)
            {
                Console.WriteLine("What port should we listen to?");
                if (!int.TryParse(Console.ReadLine(), out int port) && (port < 1000 || port >= 10000))
                {
                    Console.WriteLine("Invalid port!");
                    continue;
                }

                listeningPort = port;
                break;
            }
            while (true)
            {
                Console.WriteLine("What port should we post to?");
                if (!int.TryParse(Console.ReadLine(), out int port) && (port < 1000 || port >= 10000))
                {
                    Console.WriteLine("Invalid port!");
                    continue;
                }

                connectionPort = port;
                break;
            }
        }

        private static void Connect()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Parse(host), listeningPort));
            IPEndPoint connectionEndPoint = new IPEndPoint(IPAddress.Parse(host), connectionPort);

            while (true)
            {
                try
                {
                    socket.Connect(connectionEndPoint);
                    break;
                }
                catch (SocketException)
                {
                    Thread.Sleep(1000);
                    Console.WriteLine("Connection failed. Trying to reconnect...");
                }
            }

            Console.WriteLine($"Succesfully connected to {connectionEndPoint.Address}:{connectionEndPoint.Port}!\nYou can start chatting now!");
        }

        private static void SendUserName()
        {
            byte[] currentUserName = Encoding.Unicode.GetBytes(userName);
            socket.SendTo(currentUserName, new IPEndPoint(IPAddress.Parse(host), connectionPort));
        }
        private static string GetMessageWithAuthor(string message) => $"{userName}: {message}";

        private static void CreateDialog()
        {
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                new Task(ListenToPort).Start();


                while (true)
                {
                    byte[] data = Encoding.Unicode.GetBytes(GetMessageWithAuthor(Console.ReadLine()));
                    socket.SendTo(data, new IPEndPoint(IPAddress.Parse(host), connectionPort));
                }
            }
            catch (Exception)
            {
                Console.WriteLine("The connection was interrupted.");
            }
            finally
            {
                CloseUdp();
            }
        }

        private static void ListenToPort()
        {
            try
            {
                socket.Bind(new IPEndPoint(IPAddress.Parse(host), listeningPort));

                while (true)
                {
                    StringBuilder sb = new StringBuilder();
                    EndPoint endPoint = new IPEndPoint(IPAddress.Parse(host), connectionPort);

                    do
                    {
                        var data = new byte[64];
                        var count = socket.ReceiveFrom(data, ref endPoint);

                        sb.Append(Encoding.Unicode.GetString(data, 0, count));

                    } while (socket.Available > 0);

                    Console.WriteLine(sb.ToString());
                }
            }
            catch (Exception)
            {
                Console.WriteLine("The connection was interrupted");
            }
            finally
            {
                CloseUdp();
            }
        }

        private static void CloseUdp()
        {
            if (socket != null)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
        }
    }
}