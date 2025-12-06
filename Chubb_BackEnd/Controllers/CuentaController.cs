using Chubb_Entity.Commons;
using Chubb_Entity.Models;
using Chubb_Service.Service.Cuenta;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Chubb_BackEnd.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CuentaController : ControllerBase
    {
        private readonly ICuentaService _cuentaService;

        public CuentaController(ICuentaService cuentaService) 
        {
            _cuentaService = cuentaService;
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> Login([FromBody] LoginRequestModel login)
        {
            ResponseModel response = await _cuentaService.Autenticar(login);
            if (response.Estado == ResponseCode.Success)
            {
                return Ok(response);
            }

            return Unauthorized(response);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> RegistrarUsuario([FromBody] UsuarioModel usuario)
        {
            ResponseModel response = await _cuentaService.CreateAsync(usuario);
            if (response.Estado == ResponseCode.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> CambiarClave([FromBody] CambioClaveModel modelo)
        {
            ResponseModel response = await _cuentaService.CambiarClaveAsync(modelo);
            if (response.Estado == ResponseCode.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
    }
}
