using Chubb_Entity.Models;
using Chubb_Repository.Repository.Asegurado;
using Chubb_Repository.Repository.Seguro;
using OfficeOpenXml;
using System.Text.Json;

namespace Chubb_Service.Service.Asegurado
{
    public class AseguradoService(IAseguradoRepository aseguradoRepository, ISeguroRepository segurosRepository) : IAseguradoService
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

        public async Task ProcesarExcelAsync(Stream stream)
        {
            ExcelPackage.License.SetNonCommercialPersonal("Keti");
            var asegurados = new List<AseguradoModel>();
            List<SeguroModel> seguros = await ObtenerSeguroPorEdad();

            using var package = new ExcelPackage(stream);
            var sheet = package.Workbook.Worksheets[0];

            var rowCount = sheet.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++)
            {
                var cedula = sheet.Cells[row, 1].Text.Trim();
                var nombre = sheet.Cells[row, 2].Text.Trim();
                var telefono = sheet.Cells[row, 3].Text.Trim();
                var fechaNac = DateOnly.Parse(sheet.Cells[row, 4].Text);

                var edad = CalcularEdad(fechaNac);

                AseguradoModel asegurado = null;
                asegurado = new AseguradoModel
                {
                    Cedula = cedula,
                    Nombre = nombre,
                    Telefono = telefono,
                    FechaNacimiento = fechaNac,
                    Edad = edad,
                    Seguros = []
                };

                // Logica para asignar el seguro acorde a su edad
                asegurado.Seguros.Add(seguros
                .Where(s => (s.EdadMin == null || asegurado.Edad >= s.EdadMin) &&
                            (s.EdadMax == null || asegurado.Edad <= s.EdadMax))
                .OrderByDescending(s => s.Prima)
                .ThenBy(s => s.IdSeguro)
                .FirstOrDefault().IdSeguro);

                asegurados.Add(asegurado);
            }

            for (int i = 0; i < asegurados.Count; i++)
            {
                await aseguradoRepository.RegistrarAsegurado(asegurados[i]);
            }
        }

        public async Task ProcesarTxtAsync(Stream stream)
        {
            var asegurados = new List<AseguradoModel>();
            List<SeguroModel> seguros = await ObtenerSeguroPorEdad();

            using var reader = new StreamReader(stream);

            await reader.ReadLineAsync();

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split('\t');
                //if (parts.Length < 4) continue;

                var cedula = parts[0].Trim();
                var nombre = parts[1].Trim();
                var telefono = parts[2].Trim();
                var fechaNac = DateOnly.Parse(parts[3]);

                var edad = CalcularEdad(fechaNac);

                AseguradoModel asegurado = null;
                asegurado = new AseguradoModel
                {
                    Cedula = cedula,
                    Nombre = nombre,
                    Telefono = telefono,
                    FechaNacimiento = fechaNac,
                    Edad = edad,
                    Seguros = []
                };
                
                asegurado.Seguros.Add(seguros
                .Where(s => (s.EdadMin == null || asegurado.Edad >= s.EdadMin) &&
                            (s.EdadMax == null || asegurado.Edad <= s.EdadMax))
                .OrderByDescending(s => s.Prima)
                .ThenBy(s => s.IdSeguro)
                .FirstOrDefault().IdSeguro);

                asegurados.Add(asegurado);
            }

            for (int i = 0; i < asegurados.Count; i++)
            {
                await aseguradoRepository.RegistrarAsegurado(asegurados[i]);
            }
        }

        private int CalcularEdad(DateOnly fechaNacimiento)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            int edad = today.Year - fechaNacimiento.Year;

            if (fechaNacimiento > today.AddYears(-edad))
                edad--;

            return edad;
        }

        private async Task<List<SeguroModel>> ObtenerSeguroPorEdad()
        {
            // Traemos todos los seguros activos desde tu SP
            var response = await segurosRepository.ConsultarSeguros(new ConsultaFiltrosModel
            {
                PaginaActual = 1,
                TamanioPagina = 1000 // un número grande para traer todos
            });

            var seguros = JsonSerializer
             .Deserialize<JsonElement>(JsonSerializer.Serialize(response.Datos))
             .GetProperty("seguros")
             .Deserialize<List<SeguroModel>>();


            if (seguros == null || !seguros.Any())
                throw new Exception("No hay seguros disponibles.");

            return seguros;
        }


    }
}
