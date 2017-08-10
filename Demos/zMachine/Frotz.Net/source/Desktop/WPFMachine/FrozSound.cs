using System;
using System.Collections.Generic;

using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

using System.IO;

namespace WPFMachine {
    public class FrotzSound : UserControl {
        MediaElement _element;

        public FrotzSound() {
            _element = new MediaElement();
            _element.LoadedBehavior = MediaState.Manual;
            this.Content = _element;
        }

        public void LoadSound(byte[] Sound) {
            _element.Source = null;

            String temp = null;

            for (int i = 0; i < 1000 && temp == null; i++)
            {
                try
                {
                    temp = String.Format("{0}\\{1}.aiff", Path.GetTempPath(), i);

                    FileStream fs = new FileStream(temp, FileMode.Create);
                    fs.Write(Sound, 0, Sound.Length);
                    fs.Close();
                }
                catch (System.IO.IOException)
                {
                    i++;

                    temp = null;
                }
            }

            if (temp != null)
            {
                _element.Source = new Uri("file:///" + temp);
            }
        }

        public void PlaySound() {
            _element.Play();
        }

        public void StopSound()
        {
            _element.Stop();
        }
    }
}
