using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.IO;
using ProtoBuf;
using EventProcessor;
using Proto.Models;
namespace KafkaConsumer.Tests
{
    [TestFixture]
    public class ProtoTests
    {
        [Test]
        public void VerifyProtoFiles()
        {
           var buff= File.ReadAllBytes(@"H:\projects\NodeKafkaProducer\serialport-event-forwarder\test_dump_file");

            using (var ms=new MemoryStream(buff))
            {
              var km=  Serializer.Deserialize<KafkaMessage>(ms);
            }

            
        }
    }
}
