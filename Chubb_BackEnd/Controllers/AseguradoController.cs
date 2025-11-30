using Azure;
using Chubb_Entity.Commons;
using Chubb_Entity.Models;
using Chubb_Service.Service.Asegurado;
using Chubb_Service.Service.Seguro;
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

        [HttpPost("[action]")]
        public async Task<IActionResult> ConsultarAsegurados([FromBody] ConsultaFiltrosModel filtros)
        {
            ResponseModel response = await aseguradoService.ConsultarAsegurados(filtros);
            if (response.Estado == ResponseCode.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ConsultarAseguradoId([FromBody] ConsultaFiltrosModel filtros, int id)
        {
            ResponseModel response = await aseguradoService.ConsultarAseguradoId(filtros, id);
            if (response.Estado == ResponseCode.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ActualizarAsegurado([FromBody] AseguradoModel asegurado)
        {
            ResponseModel response = await aseguradoService.ActualizarAsegurado(asegurado);
            if (response.Estado == ResponseCode.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpDelete("[action]/{idAsegurado}")]
        public async Task<IActionResult> EliminarAsegurado(int idAsegurado)
        {
            ResponseModel response = await aseguradoService.EliminarAsegurado(idAsegurado);
            if (response.Estado == ResponseCode.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }
    }
}
