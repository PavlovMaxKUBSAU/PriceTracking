using Microsoft.AspNetCore.Mvc;
using PriceTracking.Models;
using System;
using System.Threading.Tasks;

namespace PriceTracking.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PriceTrackingController : ControllerBase
    {
        private readonly PriceTrackingService _service;

        public PriceTrackingController(PriceTrackingService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> Subscribe(string adUrl, string email)
        {
            var subscriptionId = await _service.SubscribeAsync(adUrl, email);
            return Ok(subscriptionId);
        }

        [HttpGet]
        public async Task<ActionResult<List<Subscription>>> GetSubscriptions(string email)
        {
            var subscriptions = await _service.GetSubscriptionsAsync(email);
            return Ok(subscriptions);
        }
    }
}
