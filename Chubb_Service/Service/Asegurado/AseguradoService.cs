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

        public async Task ProcesarExcelAsync(Stream stream, string usuarioGestor, List<ReglaAsignacionModel>? reglas = null)
        {
            ExcelPackage.License.SetNonCommercialPersonal("Keti");
            List<AseguradoModel> asegurados = new List<AseguradoModel>();

            if (reglas != null && !reglas.Any())
                throw new Exception("Debe configurar al menos una regla de asignación.");

            using ExcelPackage package = new(stream);
            ExcelWorksheet sheet = package.Workbook.Worksheets[0];
            int rowCount = sheet.Dimension.Rows;

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
                        Seguros = [],
                        UsuarioGestor = usuarioGestor
                    };

                    if (reglas != null && reglas.Any())
                    {
                        // ===== MODO PARAMETRIZADO =====
                        List<int> segurosAplicables = reglas
                            .Where(r =>
                            {
                                if (!r.EdadMinima.HasValue && !r.EdadMaxima.HasValue)
                                return true;

                                bool cumpleMin = !r.EdadMinima.HasValue || edad >= r.EdadMinima.Value;
                                bool cumpleMax = !r.EdadMaxima.HasValue || edad <= r.EdadMaxima.Value;

                                return cumpleMin && cumpleMax;
                            })
                            .Select(r => r.IdSeguro)
                            .Distinct()
                            .ToList();

                        asegurado.Seguros.AddRange(segurosAplicables);
                    }
                    else
                    {
                        // ===== MODO AUTOMÁTICO =====
                        ResponseModel response = await segurosRepository.ConsultarSeguros(new ConsultaFiltrosModel
                        {
                            PaginaActual = 1,
                            TamanioPagina = 1000
                        });

                        List<SeguroModel> segurosDisponibles = JsonSerializer
                            .Deserialize<JsonElement>(JsonSerializer.Serialize(response.Datos))
                            .GetProperty("seguros")
                            .Deserialize<List<SeguroModel>>() ?? new List<SeguroModel>();

                        SeguroModel? seguroAsignado = segurosDisponibles
                            .Where(s => (s.EdadMin == null || edad >= s.EdadMin) &&
                                        (s.EdadMax == null || edad <= s.EdadMax))
                            .OrderByDescending(s => s.Prima)
                            .ThenBy(s => s.IdSeguro)
                            .FirstOrDefault();

                        if (seguroAsignado != null)
                        {
                            asegurado.Seguros.Add(seguroAsignado.IdSeguro);
                        }
                    }

                    asegurados.Add(asegurado);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error en fila {row}: {ex.Message}");
                }
            }

            for (int i = 0; i < asegurados.Count; i++)
            {
                await aseguradoRepository.RegistrarAsegurado(asegurados[i]);
            }
        }

        public async Task ProcesarTxtAsync(Stream stream, string usuarioGestor, List<ReglaAsignacionModel>? reglas = null)
        {
            List<AseguradoModel> asegurados = new List<AseguradoModel>();

            if (reglas != null && !reglas.Any())
                throw new Exception("Debe configurar al menos una regla de asignación.");

            using StreamReader reader = new(stream);
            await reader.ReadLineAsync();

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
                        Seguros = [],
                        UsuarioGestor = usuarioGestor
                    };

                    if (reglas != null && reglas.Any())
                    {
                        // MODO PARAMETRIZADO: Usar rangos de la regla
                        List<int> segurosAplicables = reglas
                            .Where(r =>
                            {
                                if (r.EsGeneral)
                                    return true;

                                bool cumpleMin = !r.EdadMinima.HasValue || edad >= r.EdadMinima.Value;
                                bool cumpleMax = !r.EdadMaxima.HasValue || edad <= r.EdadMaxima.Value;

                                return cumpleMin && cumpleMax;
                            })
                            .Select(r => r.IdSeguro)
                            .Distinct()
                            .ToList();

                        asegurado.Seguros.AddRange(segurosAplicables);
                    }
                    else
                    {
                        // MODO AUTOMÁTICO: Usar rangos del seguro
                        ResponseModel response = await segurosRepository.ConsultarSeguros(new ConsultaFiltrosModel
                        {
                            PaginaActual = 1,
                            TamanioPagina = 1000
                        });

                        List<SeguroModel> segurosDisponibles = JsonSerializer
                            .Deserialize<JsonElement>(JsonSerializer.Serialize(response.Datos))
                            .GetProperty("seguros")
                            .Deserialize<List<SeguroModel>>() ?? new List<SeguroModel>();

                        SeguroModel? seguroAsignado = segurosDisponibles
                            .Where(s => (s.EdadMin == null || edad >= s.EdadMin) &&
                                        (s.EdadMax == null || edad <= s.EdadMax))
                            .OrderByDescending(s => s.Prima)
                            .ThenBy(s => s.IdSeguro)
                            .FirstOrDefault();

                        if (seguroAsignado != null)
                        {
                            asegurado.Seguros.Add(seguroAsignado.IdSeguro);
                        }
                    }

                    asegurados.Add(asegurado);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error en línea {lineNumber}: {ex.Message}");
                }
            }

            for (int i = 0; i < asegurados.Count; i++)
            {
                await aseguradoRepository.RegistrarAsegurado(asegurados[i]);
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
