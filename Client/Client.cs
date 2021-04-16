using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Drawing;

namespace SocketClient
{
    class Program
    {
        static string FileName = "E:\\1.jpg";//"C:\\Users\\Скит\\source\\repos\\ConsoleApp8\\ConsoleApp8\\testImage";
        static void Main(string[] args)
        {
            try
            {
                SendMessageFromSocket(8005);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                Console.ReadLine();
            }
        }

        static void SendText (Socket sender, string message)
        {
            byte[] msg = Encoding.UTF8.GetBytes(message);
            int bytesSent = sender.Send(msg);
        }

        static Image getInfoPhoto()
        {
            Image iPhoto = Image.FromFile(FileName);//("testImage.jpeg");
            Console.WriteLine(" Hei = {0}, WId = {1}",iPhoto.Height.ToString(), iPhoto.Width.ToString());
            return iPhoto;
        }

        static void SendMessageFromSocket(int port)
        {
            // Буфер для входящих данных
            byte[] bytes = new byte[125000];

            // Соединяемся с удаленным устройством

            // Устанавливаем удаленную точку для сокета
            IPHostEntry ipHost = Dns.GetHostEntry("127.0.0.1");
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port);

            Socket sender = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Соединяем сокет с удаленной точкой
            sender.Connect(ipEndPoint);

            Console.Write("Введите сообщение: ");
            string message = Console.ReadLine();
            Console.WriteLine("Сокет соединяется с {0} ", sender.RemoteEndPoint.ToString());
            if (message == "отправь изображение")
            {
                getInfoPhoto();
                sender.SendFile(FileName);
            }
            else
            {
                SendText(sender, message);
            }

            // Получаем ответ от сервера
            int bytesRec = sender.Receive(bytes);

            Console.WriteLine("\nОтвет от сервера: {0}\n\n", Encoding.UTF8.GetString(bytes, 0, bytesRec));

            // Используем рекурсию для неоднократного вызова SendMessageFromSocket()
            if (message.IndexOf("<TheEnd>") == -1)
                SendMessageFromSocket(port);

            // Освобождаем сокет
            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }
    }
}