using Chubb_Entity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chubb_Repository.Repository.Asegurado
{
    public interface IAseguradoRepository
    {
        Task<ResponseModel> RegistrarAsegurado(AseguradoModel asegurado);
    }
}
