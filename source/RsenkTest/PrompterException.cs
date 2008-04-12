using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RsenkTest
{
    class PrompterException : Exception
    {
        public PrompterException() : base() { }
        public PrompterException(string message) : base(message) { }
        public PrompterException(string message, Exception innerExc) : base(message, innerExc) { }
    }
}
