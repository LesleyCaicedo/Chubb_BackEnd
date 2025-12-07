using Azure;
using Chubb_Entity.Commons;
using Chubb_Entity.Models;
using Chubb_Service.Service.Asegurado;
using Chubb_Service.Service.Seguro;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Chubb_BackEnd.Controllers
{
    [Authorize]
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

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No se envió ningún archivo.");

            string extension = Path.GetExtension(file.FileName).ToLower();

            if (extension != ".xlsx" && extension != ".xls" && extension != ".txt")
                return BadRequest("Solo se permiten archivos Excel (.xlsx/.xls) o TXT.");

            try
            {

                MemoryStream ms = new();
                await file.CopyToAsync(ms);
                ms.Position = 0;

                if (extension.ToLower().Contains("txt"))
                {
                    await aseguradoService.ProcesarTxtAsync(ms);
                }
                else
                {
                    await aseguradoService.ProcesarExcelAsync(ms);
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
