using ECO.Api.Helper;
using ECO.DAL.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace ECO.Api.Controller
{
    
        [Route("Error/{StatusCode}")]
        [ApiController]
    public class ErrorController : ControllerBase
    {
        [HttpGet]
        [HttpPost]
        [HttpPut]
        [HttpDelete]
        [HttpPatch]
        public IActionResult Error(int StatusCode)
        {
           return new ObjectResult(new ResponseApi(StatusCode, $"Error {StatusCode} occurred"));    
        }
    }
}