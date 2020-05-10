namespace EventProcessor.Kafka
{
    public class KafkaConfig
    {
        public string Broker { get; set; }

        public int ConsumerGroup { get; set; }

        public string DetectionTopic { get; set; }

        public string VideoRequestTopic { get; set; }
    }
}