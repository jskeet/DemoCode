using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigiMixer.Mackie;

public enum MackiePacketType : byte
{
    Request = 0,
    Response = 1,
    Broadcast = 8
}
