﻿using System;

namespace WebApp
{
    public class Payload
    {
        public DateTime TimeStamp { get; set; }

        public bool IsAlive { get; set; }

        public double Temperature  { get; set; }

        public string TagKey  { get; set; }

    }
}
