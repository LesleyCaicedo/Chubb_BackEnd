using Azure;
using Chubb_Entity.Commons;
using Chubb_Entity.Models;
using Chubb_Service.Service.Asegurado;
using Chubb_Service.Service.Seguro;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;

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
        public async Task<IActionResult> EliminarAsegurado(int idAsegurado, [FromQuery] string usuarioGestor)
        {
            ResponseModel response = await aseguradoService.EliminarAsegurado(idAsegurado, usuarioGestor);
            if (response.Estado == ResponseCode.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file, [FromQuery] string usuarioGestor, [FromQuery] string? reglasJson = null)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new ResponseModel
                {
                    Estado = ResponseCode.Error,
                    Mensaje = "No se envió ningún archivo."
                });

            if (string.IsNullOrWhiteSpace(usuarioGestor))
                return BadRequest(new ResponseModel
                {
                    Estado = ResponseCode.Error,
                    Mensaje = "Debe proporcionar el usuario gestor."
                });

            string extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".xlsx" && extension != ".xls" && extension != ".txt")
                return BadRequest(new ResponseModel
                {
                    Estado = ResponseCode.Error,
                    Mensaje = "Solo se permiten archivos Excel (.xlsx/.xls) o TXT."
                });

            try
            {
                // Deserializar reglas si existen
                List<ReglaAsignacionModel>? reglas = null;
                if (!string.IsNullOrEmpty(reglasJson))
                {
                    try
                    {
                        JsonSerializerOptions options = new()
                        {
                            PropertyNameCaseInsensitive = true,
                            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                        };

                        reglas = JsonSerializer.Deserialize<List<ReglaAsignacionModel>>(reglasJson, options);

                        if (reglas != null && reglas.Any())
                        {
                            foreach (ReglaAsignacionModel regla in reglas)
                            {
                                if (regla.IdSeguro <= 0)
                                {
                                    return BadRequest(new ResponseModel
                                    {
                                        Estado = ResponseCode.Error,
                                        Mensaje = $"Regla con IdSeguro inválido: {regla.IdSeguro}"
                                    });
                                }

                                if (!regla.EsGeneral && (!regla.EdadMinima.HasValue || !regla.EdadMaxima.HasValue))
                                    throw new Exception("Reglas no generales deben tener edad mínima y máxima");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(new ResponseModel
                        {
                            Estado = ResponseCode.Error,
                            Mensaje = $"Error al procesar reglas: {ex.Message}"
                        });
                    }
                }

                MemoryStream ms = new();
                await file.CopyToAsync(ms);
                ms.Position = 0;

                if (extension.Contains("txt"))
                {
                    await aseguradoService.ProcesarTxtAsync(ms, usuarioGestor, reglas);
                }
                else
                {
                    await aseguradoService.ProcesarExcelAsync(ms, usuarioGestor, reglas);
                }

                return Ok(new ResponseModel
                {
                    Estado = ResponseCode.Success,
                    Mensaje = "Carga masiva procesada exitosamente.",
                    Datos = new
                    {
                        archivo = file.FileName,
                        usuarioGestor = usuarioGestor,
                        modoParametrizado = reglas != null && reglas.Any(),
                        reglasAplicadas = reglas?.Count ?? 0,
                        detalleReglas = reglas?.Select(r => new
                        {
                            seguro = r.NombreSeguro,
                            rangoEdad = r.EsGeneral
                                ? "Sin restricción"
                                : $"{r.EdadMinima}-{r.EdadMaxima} años"
                        })
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseModel
                {
                    Estado = ResponseCode.Error,
                    Mensaje = $"Error procesando archivo: {ex.Message}"
                });
            }
        }
    }
}
