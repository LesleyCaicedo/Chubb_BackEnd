using Azure;
using Chubb_Entity.Commons;
using Chubb_Entity.Models;
using Chubb_Service.Service.Asegurado;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Chubb_BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AseguradoController(IAseguradoService aseguradoService) : ControllerBase
    {
        [HttpPost("registrar")]
        public async Task<IActionResult> RegistrarAsegurado([FromBody] AseguradoModel asegurado)
        {
            ResponseModel response = await aseguradoService.RegistrarAsegurado(asegurado);

            if (response.Estado == ResponseCode.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }
    }
}
