namespace EventProcessor
{
    public class AppConfig
    {
        public string KafkaBrokers { get; set; }
        public string MainEventTopic { get; set; }

        public string ConsumerGroup { get; set; }
    }
}