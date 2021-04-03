using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace ConsoleApp8
{
    class Server
    {
        static int port = 8005;
        static void Main(string[] args)
        {
            // получаем адреса для запуска сокета
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);

            // создаем сокет
            Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                // связываем сокет с локальной точкой, по которой будем принимать данные
                listenSocket.Bind(ipPoint);
                // начинаем прослушивание, максимальное количество подключений - 10
                listenSocket.Listen(10); 
                Console.WriteLine("Сервер запущен. Ожидание подключений...");

                while (true)
                {
                    Socket handler = listenSocket.Accept();
                    Console.WriteLine("Клиент найден");
                    // получаем сообщение
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0; // количество полученных байтов
                    byte[] data = new byte[256]; // буфер для получаемых данных

                    do
                    {
                        bytes = handler.Receive(data);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (handler.Available > 0);

                    Console.WriteLine(DateTime.Now.ToShortTimeString() + ": " + builder.ToString());
                    // отправляем ответ
                    string message;
                    if (builder.ToString().Equals("Пока"))
                    {
                        message = "Пока";
                        data = Encoding.Unicode.GetBytes(message);
                        handler.Send(data);
                     //   handler.Shutdown(SocketShutdown.Both);
                     //   handler.Close();
                    }
                    else
                    {
                        message = "ваше сообщение доставлено";
                        data = Encoding.Unicode.GetBytes(message);
                        handler.Send(data);
                    }
                    // закрываем сокет
                    if (builder.ToString().Equals("Пока"))
                    {
                        handler.Shutdown(SocketShutdown.Both);
                        handler.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
