using Microsoft.AspNetCore.Mvc;
using RestMock.Domain;

namespace RestMock.Controllers
{
    [ApiController]
    [Route("mocks")]
    public class MockController : ControllerBase
    {
        private readonly ILogger<MockController> _logger;

        public MockController(ILogger<MockController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public IActionResult CreateEndpoint(EndpointModel request)
        {
            EndpointCollection.Add(request);
            return Created($"/mocks/{request.Id}", request);
        }

        [HttpGet]
        [Route("{guid}")]
        public IActionResult Get(Guid guid)
        {
            var model = EndpointCollection.GetById(guid);

            if (model != null)
            {
                return Ok(model);

            }

            return NotFound();
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(EndpointCollection.GetAll());
        }

        [HttpDelete]
        [Route("{guid}")]
        public IActionResult Remove(Guid guid)
        {
            try
            {
                EndpointCollection.Remove(guid);
                return NoContent();
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }
    }
}