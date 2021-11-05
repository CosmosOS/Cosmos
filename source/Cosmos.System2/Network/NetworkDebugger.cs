using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Debug.Kernel;
using Cosmos.HAL;
using Cosmos.System.Network.Config;
using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.IPv4.TCP;
using Cosmos.System.Network.IPv4.UDP.DHCP;
using Con = System.Console;
namespace Cosmos.System.Network
{
    public class NetworkDebugger
    {
        private TcpListener xListener = null;
        private TcpClient xClient = null;
        /// <summary>
        /// Remote IP Address
        /// </summary>
        public Address Ip { get; set; }

        /// <summary>
        /// Port used
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Create NetworkDebugger class (used to listen for a debugger connection)
        /// </summary>
        public NetworkDebugger(int port)
        {
            Port = port;
            xListener = new TcpListener((ushort)port);
        }

        /// <summary>
        /// Create NetworkDebugger class (used to connect to a remote debugger)
        /// </summary>
        public NetworkDebugger(Address ip, int port)
        {
            Ip = ip;
            Port = port;
            xClient = new TcpClient(port);
        }

        /// <summary>
        /// Start debugger
        /// </summary>
        public void Start()
        {
            if (xClient == null)
            {
                xListener.Start();
                
                Con.WriteLine("Waiting for remote debugger connection at " + NetworkConfig.CurrentConfig.Value.IPAddress.ToString() + ":" + Port);
                xClient = xListener.AcceptTcpClient(); //blocking
            }
            else if (xListener == null)
            {
                xClient.Connect(Ip, Port);
            }

            Send("--- Cosmos Network Debugger ---");
            Send("Debugger Connected!");
        }

        /// <summary>
        /// Send text to the debugger
        /// </summary>
        public void Send(string message)
        {
            xClient.Send(Encoding.ASCII.GetBytes("[" + Time.TimeString(true, true, true) + "] - " + message + "\r\n"));
        }

        /// <summary>
        /// Stop the debugger by closing TCP Connection
        /// </summary>
        public void Stop()
        {
            Con.WriteLine("Closing Debugger connection");
            Send("Closing...");
            xClient.Close();
        }
    }

    public static class Time
    {

        static int Hour() { return RTC.Hour; }

        static int Minute() { return RTC.Minute; }

        static int Second() { return RTC.Second; }

        static int Century() { return RTC.Century; }

        static int Year() { return RTC.Year; }

        static int Month() { return RTC.Month; }

        static int DayOfMonth() { return RTC.DayOfTheMonth; }

        static int DayOfWeek() { return RTC.DayOfTheWeek; }

        static string getTime24(bool hour, bool min, bool sec)
        {
            string timeStr = "";
            if (hour)
            {
                if (Hour().ToString().Length == 1)
                {
                    timeStr += "0" + Hour().ToString();
                }
                else
                {
                    timeStr += Hour().ToString();
                }
            }
            if (min)
            {
                if (Minute().ToString().Length == 1)
                {
                    timeStr += ":";
                    timeStr += "0" + Minute().ToString();
                }
                else
                {
                    timeStr += ":";
                    timeStr += Minute().ToString();
                }
            }
            if (sec)
            {
                if (Second().ToString().Length == 1)
                {
                    timeStr += ":";
                    timeStr += "0" + Second().ToString();
                }
                else
                {
                    timeStr += ":";
                    timeStr += Second().ToString();
                }
            }
            return timeStr;
        }

        static string getTime12(bool hour, bool min, bool sec)
        {
            string timeStr = "";
            if (hour)
            {
                if (Hour() > 12)
                    timeStr += Hour() - 12;
                else
                    timeStr += Hour();
            }
            if (min)
            {
                if (Minute().ToString().Length == 1)
                {
                    timeStr += ":";
                    timeStr += "0" + Minute().ToString();
                }
                else
                {
                    timeStr += ":";
                    timeStr += Minute().ToString();
                }
            }
            if (sec)
            {
                if (Second().ToString().Length == 1)
                {
                    timeStr += ":";
                    timeStr += "0" + Second().ToString();
                }
                else
                {
                    timeStr += ":";
                    timeStr += Second().ToString();
                }
            }
            if (hour)
            {
                if (Hour() > 12)
                    timeStr += " PM";
                else
                    timeStr += " AM";
            }
            return timeStr;
        }

        /// <summary>
        /// return the Hour String
        /// </summary>
        /// <returns>Actual Hour</returns>
        public static string TimeString(bool hour, bool min, bool sec)
        {
            return getTime12(hour, min, sec);
        }

        /// <summary>
        /// return the Year String
        /// </summary>
        /// <returns>Actual Year</returns>
        public static string YearString()
        {
            int intyear = Year();
            string stringyear = intyear.ToString();

            if (stringyear.Length == 2)
            {
                stringyear = "20" + stringyear;
            }
            return stringyear;
        }

        /// <summary>
        /// return the Month String
        /// </summary>
        /// <returns>Actual Month</returns>
        public static string MonthString()
        {
            int intmonth = Month();
            string stringmonth = intmonth.ToString();

            if (stringmonth.Length == 1)
            {
                stringmonth = "0" + stringmonth;
            }
            return stringmonth;
        }

        /// <summary>
        /// return the Day String
        /// </summary>
        /// <returns>Actual Day</returns>
        public static string DayString()
        {
            int intday = DayOfMonth();
            string stringday = intday.ToString();

            if (stringday.Length == 1)
            {
                stringday = "0" + stringday;
            }
            return stringday;
        }

    }
}
