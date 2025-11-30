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
        public async Task<ResponseModel> ConsultarAsegurados(ConsultaFiltrosModel filtros)
        {
            return await aseguradoRepository.ConsultarAsegurados(filtros);
        }
        public async Task<ResponseModel> ConsultarAseguradoId(ConsultaFiltrosModel filtros, int id)
        {
            return await aseguradoRepository.ConsultarAseguradoId(filtros, id);
        }
        public async Task<ResponseModel> ActualizarAsegurado(AseguradoModel asegurado)
        {
            return await aseguradoRepository.ActualizarAsegurado(asegurado);
        }
        public async Task<ResponseModel> EliminarAsegurado(int idAsegurado)
        {
            return await aseguradoRepository.EliminarAsegurado(idAsegurado);
        }
    }
}
