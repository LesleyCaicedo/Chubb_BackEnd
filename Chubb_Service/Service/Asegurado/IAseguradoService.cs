using Chubb_Entity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chubb_Service.Service.Asegurado
{
    public interface IAseguradoService
    {
        Task<ResponseModel> RegistrarAsegurado(AseguradoModel asegurado);
    }
}
