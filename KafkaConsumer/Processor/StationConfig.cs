using System;
using System.Collections.Generic;



namespace PIREventProcessor.Processor
{
    public class StationConfig
    {
        public Station[] Stations { get; set; }

    }



    public class Station
    {
        public string Desc { get; set; }

        public string Id { get; set; }

        public bool Enabled { get; set; }

    }
}
