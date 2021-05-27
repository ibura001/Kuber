using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;

namespace Service1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : ControllerBase
    {
        public HomeController(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        static readonly HttpClient client = new HttpClient();

        public IServiceProvider ServiceProvider { get; }

        [HttpGet]
        public string GetHello()
        {
            return "hello from service1";
        }


        [HttpGet]
        [Route("TimeOutRequest")]
        public string GetTimeOutResponse([FromQuery] int timeout = 0)
        {
            Thread.Sleep(timeout);
            return $"Response was sent for the {timeout} delay.";
        }

        [HttpGet]
        [Route("Get500Error")]
        public IActionResult CreateError()
        {
            throw new Exception("this is exception");
        }
        
        [HttpGet]
        [Route("CallServiceBrokenEndpoint")]
        public async Task<IActionResult> CallService1([FromQuery] int amountOfCallsToService1 = 10)
        {
            try
            {
                var tasks = new List<Task>();

                for (int i = 0; i < amountOfCallsToService1; i++)
                {
                    tasks.Add(client.GetAsync(
                        $"http://service2-service:8080/Snus/GetError"));
                }

                await Task.WhenAll(tasks);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }
    }
}