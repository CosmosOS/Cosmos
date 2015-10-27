using System;
using System.Windows.Forms;
using System.IO;

namespace File2ByteArray_Converter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox2.Text = openFileDialog1.FileName;
                FileStream strm = new FileStream(openFileDialog1.FileName, FileMode.Open);
                StreamWriter strw = new StreamWriter(openFileDialog1.FileName + ".ByteArray.txt");
                textBox1.Text = "";
                strw.Write("byte[] " + Path.GetFileName(openFileDialog1.FileName).Replace('.', '_') + " = new byte[] \r\n{\r\n");
                int i = 0;
                bool first = true;
                for (int loc = 0; loc < strm.Length; loc++)
                {
                    string byt = ConvertToHex((uint)strm.ReadByte());
                    if (i >= 8)
                    {
                        i = 0;
                        strw.Flush();
                        byt = ", \r\n" + byt;
                    }
                    else
                    {
                        if (!first)
                        {
                            byt = ", " + byt;
                        }
                    }
                    strw.Write(byt);
                    first = false;
                    i++;
                }
                strw.Write("\r\n};");
                strw.Flush();
                strw.Close();
                strw.Dispose();
                strm.Flush();
                strm.Close();
                strm.Dispose();
                StreamReader strdr = new StreamReader(openFileDialog1.FileName + ".ByteArray.txt");
                textBox1.Text = strdr.ReadToEnd();
                strdr.Close();
                strdr.Dispose();
                MessageBox.Show("Conversion Complete!");
            }
        }

        private static string ConvertToHex(UInt32 num)
        {
            string xHex = string.Empty;

            if (num == 0)
            {
                xHex = "0";
            }
            else
            {
                while (num != 0)
                {
                    //Note; char is converted to string because Cosmos crashes when adding char and string. Frode, 7.june.
                    //TODO: Is this still true? I think Cosmos can handle char + string just fine now.
                    xHex = SingleDigitToHex((byte)(num & 0xf)) + xHex;
                    num = num >> 4;
                }
            }

            return "0x" + (xHex.PadLeft(2,'0'));
        }

        private static string SingleDigitToHex(byte d)
        {
            switch (d)
            {
                case 0:
                    return "0";
                case 1:
                    return "1";
                case 2:
                    return "2";
                case 3:
                    return "3";
                case 4:
                    return "4";
                case 5:
                    return "5";
                case 6:
                    return "6";
                case 7:
                    return "7";
                case 8:
                    return "8";
                case 9:
                    return "9";
                case 10:
                    return "A";
                case 11:
                    return "B";
                case 12:
                    return "C";
                case 13:
                    return "D";
                case 14:
                    return "E";
                case 15:
                    return "F";
            }
            return " ";

        }
    }
}
