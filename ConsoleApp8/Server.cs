using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace SocketServer
{
    enum MessageType
    {
        TextMsg = 0x01, // простое текстовое сообщение
        ImageMsg
    }

    //составляющие сообщения
    struct Message
    {
        public MessageType iType; // тип сообщения
        public byte[] yBody; // содержимое сообщения
    };
    class Program
    {
        static dynamic ReceiveMessage(ref byte[] byteMessage)
        {
            using (var ms = new MemoryStream(byteMessage))
            {
                using (var br = new BinaryReader(ms, Encoding.UTF8))
                {
                    MessageType type = (MessageType)br.ReadInt32();
                    if (type == MessageType.TextMsg)
                    {
                        Message message;
                        message.iType = type;
                        message.yBody = br.ReadBytes(byteMessage.Length - 1); // общая длина сообщения - длина типа мессаги? надо переделать в более верный вариант
                        return message;
                    }
                    else if (type == MessageType.ImageMsg)
                    {
                        // ToDO - распиши как работать на сервере с изображением. Клиент уже вроде умеет его нормально отправлять
                    }
/*                    else
                    {
                        //тут должна быть обработка мессаги с изображением

                    }*/

                }
            }
            return "fuck";
        }
        static void Main(string[] args)
        {
            // Устанавливаем для сокета локальную конечную точку
            IPHostEntry ipHost = Dns.GetHostEntry("127.0.0.1");
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 8005);

            // Создаем сокет Tcp/Ip
            Socket sListener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Назначаем сокет локальной конечной точке и слушаем входящие сокеты
            try
            {
                sListener.Bind(ipEndPoint);
                sListener.Listen(10);

                // Начинаем слушать соединения
                while (true)
                {
                    Console.WriteLine("Ожидаем соединение через порт {0}", ipEndPoint);

                    // Программа приостанавливается, ожидая входящее соединение
                    Socket handler = sListener.Accept();
             //       string data = null;

                    // Мы дождались клиента, пытающегося с нами соединиться
                    byte[] bytes = new byte[125000];
                    
                    int bytesRec = handler.Receive(bytes);
                    dynamic msg = ReceiveMessage(ref bytes);
                    if (msg is string)
                    {
                        Console.WriteLine(msg);
                    }
                    else
                    {
                        Console.WriteLine("Получено сообщение типа {0}, содержимое = {1}", msg.iType, msg.sBody);
                    }
                    //для получения и сохранения картинок
                    /*Image x = (Bitmap)((new ImageConverter()).ConvertFrom(bytes));
                    x.Save("TEST.jpg");*/


                    //data += Encoding.UTF8.GetString(bytes, 0, bytesRec);

                    // Показываем данные на консоли
                    //Console.Write("Полученный текст: " + data + "\n\n");

                    // Отправляем ответ клиенту\
                  /*  string reply = "Спасибо за запрос в " + data.Length.ToString()
                            + " символов";*/
                    //byte[] msg = Encoding.UTF8.GetBytes(reply);
                    //handler.Send(msg);

                    if (msg.sBody.IndexOf("<TheEnd>") > -1)
                    {
                        Console.WriteLine("Сервер завершил соединение с клиентом.");
                        break;
                    }

                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
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
    }
}