﻿using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace CDOverhaul.CustomMultiplayer
{
    [Serializable]
    public class OverhaulPacket
    {
        public ulong Target;

        /// <summary>
        /// Called when we receive the packet
        /// </summary>
        public virtual void Handle() { }

        public virtual int GetChannel() => 0;
    }
}
