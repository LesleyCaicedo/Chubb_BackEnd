using Chubb_Entity.Commons;
using Chubb_Entity.Models;
using Chubb_Repository.Repository.Seguro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chubb_Service.Service.Seguro
{
    public class SeguroService (ISeguroRepository seguroRepository) : ISeguroService
    {
        public async Task<ResponseModel> RegistrarSeguro(SeguroModel seguro)
        {
            return await seguroRepository.RegistrarSeguro(seguro);
        }

        public async Task<ResponseModel> ConsultarSeguros(ConsultaFiltrosModel filtros)
        {
            return await seguroRepository.ConsultarSeguros(filtros);
        }

        public async Task<ResponseModel> ConsultarSeguroId(ConsultaFiltrosModel filtros, int id)
        {
            return await seguroRepository.ConsultarSeguroId(filtros, id);
        }

        public async Task<ResponseModel> ActualizarSeguro(SeguroModel seguro)
        {
            return await seguroRepository.ActualizarSeguro(seguro);
        }

        public async Task<ResponseModel> EliminarSeguro(int id)
        {
            return await seguroRepository.EliminarSeguro(id);
        }

        public async Task<ResponseModel> ConsultaGeneral(ConsultaFiltrosModel filtros, string cedula, string codigo)
        {
            return await seguroRepository.ConsultaGeneral(filtros, cedula, codigo);
        }

    }
}
