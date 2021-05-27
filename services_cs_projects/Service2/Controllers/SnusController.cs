using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Service2.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SnusController : ControllerBase
    {
        private static readonly HttpClient Client = new HttpClient();

        [HttpGet]
        public string GetHello()
        {
            return "hello from service2";
        }


        [HttpGet]
        [Route("GetError")]
        public IActionResult CreateError()
        {
            throw new Exception("this is exception");
        }

        [HttpGet]
        [Route("CallServiceDelayedEndpoint")]
        public async Task<IActionResult> CallService1([FromQuery] int amountOfCallsToService1 = 10,
            [FromQuery] int delayForEachRequest = 1000)
        {
            try
            {
                var tasks = new List<Task>();

                for (int i = 0; i < amountOfCallsToService1; i++)
                {
                    tasks.Add(Client.GetAsync(
                        $"http://service1-service:8080/Home/TimeOutRequest?timeout={delayForEachRequest}"));
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