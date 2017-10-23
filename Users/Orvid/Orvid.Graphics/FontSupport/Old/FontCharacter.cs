using System;
using System.Collections.Generic;
using System.Text;

namespace Orvid.Graphics.FontSupport.Old
{
    public class FontCharacter
    {
        private class SingleForm
        {
            byte[] Data;
            bool Filled;
            Image LoadedImage;
            byte Height;
            byte Width;
            UInt32 Bits;

            public SingleForm(byte[] data, byte height, byte width, UInt32 bits)
            {
                this.Data = data;
                this.Filled = false;
                this.Height = height;
                this.Width = width;
                this.Bits = bits;
            }

            public Image GetCharacter()
            {
                if (Filled)
                {
                    return LoadedImage;
                }
                else
                {
                    LoadedImage = LoadFromBinary(Data, Height, Width, Bits);
                    Filled = true;
                    Data = null;
                    return LoadedImage;
                }
            }

            private byte ReadByte(bool[] data)
            {
                byte r = 0;
                for (int i = 0; i < 8; i++)
                {
                    if (!data[i]) // The data loading seems to invert the bools.
                    {
                        r <<= 1;
                        r += 1;
                    }
                }
                return r;
            }

            private Image LoadFromBinary(byte[] data, byte height, byte width, uint bits)
            {
                #region LoadData
                bool[] idata = new bool[data.Length * 8];
                int bitnum = 0;
                for (int inc = 0; inc < data.Length; inc++)
                {
                    if ((data[inc] & 1) == 1)
                    {
                        idata[bitnum] = true;
                    }
                    bitnum++;
                    if ((data[inc] & 2) == 2)
                    {
                        idata[bitnum] = true;
                    }
                    bitnum++;
                    if ((data[inc] & 4) == 4)
                    {
                        idata[bitnum] = true;
                    }
                    bitnum++;
                    if ((data[inc] & 8) == 8)
                    {
                        idata[bitnum] = true;
                    }
                    bitnum++;
                    if ((data[inc] & 16) == 16)
                    {
                        idata[bitnum] = true;
                    }
                    bitnum++;
                    if ((data[inc] & 32) == 32)
                    {
                        idata[bitnum] = true;
                    }
                    bitnum++;
                    if ((data[inc] & 64) == 64)
                    {
                        idata[bitnum] = true;
                    }
                    bitnum++;
                    if ((data[inc] & 128) == 128)
                    {
                        idata[bitnum] = true;
                    }
                    bitnum++;
                }
                #endregion

                bitnum = 0;
                Image i = new Image(width, height);

                //for (uint y = 0; y < height; y++)
                for (uint x = 0; x < width; x++)
                {
                    //for (uint x = 0; x < width; x++)
                    for (uint y = 0; y < height; y++)
                    {
                        if (bitnum >= bits)
                        {
                            break;
                        }
                        if (idata[bitnum])
                        {
                            bitnum++;
                            if (idata[bitnum])
                            {
                                bitnum++;
                                i.SetPixel(x, y, Colors.Black); // Color the pixel white
                            }
                            else
                            {
                                bitnum++;
                                bool[] tmp = new bool[8];
                                Array.Copy(idata, bitnum, tmp, 0, 8);
                                bitnum += 8;
                                byte greyscale = ReadByte(tmp);
                                i.SetPixel(x, y, new Pixel(greyscale, greyscale, greyscale, 255));
                            }
                        }
                        else
                        {
                            bitnum++;
                            i.SetPixel(x, y, Colors.White); // Color the pixel black
                        }
                    }
                    if (bitnum >= bits)
                    {
                        break;
                    }
                }

                return i;
            }
        }
        
        Image[] forms;

        public FontCharacter(Image character, FontFlag flags)
        {
            Int32 i = (Int32)flags;
            forms = new Image[i + 1];
            forms[i] = character;
        }

        public void AddForm(Image character, FontFlag flags)
        {
            Int32 i = (Int32)flags;
            if (i > forms.Length - 1)
            {
                Image[] forms2 = new Image[i + 1];
                Array.Copy(forms, forms2, forms.Length);
                forms = forms2;
            }
            forms[i] = character;
        }

        public Image GetForm(FontFlag flags)
        {
            if ((Int32)flags < forms.Length)
            {
                return (forms[((Int32)flags)]);
            }
            else
            {
                return null;
            }
        }
    }
}
