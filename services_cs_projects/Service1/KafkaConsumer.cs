using System;
using System.Threading; 
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Service1
{
    public class KafkaConsumer : IHostedService
    {
        private readonly ILogger<KafkaConsumer> _logger;
        private readonly ConsumerConfig _config;
        private readonly string _topic;

        public KafkaConsumer(ILogger<KafkaConsumer> logger, IConfiguration configuration)
        {
            _logger = logger;
            _topic = configuration.GetSection("Kafka")["Topic"];
            _config = new ConsumerConfig
            {
                GroupId = "st_consumer_group",
                BootstrapServers = configuration.GetSection("Kafka")["BootstrapServers"],
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            _logger.LogInformation($"CONSUMER TOPIC: {_topic}");

        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var builder = new ConsumerBuilder<Ignore, string>(_config).Build();
            builder.Subscribe(_topic);
            var cancelToken = new CancellationTokenSource();
            try
            {
                while (true)    
                {
                    var consumer = builder.Consume(cancelToken.Token);
                    _logger.LogInformation(
                        $"Message: {consumer.Message.Value} received from {consumer.TopicPartitionOffset}");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                builder.Close();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}