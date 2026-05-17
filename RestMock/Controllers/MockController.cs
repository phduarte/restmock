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

        [HttpPut]
        [Route("{guid}")]
        public IActionResult Update(Guid guid, EndpointModel request)
        {
            try
            {
                EndpointCollection.Update(guid, request);
                return Ok(EndpointCollection.GetById(guid));
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
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