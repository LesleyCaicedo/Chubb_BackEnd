using Chubb_Entity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chubb_Repository.Repository.Seguro
{
    public interface ISeguroRepository
    {
        Task<ResponseModel> RegistrarSeguro(SeguroModel seguro);
        Task<ResponseModel> ConsultarSeguros(ConsultaFiltrosModel filtros);
        Task<ResponseModel> ConsultarSeguroId(ConsultaFiltrosModel filtros, int id);
        Task<ResponseModel> ActualizarSeguro(SeguroModel seguro);
        Task<ResponseModel> EliminarSeguro(int id);
        Task<ResponseModel> ConsultaGeneral(ConsultaFiltrosModel filtros, string cedula, string codigo);
        Task<ResponseModel> ConsultarSegurosPorEdad(ConsultaSegurosEdadModel filtros);
    }
}
