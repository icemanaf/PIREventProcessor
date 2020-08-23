namespace EventProcessor.MessageActionFilters
{
    public class StationConfig
    {
        public Station[] Stations { get; set; }
    }

    public class Station
    {
        public string Description { get; set; }

        public string Id { get; set; }

        public bool Enabled { get; set; }
    }
}