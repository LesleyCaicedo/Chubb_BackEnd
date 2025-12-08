using Chubb_Entity.Commons;
using Chubb_Entity.Models;
using Chubb_Repository.Repository.Asegurado;
using Chubb_Repository.Repository.Seguro;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
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

        public async Task<ResponseModel> EliminarSeguro(int id, string usuarioGestor)
        {
            return await seguroRepository.EliminarSeguro(id, usuarioGestor);
        }

        public async Task<ResponseModel> ConsultaGeneral(ConsultaFiltrosModel filtros, string cedula, string codigo)
        {
            return await seguroRepository.ConsultaGeneral(filtros, cedula, codigo);
        }

        public async Task<ResponseModel> ConsultarSegurosPorEdad(ConsultaSegurosEdadModel filtros)
        {
            return await seguroRepository.ConsultarSegurosPorEdad(filtros);
        }

        public async Task ProcesarExcelAsync(Stream stream, string usuarioGestor)
        {
            ExcelPackage.License.SetNonCommercialPersonal("Keti");
            List<SeguroModel> seguros = new();

            using ExcelPackage package = new(stream);
            ExcelWorksheet sheet = package.Workbook.Worksheets[0];

            int rowCount = sheet.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++)
            {
                string codigo = sheet.Cells[row, 1].Text.Trim();
                string nombre = sheet.Cells[row, 2].Text.Trim();
                decimal sumaAsegurada = sheet.Cells[row, 3].GetValue<decimal>();

                decimal prima = CalcularPrima(sumaAsegurada);

                //Verificar si la celda tiene valor antes de leer
                int? edadMin = sheet.Cells[row, 4].Value != null
                    ? Convert.ToInt32(sheet.Cells[row, 4].Value)
                    : (int?)null;

                int? edadMax = sheet.Cells[row, 5].Value != null
                    ? Convert.ToInt32(sheet.Cells[row, 5].Value)
                    : (int?)null;

                SeguroModel seguro = null;
                seguro = new SeguroModel
                {
                    Codigo = codigo,
                    Nombre = nombre,
                    SumaAsegurada = sumaAsegurada,
                    Prima = prima,
                    EdadMin = edadMin,
                    EdadMax = edadMax,
                    UsuarioGestor = usuarioGestor
                };

                seguros.Add(seguro);
            }

            for (int i = 0; i < seguros.Count; i++)
            {
                await seguroRepository.RegistrarSeguro(seguros[i]);
            }
        }

        public async Task ProcesarTxtAsync(Stream stream, string usuarioGestor)
        {
            List<SeguroModel> seguros = new();

            using StreamReader reader = new(stream);

            await reader.ReadLineAsync();

            while (!reader.EndOfStream)
            {
                string line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split('\t');
                //if (parts.Length < 4) continue;

                string codigo = parts[0].Trim();
                string nombre = parts[1].Trim();
                decimal sumaAsegurada = Convert.ToDecimal(parts[2].Trim());

                decimal prima = CalcularPrima(sumaAsegurada);

                int edadMin = Convert.ToInt32(parts[3].Trim());
                int edadMax = Convert.ToInt32(parts[4].Trim());

                SeguroModel seguro = null;
                seguro = new SeguroModel
                {
                    Codigo = codigo,
                    Nombre = nombre,
                    SumaAsegurada = sumaAsegurada,
                    Prima = prima,
                    EdadMin = edadMin,
                    EdadMax = edadMax,
                    UsuarioGestor = usuarioGestor

                };

                seguros.Add(seguro);
            }

            for (int i = 0; i < seguros.Count; i++)
            {
                await seguroRepository.RegistrarSeguro(seguros[i]);
            }
        }

        private decimal CalcularPrima(decimal sumaAsegurada)
        {
            if (sumaAsegurada > 0)
                return sumaAsegurada * 0.05m;

            return 0m;
        }

    }
}
