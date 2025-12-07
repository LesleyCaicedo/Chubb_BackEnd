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
        public async Task<ResponseModel> EliminarAsegurado(int idAsegurado, string usuarioGestor)
        {
            return await aseguradoRepository.EliminarAsegurado(idAsegurado, usuarioGestor);
        }

        public async Task ProcesarExcelAsync(Stream stream, string usuarioGestor)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            List<AseguradoModel> asegurados = new List<AseguradoModel>();

            // Obtener seguros para asignación
            List<SeguroModel> segurosParaAsignar;

            if (reglas != null && reglas.Any())
            {
                // MODO PARAMETRIZADO: Usar solo las reglas configuradas por el usuario
                segurosParaAsignar = reglas.Select(r => new SeguroModel
                {
                    Cedula = cedula,
                    Nombre = nombre,
                    Telefono = telefono,
                    FechaNacimiento = fechaNac,
                    Edad = edad,
                    Seguros = [],
                    UsuarioGestor = usuarioGestor
                };

                segurosParaAsignar = JsonSerializer
                    .Deserialize<JsonElement>(JsonSerializer.Serialize(response.Datos))
                    .GetProperty("seguros")
                    .Deserialize<List<SeguroModel>>() ?? new List<SeguroModel>();
            }

            if (!segurosParaAsignar.Any())
                throw new Exception("No hay seguros disponibles para asignar.");

            // Leer archivo Excel
            using ExcelPackage package = new(stream);
            ExcelWorksheet sheet = package.Workbook.Worksheets[0];
            int rowCount = sheet.Dimension.Rows;

            // Procesar cada fila
            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    string cedula = sheet.Cells[row, 1].Text.Trim();
                    string nombre = sheet.Cells[row, 2].Text.Trim();
                    string telefono = sheet.Cells[row, 3].Text.Trim();
                    DateOnly fechaNac = DateOnly.Parse(sheet.Cells[row, 4].Text);
                    int edad = CalcularEdad(fechaNac);

                    AseguradoModel asegurado = new AseguradoModel
                    {
                        Cedula = cedula,
                        Nombre = nombre,
                        Telefono = telefono,
                        FechaNacimiento = fechaNac,
                        Edad = edad,
                        Seguros = []
                    };

                    // Asignar el seguro que mejor se ajuste a la edad
                    var seguroAsignado = segurosParaAsignar
                        .Where(s => (s.EdadMin == null || asegurado.Edad >= s.EdadMin) &&
                                    (s.EdadMax == null || asegurado.Edad <= s.EdadMax))
                        .OrderByDescending(s => s.Prima)  // Mayor prima primero
                        .ThenBy(s => s.IdSeguro)          // Desempate por ID
                        .FirstOrDefault();

                    if (seguroAsignado != null)
                    {
                        asegurado.Seguros.Add(seguroAsignado.IdSeguro);
                    }
                    else
                    {
                        // Log: asegurado sin seguro compatible
                        Console.WriteLine($"Advertencia: No se encontró seguro para {nombre} (edad {edad})");
                    }

                    asegurados.Add(asegurado);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error en fila {row}: {ex.Message}");
                }
            }

            // Registrar todos los asegurados
            foreach (var asegurado in asegurados)
            {
                await aseguradoRepository.RegistrarAsegurado(asegurado);
            }
        }

        public async Task ProcesarTxtAsync(Stream stream, string usuarioGestor)
        {
            List<AseguradoModel> asegurados = new List<AseguradoModel>();

            // Obtener seguros para asignación
            List<SeguroModel> segurosParaAsignar;

            if (reglas != null && reglas.Any())
            {
                segurosParaAsignar = reglas.Select(r => new SeguroModel
                {
                    Cedula = cedula,
                    Nombre = nombre,
                    Telefono = telefono,
                    FechaNacimiento = fechaNac,
                    Edad = edad,
                    Seguros = [],
                    UsuarioGestor = usuarioGestor
                };
                
                asegurado.Seguros.Add(seguros
                .Where(s => (s.EdadMin == null || asegurado.Edad >= s.EdadMin) &&
                            (s.EdadMax == null || asegurado.Edad <= s.EdadMax))
                .OrderByDescending(s => s.Prima)
                .ThenBy(s => s.IdSeguro)
                .FirstOrDefault().IdSeguro);

                segurosParaAsignar = JsonSerializer
                    .Deserialize<JsonElement>(JsonSerializer.Serialize(response.Datos))
                    .GetProperty("seguros")
                    .Deserialize<List<SeguroModel>>() ?? new List<SeguroModel>();
            }

            if (!segurosParaAsignar.Any())
                throw new Exception("No hay seguros disponibles para asignar.");

            using StreamReader reader = new(stream);
            await reader.ReadLineAsync(); // Saltar encabezado

            int lineNumber = 1;
            while (!reader.EndOfStream)
            {
                lineNumber++;
                try
                {
                    string? line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    string[] parts = line.Split('\t');
                    if (parts.Length < 4)
                        throw new Exception($"Línea incompleta (se esperan 4 columnas)");

                    string cedula = parts[0].Trim();
                    string nombre = parts[1].Trim();
                    string telefono = parts[2].Trim();
                    DateOnly fechaNac = DateOnly.Parse(parts[3].Trim());
                    int edad = CalcularEdad(fechaNac);

                    AseguradoModel asegurado = new AseguradoModel
                    {
                        Cedula = cedula,
                        Nombre = nombre,
                        Telefono = telefono,
                        FechaNacimiento = fechaNac,
                        Edad = edad,
                        Seguros = []
                    };

                    var seguroAsignado = segurosParaAsignar
                        .Where(s => (s.EdadMin == null || asegurado.Edad >= s.EdadMin) &&
                                    (s.EdadMax == null || asegurado.Edad <= s.EdadMax))
                        .OrderByDescending(s => s.Prima)
                        .ThenBy(s => s.IdSeguro)
                        .FirstOrDefault();

                    if (seguroAsignado != null)
                    {
                        asegurado.Seguros.Add(seguroAsignado.IdSeguro);
                    }

                    asegurados.Add(asegurado);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error en línea {lineNumber}: {ex.Message}");
                }
            }

            foreach (var asegurado in asegurados)
            {
                await aseguradoRepository.RegistrarAsegurado(asegurado);
            }
        }

        private int CalcularEdad(DateOnly fechaNacimiento)
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            int edad = today.Year - fechaNacimiento.Year;

            if (fechaNacimiento > today.AddYears(-edad))
                edad--;

            return edad;
        }
    }
}
