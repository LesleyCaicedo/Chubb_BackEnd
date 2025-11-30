using Chubb_Entity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chubb_Service.Service.Seguro
{
    public interface ISeguroService
    {
        Task<ResponseModel> RegistrarSeguro(SeguroModel seguro);
        Task<ResponseModel> ConsultarSeguros(ConsultaFiltrosModel filtros);
        Task<ResponseModel> ConsultarSeguroId(ConsultaFiltrosModel filtros, int id);
        Task<ResponseModel> ActualizarSeguro(SeguroModel seguro);
        Task<ResponseModel> EliminarSeguro(int id);
    }
}
