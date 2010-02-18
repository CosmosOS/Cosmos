using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreLib.SIA.Services
{
    /// <summary>
    /// Services are different from apps in that the underlying messaging is exposed so that services can respond to messages via worker threads
    /// 
    /// All services are monitored adn if they dont respond to keep alive messages they can get killed and restarted  ( at first just restarting threads with the memory the same , if that doesnt work a full restart) 
    /// </summary>
    public class ServiceBase
    {
    }
}
