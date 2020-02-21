using System;
using System.Collections.Generic;
using System.Text;
using ChanceNET;
using ChanceNET.Attributes;

namespace DotNetDataGenerator
{
    /// <summary>
    /// sample data take from PHD_Raw_Data.csv
    /// </summary>
    public class Payload
    {
        [ChanceNET.Attributes.Date(2020,Month.January,01,2019,2025)]
        public DateTime TimeStamp { get; set; }

        [ChanceNET.Attributes.Date(2020,Month.January,01,2019,2025)]
        public DateTime ProcessedTimestamp { get; set; }

        public string ValueVarchar  { get; set; }

        public decimal ValueNumeric  { get; set; }
        
        
        public Int32 Confidence  { get; set; }

        public string TagKey  { get; set; }

    }
}
