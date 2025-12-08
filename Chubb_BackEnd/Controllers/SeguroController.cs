using Chubb_Entity.Commons;
using Chubb_Entity.Models;
using Chubb_Service.Service.Seguro;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Chubb_BackEnd.Controllers
{
    [Authorize]
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
        public async Task<IActionResult> EliminarSeguro(int id, string usuarioGestor)
        {
            ResponseModel response = await seguroService.EliminarSeguro(id, usuarioGestor);
            if (response.Estado == ResponseCode.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ConsultaGeneral([FromBody] ConsultaFiltrosModel filtros, string? cedula, string? codigo)
        {
            ResponseModel response = await seguroService.ConsultaGeneral(filtros, cedula, codigo);
            if (response.Estado == ResponseCode.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ConsultarSegurosPorEdad([FromBody] ConsultaSegurosEdadModel filtros)
        {
            try
            {
                ResponseModel response = await seguroService.ConsultarSegurosPorEdad(filtros);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseModel
                {
                    Estado = ResponseCode.Error,
                    Mensaje = $"Error al consultar seguros: {ex.Message}"
                });
            }
        }

        [HttpPost("uploadSeguros")]
        public async Task<IActionResult> UploadFile(IFormFile file, [FromQuery] string usuarioGestor)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No se envió ningún archivo.");

            var extension = Path.GetExtension(file.FileName).ToLower();

            if (extension != ".xlsx" && extension != ".xls" && extension != ".txt")
                return BadRequest("Solo se permiten archivos Excel (.xlsx/.xls) o TXT.");

            try
            {

                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                ms.Position = 0;

                if (extension.ToLower().Contains("txt"))
                {
                    await seguroService.ProcesarTxtAsync(ms, usuarioGestor);
                }
                else
                {
                    await seguroService.ProcesarExcelAsync(ms, usuarioGestor);
                }


                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error procesando archivo: {ex.Message}");
            }
        }
    }
}
