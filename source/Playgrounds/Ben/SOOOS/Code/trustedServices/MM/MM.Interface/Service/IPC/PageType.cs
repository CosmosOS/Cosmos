using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ben.Services.MM
{
    public enum PageType : byte { Unknown, Kernel, IO, Paged }

    public enum PageSharing : byte { Unknown, Private, SharedRead }
}
