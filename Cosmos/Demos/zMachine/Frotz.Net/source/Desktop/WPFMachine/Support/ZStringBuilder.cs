using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WPFMachine.Support
{
    public class ZStringBuilder
    {
        private StringBuilder _builder;

        public ZStringBuilder()
        {
            _builder = new StringBuilder();
        }

        public void Append(char c)
        {
            if (_builder.Length < _currentPosition + 1)
            {
                _builder.Length = _currentPosition + 1;
            }
            _builder[_currentPosition++] = c;
        }

        public void Clear()
        {
            _builder.Clear();
            _currentPosition = 0;
        }

        public int Length
        {
            get { return _builder.Length; }
        }
        
        public void Remove(int startIndex, int length)
        {
            _builder.Remove(startIndex, length);
        }

        public new String ToString()
        {
            return _builder.ToString();
        }

        public void SetCurrentPosition(int position)
        {
            
        }

        int _currentPosition = 0;
    }
}
