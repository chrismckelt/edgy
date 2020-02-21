using System;

namespace WebApp
{
    /// <summary>
    /// sample data take from PHD_Raw_Data.csv
    /// </summary>
    public class Payload
    {
        public DateTime TimeStamp { get; set; }

        public DateTime ProcessedTimestamp { get; set; }

        public string ValueVarchar  { get; set; }

        public decimal ValueNumeric  { get; set; }
        
        
        public Int32 Confidence  { get; set; }

        public string TagKey  { get; set; }

    }
}
