using Chubb_Entity.Commons;
using Chubb_Entity.Models;
using Chubb_Service.Service.Seguro;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Chubb_BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeguroController (ISeguroService seguroService) : ControllerBase
    {
        [HttpPost("[action]")]
        public async Task<IActionResult> RegistrarSeguro([FromBody] SeguroModel seguro)
        {
            ResponseModel response = await seguroService.RegistrarSeguro(seguro);
            
            if(response.Estado == ResponseCode.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ConsultarSeguros([FromBody] ConsultaFiltrosModel filtros)
        {
            ResponseModel response = await seguroService.ConsultarSeguros(filtros);
            if (response.Estado == ResponseCode.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ConsultarSeguroId([FromBody] ConsultaFiltrosModel filtros, int id)
        {
            ResponseModel response = await seguroService.ConsultarSeguroId(filtros, id);
            if (response.Estado == ResponseCode.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ActualizarSeguro([FromBody] SeguroModel seguro)
        {
            ResponseModel response = await seguroService.ActualizarSeguro(seguro);

            if (response.Estado == ResponseCode.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpDelete("[action]/{id}")]
        public async Task<IActionResult> EliminarSeguro(int id)
        {
            ResponseModel response = await seguroService.EliminarSeguro(id);
            if (response.Estado == ResponseCode.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }
    }
}
