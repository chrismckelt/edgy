using System;

namespace DotNetDataRecorder
{
    public class Payload
    {
        public DateTime TimeStamp { get; set; }

        public bool IsAlive { get; set; }

        public double Confidence  { get; set; }

        public string TagKey  { get; set; }

    }
}
