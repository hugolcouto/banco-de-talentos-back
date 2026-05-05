using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BancoDeTalentos.API.Controllers
{
    [Route("api/vagas")]
    [ApiController]
    public class JobsController : ControllerBase
    {
            // create
            [HttpPost]
            public IActionResult Create()
            {
                return Ok();
            }
        
            // read
            [HttpGet]
            public IActionResult Get()
            {
                return Ok();
            }
        
            [HttpGet("{id}")]
            public IActionResult GetById(int id)
            {
                return Ok();
            }
        
            // update
            [HttpPatch("{id}")]
            public IActionResult Update(int id)
            {
                return Ok();
            }
        
            // delete
            [HttpDelete("{id}")]
            public IActionResult Delete(int id)
            {
                return Ok();
            }
    }
}
