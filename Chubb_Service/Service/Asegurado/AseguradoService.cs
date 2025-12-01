using Chubb_Entity.Models;
using Chubb_Repository.Repository.Asegurado;
using Chubb_Repository.Repository.Seguro;
using OfficeOpenXml;

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

        public async Task<List<AseguradoModel>> ProcesarArchivoAsync(byte[] fileBytes, string fileName)
        {
            if (fileBytes == null || fileBytes.Length == 0)
                throw new Exception("Archivo vacío.");

            var extension = Path.GetExtension(fileName).ToLower();

            using var stream = new MemoryStream(fileBytes);

            return extension switch
            {
                ".xlsx" => await ProcesarExcelAsync(stream),
                ".txt" => await ProcesarTxtAsync(stream),
                _ => throw new NotSupportedException("Formato no permitido.")
            };
        }

        private async Task<List<AseguradoModel>> ProcesarExcelAsync(Stream stream)
        {
            var asegurados = new List<AseguradoModel>();

            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using var package = new ExcelPackage(stream);
            var sheet = package.Workbook.Worksheets[0];

            var rowCount = sheet.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++)
            {
                var cedula = sheet.Cells[row, 1].Text.Trim();
                var nombre = sheet.Cells[row, 2].Text.Trim();
                var telefono = sheet.Cells[row, 3].Text.Trim();
                var fechaNac = DateOnly.Parse(sheet.Cells[row, 4].Text);

                var edad = CalcularEdad(Convert.ToDateTime(fechaNac));

                var idSeguro = await ObtenerSeguroPorEdad(edad);

                asegurados.Add(new AseguradoModel
                {
                    Cedula = cedula,
                    Nombre = nombre,
                    Telefono = telefono,
                    FechaNacimiento = fechaNac,
                    Edad = edad,
                    Seguros = new List<int> { idSeguro }
                });
            }

            return asegurados;
        }

        private async Task<List<AseguradoModel>> ProcesarTxtAsync(Stream stream)
        {
            var asegurados = new List<AseguradoModel>();

            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;

                // Cedula|Nombre|Telefono|FechaNacimiento   (ejemplo)
                var parts = line.Split('|');
                if (parts.Length < 4) continue;

                var cedula = parts[0].Trim();
                var nombre = parts[1].Trim();
                var telefono = parts[2].Trim();
                var fechaNac = DateOnly.Parse(parts[3]);

                var edad = CalcularEdad(Convert.ToDateTime(fechaNac));

                var idSeguro = await ObtenerSeguroPorEdad(edad);

                asegurados.Add(new AseguradoModel
                {
                    Cedula = cedula,
                    Nombre = nombre,
                    Telefono = telefono,
                    FechaNacimiento = fechaNac,
                    Edad = edad,
                    Seguros = new List<int> { idSeguro }
                });
            }

            return asegurados;
        }

        private int CalcularEdad(DateTime fechaNacimiento)
        {
            var hoy = DateTime.Today;
            var edad = hoy.Year - fechaNacimiento.Year;

            if (fechaNacimiento.Date > hoy.AddYears(-edad)) edad--;

            return edad;
        }

        private async Task<int> ObtenerSeguroPorEdad(int edad)
        {
            // Traemos todos los seguros activos desde tu SP
            var response = await segurosRepository.ConsultarSeguros(new ConsultaFiltrosModel
            {
                PaginaActual = 1,
                TamanioPagina = 1000 // un número grande para traer todos
            });

            var seguros = ((dynamic)response.Datos).seguros as List<SeguroModel>;

            if (seguros == null || !seguros.Any())
                throw new Exception("No hay seguros disponibles.");

            // Filtramos por edad considerando NULL como sin límite
            var mejor = seguros
                .Where(s => (s.EdadMin == null || edad >= s.EdadMin) &&
                            (s.EdadMax == null || edad <= s.EdadMax))
                .OrderByDescending(s => s.Prima)
                .ThenBy(s => s.IdSeguro)
                .FirstOrDefault();

            if (mejor == null)
                throw new Exception($"No existe seguro válido para la edad {edad}");

            return mejor.IdSeguro;
        }


    }
}
