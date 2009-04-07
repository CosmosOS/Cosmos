using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Threading;

namespace CosmosViewer
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private TcpClient client;
        private NetworkStream stream;
        private volatile bool running = false;
        private Thread thread;
        private byte[] buffer = new byte[16384];

        enum ClientState { NONE, CONNECTED, RECVD_VERSION, RECVD_SECURITY, RECVD_SECURITY_RESULT, READY };
        private ClientState state = ClientState.NONE;

        public Window1()
        {
            InitializeComponent();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            client = new TcpClient("192.168.20.123", 5900);
            state = ClientState.CONNECTED;

            stream = client.GetStream();
            thread = new Thread(this.exec);
            thread.Start();
        }

        private void exec()
        {
            this.running = true;
            while (this.running)
            {
                if (client.Client.Poll(100, SelectMode.SelectRead) == true)
                {
                    if (this.stream.DataAvailable == false)
                    {
                        Console.WriteLine("Server Closed connection Unexpectedly!!");
                        break;
                    }
                    else
                    {
                        int bytes = this.stream.Read(buffer, 0, buffer.Length);

                        Console.WriteLine("Data Recvd: " + BitConverter.ToString(buffer, 0, bytes));
                        switch (state)
                        {
                            case ClientState.CONNECTED:
                                string protocolString = Encoding.ASCII.GetString(buffer, 0, bytes);
                                Console.WriteLine("Recvd Protocol Header: " + protocolString);

                                stream.Write(buffer, 0, bytes);
                                state = ClientState.RECVD_VERSION;
                                break;
                            case ClientState.RECVD_VERSION:
                                Console.WriteLine("Recvd Security List Count=" + buffer[0]);

                                byte[] securityOption = new byte[] { 0x01 };
                                stream.Write(securityOption, 0, securityOption.Length);
                                state = ClientState.RECVD_SECURITY;
                                break;
                            case ClientState.RECVD_SECURITY:
                                Console.WriteLine("Recvd Security Result=" + buffer[0]);

                                byte[] clientInit = new byte[] { 0x00 };
                                stream.Write(clientInit, 0, clientInit.Length);
                                state = ClientState.RECVD_SECURITY_RESULT;
                                break;
                            case ClientState.RECVD_SECURITY_RESULT:
                                Console.WriteLine("Recvd Server Init...");
                                UInt16 framebufferWidth = (UInt16)((buffer[0] << 8) | buffer[1]);
                                UInt16 framebufferHeight = (UInt16)((buffer[2] << 8) | buffer[3]);

                                Console.WriteLine("\t Width=" + framebufferWidth + ", Height=" + framebufferHeight);
                                drawingArea.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                                    {
                                        drawingArea.Width = framebufferWidth;
                                        drawingArea.Height = framebufferHeight;
                                    }));
                                UInt32 nameLength = (UInt32)((buffer[20] << 24) | (buffer[21] << 16) | (buffer[22] << 8) | buffer[23]);
                                string sessionName = Encoding.ASCII.GetString(buffer, 24, (int)nameLength);
                                this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                                    {
                                        this.Title += " - " + sessionName;
                                    }));

                                byte[] initialUpdateRequest = new byte[10];
                                initialUpdateRequest[0] = 0x03;
                                initialUpdateRequest[2] = 0;
                                initialUpdateRequest[3] = 0;
                                initialUpdateRequest[4] = 0;
                                initialUpdateRequest[5] = 0;
                                initialUpdateRequest[6] = (byte)((framebufferWidth >> 8) & 0xFF);
                                initialUpdateRequest[7] = (byte)(framebufferWidth & 0xFF);
                                initialUpdateRequest[8] = (byte)((framebufferHeight >> 8) & 0xFF);
                                initialUpdateRequest[9] = (byte)(framebufferHeight & 0xFF);

                                stream.Write(initialUpdateRequest, 0, initialUpdateRequest.Length);
                                state = ClientState.READY;
                                break;
                            case ClientState.READY:
                                byte cmd = buffer[0];
                                switch (cmd)
                                {
                                    case 0:
                                        Console.WriteLine("Recvd FrameBufferUpdate...");
                                        UInt16 numRectangles = (UInt16)((buffer[2] << 8) | buffer[3]);
                                        Console.WriteLine("\tNumRectangles=" + numRectangles);
                                        int bufferOffset = 4;
                                        for (int rect = 0; rect < numRectangles; rect++)
                                        {
                                            UInt16 xPos = (UInt16)((buffer[bufferOffset + 0] << 8) | buffer[bufferOffset + 1]);
                                            UInt16 yPos = (UInt16)((buffer[bufferOffset + 2] << 8) | buffer[bufferOffset + 3]);
                                            UInt16 rectWidth = (UInt16)((buffer[bufferOffset + 4] << 8) | buffer[bufferOffset + 5]);
                                            UInt16 rectHeight = (UInt16)((buffer[bufferOffset + 6] << 8) | buffer[bufferOffset + 7]);
                                            Int32 encodingType = (Int32)((buffer[bufferOffset + 8] << 24) | (buffer[bufferOffset + 9] << 16) |
                                                (buffer[bufferOffset + 10] << 8) | buffer[bufferOffset + 11]);
                                            Console.WriteLine("Rect " + rect + ": x=" + xPos + ", y=" + yPos +
                                                ", w=" + rectWidth + ", h=" + rectHeight + ", enc=" + encodingType);
                                            bufferOffset += 12;

                                            if (encodingType == 0x22)
                                            {
                                                UInt32 numControls = (UInt32)((buffer[bufferOffset + 0] << 24) | (buffer[bufferOffset + 1] << 16) |
                                                    (buffer[bufferOffset + 2] << 8) | buffer[bufferOffset + 3]);
                                                bufferOffset += 4;

                                                for (int idx = 0; idx < numControls; idx++)
                                                {
                                                    UInt16 type = BitConverter.ToUInt16(buffer, bufferOffset);

                                                    switch (type)
                                                    {
                                                        case 0: // Background Color
                                                            UInt32 bgPixelColor = BitConverter.ToUInt32(buffer, bufferOffset + 2);
                                                            byte red = (byte)((bgPixelColor >> 16) & 0xFF);
                                                            byte green = (byte)((bgPixelColor >> 8) & 0xFF);
                                                            byte blue = (byte)(bgPixelColor & 0xFF);
                                                            Color bgColor = Color.FromRgb(red, green, blue);
                                                            drawingArea.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                                                            {
                                                                drawingArea.Background = new SolidColorBrush(bgColor);
                                                            }));
                                                            bufferOffset += 6;
                                                            break;
                                                        case 1: // Linear Gradient Background
                                                            UInt32 bgPixelColorStart = BitConverter.ToUInt32(buffer, bufferOffset + 2);
                                                            UInt32 bgPixelColorEnd = BitConverter.ToUInt32(buffer, bufferOffset + 6);
                                                            byte redStart = (byte)((bgPixelColorStart >> 16) & 0xFF);
                                                            byte greenStart = (byte)((bgPixelColorStart >> 8) & 0xFF);
                                                            byte blueStart = (byte)(bgPixelColorStart & 0xFF);
                                                            Color bgColorStart = Color.FromRgb(redStart, greenStart, blueStart);
                                                            byte redEnd = (byte)((bgPixelColorEnd >> 16) & 0xFF);
                                                            byte greenEnd = (byte)((bgPixelColorEnd >> 8) & 0xFF);
                                                            byte blueEnd = (byte)(bgPixelColorEnd & 0xFF);
                                                            Color bgColorEnd = Color.FromRgb(redEnd, greenEnd, blueEnd);
                                                            drawingArea.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                                                            {
                                                                drawingArea.Background = new LinearGradientBrush(bgColorStart, bgColorEnd, 90);
                                                            }));
                                                            bufferOffset += 6;
                                                            break;
                                                    }

                                                }
                                            }
                                        }
                                        break;
                                }
                                break;
                        }
                    }
                }
            }

            stream.Close();
            client.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if ((client != null) && (client.Connected == true))
            {
                this.running = false;
                Thread.Sleep(1000);
            }
        }
    }
}
