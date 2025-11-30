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
            if (seguro.SumaAsegurada <= 0)
                return new ResponseModel { Estado = ResponseCode.Error, Mensaje = "La suma asegurada debe ser mayor a 0." };

            seguro.Prima = CalcularPrima(seguro.SumaAsegurada);

            return await seguroRepository.RegistrarSeguro(seguro);
        }

        private decimal CalcularPrima(decimal sumaAsegurada)
        {
            decimal tasa = 0.025m;
            decimal costoFijo = 5m;

            return Math.Round((sumaAsegurada * tasa) + costoFijo, 2);
        }

        public async Task<ResponseModel> ConsultarSeguros()
        {
            return await seguroRepository.ConsultarSeguros();
        }

        public async Task<ResponseModel> ActualizarSeguro(SeguroModel seguro)
        {
            if (seguro.SumaAsegurada <= 0)
                return new ResponseModel { Estado = ResponseCode.Error, Mensaje = "La suma asegurada debe ser mayor a 0." };

            seguro.Prima = CalcularPrima(seguro.SumaAsegurada);

            return await seguroRepository.ActualizarSeguro(seguro);
        }

        public async Task<ResponseModel> EliminarSeguro(int id)
        {
            return await seguroRepository.EliminarSeguro(id);
        }
    }
}
