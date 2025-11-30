using Chubb_Entity.Models;
using Chubb_Repository.Repository.Asegurado;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chubb_Service.Service.Asegurado
{
    public class AseguradoService(IAseguradoRepository aseguradoRepository) : IAseguradoService
    {
        public async Task<ResponseModel> RegistrarAsegurado(AseguradoModel asegurado)
        {
            return await aseguradoRepository.RegistrarAsegurado(asegurado);
        }
    }
}
