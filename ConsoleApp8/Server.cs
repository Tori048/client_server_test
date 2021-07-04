using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using Gma.QrCodeNet.Encoding;
using Gma.QrCodeNet.Encoding.Windows.Render;
using System.Drawing.Imaging;
using MessagingToolkit.QRCode;
using MessagingToolkit.QRCode.Codec;
using MessagingToolkit.QRCode.Codec.Data;

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
        /*
         * Создаёт QR код с известным текстом.
         */
        static void QRcode()
        {
            string new_file_name = "qrTest";

            QrEncoder qrEncoder = new QrEncoder(ErrorCorrectionLevel.H);
            QrCode qrCode = new QrCode();
            qrEncoder.TryEncode("Hello, QRWord", out qrCode);
            var fCodeSize = new FixedCodeSize(200, QuietZoneModules.Two);
            fCodeSize.QuietZoneModules = QuietZoneModules.Four;
            GraphicsRenderer renderer = new GraphicsRenderer(fCodeSize, Brushes.Black, Brushes.White);

            MemoryStream ms = new MemoryStream();

            renderer.WriteToStream(qrCode.Matrix, ImageFormat.Png, ms);

            var imageTemp = new Bitmap(ms);

            var image = new Bitmap(imageTemp, new Size(new Point(200, 200)));

            image.Save(new_file_name + ".png", ImageFormat.Png);
        }

        static dynamic ReceiveMessage(ref byte[] byteMessage, int bytesRec)
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
                        message.yBody = br.ReadBytes(bytesRec - sizeof(int)); // общая длина сообщения - длина типа мессаги? надо переделать в более верный вариант
                        
                        return message;
                    }
                    else if (type == MessageType.ImageMsg)
                    {
                        Message message;
                        message.iType = type;
                        message.yBody = br.ReadBytes(bytesRec - sizeof(int)); // общая длина сообщения - длина типа мессаги? надо переделать в более верный вариант
                        Image x = (Bitmap)((new ImageConverter()).ConvertFrom(message.yBody));
                        x.Save("TESTQR.jpg");
                        QRCodeDecoder decoder = new QRCodeDecoder();
                        var dec = decoder.decode(new QRCodeBitmapImage(x as Bitmap));
                        Console.Write("dec = {0}", dec);
                        return message;
                    }
                }
            }
            return "fuck";
        }
        static void Main(string[] args)
        {
            //QRcode();
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
                    Message msg = ReceiveMessage(ref bytes, bytesRec);
                    if (msg.iType == MessageType.TextMsg)
                    {
                        Console.WriteLine(Encoding.UTF8.GetString(msg.yBody));
                    }
                    else if (msg.iType == MessageType.ImageMsg)
                    {
                        Console.WriteLine("Получено сообщение типа {0}", msg.iType);//, msg.yBody);
                    }
                    else
                        Console.WriteLine("что-то пошло не так");
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

                        /*  if (msg.yBody.IndexOf("<TheEnd>") > -1)
                          {
                              Console.WriteLine("Сервер завершил соединение с клиентом.");
                              break;
                          }*/

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