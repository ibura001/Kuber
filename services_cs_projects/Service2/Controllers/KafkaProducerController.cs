using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Service2.Controllers
{
    [Route("kafka")]
    [ApiController]
    public class KafkaProducerController : ControllerBase
    {
        private readonly ILogger<KafkaProducerController> _logger;

        private readonly ProducerConfig _config;
        private const string ConfigName = "Kafka";
        private readonly string _topic;

        public KafkaProducerController(ILogger<KafkaProducerController> logger, IConfiguration configuration)
        {
            _topic = configuration.GetSection(ConfigName)["Topic"];
            _config = new ProducerConfig
            {
                BootstrapServers = configuration.GetSection(ConfigName)["BootstrapServers"]
            };
            _logger = logger;
            _logger.LogInformation($"PRODUCER TOPIC: {_topic}");
        }

        [HttpPost]
        public IActionResult Post([FromQuery] string message)
        {
            try
            {
                return Created(string.Empty, SendToKafka(_topic, message));
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return BadRequest(e);
            }
        }

        [HttpGet]
        [Route("vault")]
        public async Task<IActionResult> Vault()
        {
            try
            {
                var jwtPath = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JWT_PATH"))
                    ? "/var/run/secrets/kubernetes.io/serviceaccount/token"
                    : Environment.GetEnvironmentVariable("JWT_PATH");

                var vaultUrl = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VAULT_ADDR"))
                    ? $"http://vault:8200"
                    : Environment.GetEnvironmentVariable("VAULT_ADDR");

                var jwtToken = await System.IO.File.ReadAllTextAsync(jwtPath!);

                const string authPath = "auth/kubernetes/login";
                const string secretPath = "v1/secretStorage/data/webapp/config";

                HttpResponseMessage response;

                var request = new VaultAuthRequestModel
                {
                    role = "webapp",
                    jwt = jwtToken
                };

                var requestModel = JsonConvert.SerializeObject(request, Formatting.Indented);

                _logger.LogInformation($"Vault url: {vaultUrl}\nJWT path: {jwtPath}\nJWT token = {jwtToken}\n\n");

                _logger.LogInformation(requestModel);

                using (var httpClient = new HttpClient())
                {
                    response = await httpClient.PutAsync($"{vaultUrl}/v1/{authPath}",
                        new StringContent(requestModel, Encoding.UTF8, "application/json"));
                }

                var responseText = await response.Content.ReadAsStringAsync();

                var responseModel =
                    JsonConvert.DeserializeObject<VaultLoginResponseModel>(responseText);

                _logger.LogInformation($"\n\n{JsonConvert.SerializeObject(responseModel, Formatting.Indented)}\n\n");

                if (responseModel != null)
                    _logger.LogInformation(
                        $"Response {response.StatusCode.ToString()}\nToken: {responseModel.Auth.ClientToken}");

                HttpResponseMessage secretResponse;

                using (var httpClient = new HttpClient())
                {
                    secretResponse = await httpClient.GetAsync($"{vaultUrl}/{secretPath}");
                }

                var secretResponseText = await secretResponse.Content.ReadAsStringAsync();

                SecretResponseModel secretResponseModel =
                    JsonConvert.DeserializeObject<SecretResponseModel>(secretResponseText);

                _logger.LogInformation(
                    $"\n\n{JsonConvert.SerializeObject(secretResponseModel, Formatting.Indented)}\n\n");

                var sb = new StringBuilder();

                if (secretResponseModel != null)
                    foreach (var (key, value) in secretResponseModel.Data.Data)
                    {
                        sb.Append($"{key}: {value}\n");
                    }

                _logger.LogInformation(sb.ToString());

                return Ok(responseText);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "");
                return BadRequest(e);
            }
        }

        private object SendToKafka(string topic, string message)
        {
            using var producer =
                new ProducerBuilder<Null, string>(_config).Build();
            try
            {
                _logger.LogInformation($"Send message: {message} to topic {topic}");

                return producer.ProduceAsync(topic, new Message<Null, string> {Value = message})
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }
        }
    }
}