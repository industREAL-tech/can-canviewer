using industREAL.CAN.CORE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace industREAL.CAN.CanViewer.Models
{
    public sealed class CanFrameModel
    {
        public CanFrame canFrame { get; private set; }

        public DateTime timeStamp { get; set; }

        public List<string> flagsStr { get; private set; }

        public string comment { get; set; }

        public CanFrameModel(CanFrame actualFrame) { 
            canFrame = actualFrame; 
            flagsStr = BuildFlagsList(actualFrame._flags);
            timeStamp = DateTime.Now;
            comment = string.Empty;
        }

        private static List<string> BuildFlagsList(CanFlags flags)
        {
            var list = new List<string>();
            if (flags.HasFlag(CanFlags.BrsOn)) list.Add("BRS");
            if (flags.HasFlag(CanFlags.FdCanFormat)) list.Add("FD");
            if (flags.HasFlag(CanFlags.RemoteFrame)) list.Add("RTR");
            if (flags.HasFlag(CanFlags.ExtendedId)) list.Add("EXT");
            return list;
        }
    }
}
