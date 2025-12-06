using Chubb_Entity.Commons;
using Chubb_Entity.Extensions;
using Chubb_Entity.Models;
using Chubb_Entity.Utils;
using Chubb_Repository.Repository.Cuenta;
using Chubb_Repository.Repository.Usuario;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Chubb_Service.Service.Cuenta
{
    public class CuentaService : ICuentaService
    {
        private readonly IConfiguration _configuration;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ICuentaRepository _cuentaRepository;

        public CuentaService(IConfiguration configuration, IUsuarioRepository usuarioRepository, ICuentaRepository cuentaRepository) 
        {
            _configuration = configuration;
            _usuarioRepository = usuarioRepository;
            _cuentaRepository = cuentaRepository;
        }

        public async Task<ResponseModel> Autenticar(LoginRequestModel login)
        {
            UsuarioModel usuario = await _usuarioRepository.GetByUsuarioAsync(login.Usuario);
            if (usuario == null || !EncryptHelper.VerificarClave(login.Clave!, usuario.Clave))
            {
                return new ResponseModel
                {
                    Estado = ResponseCode.Error,
                    Mensaje = "Usuario no existe o clave incorrecta."
                };
            }

            string Issuer = _configuration["JwtSettings:Issuer"]!;
            string Audience = _configuration["JwtSettings:Audience"]!;
            string Secret = _configuration["JwtSettings:Secret"]!;
            int ExpMinutes = int.TryParse(_configuration["JwtSettings:ExpMinutes"], out var m) ? m : 60;
            DateTime TokenExpiracion = TimeMethods.EC_Time().AddMinutes(ExpMinutes);

            SecurityTokenDescriptor TokenDescriptor = new()
            {
                Subject = new ClaimsIdentity(
                [
                    new Claim("IdUsuario", usuario.Id.ToString()),
                    new Claim("Usuario", usuario.Usuario),
                    new Claim("Nombre", usuario.Nombre),
                    new Claim("Correo", usuario.Correo),
                    new Claim("Rol", usuario.Rol),
                    new Claim("Permisos", usuario.Permisos)
                ]),
                Expires = TokenExpiracion,
                NotBefore = TimeMethods.EC_Time(),
                Issuer = Issuer,
                Audience = Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Secret!)), SecurityAlgorithms.HmacSha256Signature)
            };

            JwtSecurityTokenHandler tokenHandler = new();
            SecurityToken securityToken = tokenHandler.CreateToken(TokenDescriptor);
            string token = tokenHandler.WriteToken(securityToken);

            return new ResponseModel
            {
                Estado = ResponseCode.Success,
                Mensaje = "Autenticación exitosa.",
                Datos = new LoginResponseModel
                {
                    Usuario = login.Usuario,
                    TokenDeAcceso = token,
                    ExpiraEn = (int)TokenExpiracion.Subtract(TimeMethods.EC_Time()).TotalSeconds
                }
            };
        }

        public async Task<ResponseModel> CreateAsync(UsuarioModel u)
        {
            ResponseModel response = await _cuentaRepository.CreateAsync(u);

            if (response.Estado == ResponseCode.Success)
            {
                RegistroExitosoModel registro = response.Datos!.MapToModel<RegistroExitosoModel>();

                response.Datos = null;
            }

            return response;
        }

        public async Task<ResponseModel> CambiarClaveAsync(CambioClaveModel model)
        {
            UsuarioModel usuario = await _usuarioRepository.GetByIdAsync(model.IdUsuario);

            if (!EncryptHelper.VerificarClave(model.ClaveActual, usuario.Clave))
            {
                return new ResponseModel
                {
                    Estado = ResponseCode.Error,
                    Mensaje = "La clave actual es incorrecta."
                };
            }
            else if (EncryptHelper.VerificarClave(model.NuevaClave, usuario.Clave))
            {
                return new ResponseModel
                {
                    Estado = ResponseCode.Error,
                    Mensaje = "La nueva clave no puede ser igual a la clave actual."
                };
            }
            else
            {
                ResponseModel response = await _cuentaRepository.CambiarClaveAsync(model);

                return response;
            }
        }
    }
}
