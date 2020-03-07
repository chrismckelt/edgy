using System;

namespace WebApp
{
    public class Payload
    {
        public DateTime TimeStamp { get; set; }

        public bool IsAirConditionerOn { get; set; }

        public double Temperature  { get; set; }

        public string TagKey  { get; set; }

    }
}
