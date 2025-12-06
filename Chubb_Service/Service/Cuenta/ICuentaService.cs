using Chubb_Entity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chubb_Service.Service.Cuenta
{
    public interface ICuentaService
    {
        Task<ResponseModel> Autenticar(LoginRequestModel login);
        Task<ResponseModel> CreateAsync(UsuarioModel u);
        Task<ResponseModel> CambiarClaveAsync(CambioClaveModel model);
    }
}
