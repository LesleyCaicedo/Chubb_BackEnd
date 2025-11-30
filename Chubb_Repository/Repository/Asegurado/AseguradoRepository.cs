using Chubb_Entity.Commons;
using Chubb_Entity.Models;
using Chubb_Entity.Utils;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;


namespace Chubb_Repository.Repository.Asegurado
{
    public class AseguradoRepository : IAseguradoRepository
    {
        private readonly string? _connectionString;

        public AseguradoRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");
        }

        public async Task<ResponseModel> RegistrarAsegurado(AseguradoModel asegurado)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var cmd = new SqlCommand("RegistrarAseguradoConSeguro", connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Nombre", asegurado.Nombre);
            cmd.Parameters.AddWithValue("@Cedula", asegurado.Cedula);
            cmd.Parameters.AddWithValue("@Telefono", asegurado.Telefono);
            cmd.Parameters.AddWithValue("@Eliminado", asegurado.Eliminado);
            cmd.Parameters.AddWithValue("@FechaNacimiento", asegurado.FechaNacimiento);
            cmd.Parameters.AddWithValue("@Seguros", string.Join(",", asegurado.Seguros));
            cmd.Parameters.AddWithValue("@FechaCreacion", TimeMethods.EC_Time());
            bool inserted = Convert.ToBoolean(await cmd.ExecuteScalarAsync());

            return new ResponseModel
            {
                Estado = inserted ? ResponseCode.Success : ResponseCode.Error,
                Mensaje = inserted ? "Asegurado actualizado exitosamente." : "Cedula ya registrada.",
                Datos = null
            };
        }

        public async Task<ResponseModel> ConsultarAsegurados(ConsultaFiltrosModel filtros) 
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var cmd = new SqlCommand("ConsultarAsegurados", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@termino", (object?)filtros.Termino ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@paginaActual", filtros.PaginaActual);
            cmd.Parameters.AddWithValue("@tamanioPagina", filtros.TamanioPagina);
            await using SqlDataReader reader = await cmd.ExecuteReaderAsync();


            // Primer resultado: Total
            int registrosTotales = 0;
            if (await reader.ReadAsync())
                registrosTotales = reader.GetInt32(0);

            await reader.NextResultAsync();

            // Segundo resultado: Filtrados
            int registrosFiltrados = 0;
            if (await reader.ReadAsync())
                registrosFiltrados = reader.GetInt32(0);

            await reader.NextResultAsync();

            // Tercer resultado: Datos de clientes
            List<AseguradoModel> asegurados = new List<AseguradoModel>();
            while (await reader.ReadAsync())
            {
                asegurados.Add(new AseguradoModel
                {
                    IdAsegurado = Convert.ToInt32(reader["IdAsegurado"]),
                    Nombre = reader["Nombre"].ToString()!,
                    Cedula = reader["Cedula"].ToString()!,
                    Telefono = reader["Telefono"].ToString()!,
                    Edad = CalcularEdad(Convert.ToDateTime(reader["FechaNacimiento"])),
                    FechaNacimiento = DateOnly.FromDateTime(Convert.ToDateTime(reader["FechaNacimiento"]))
                });
            }

            return new ResponseModel
            {
                Estado = ResponseCode.Success,
                Mensaje = "Asegurados obtenidos exitosamente.",
                Datos = new { asegurados, registrosFiltrados, registrosTotales }
            };
        }

        public async Task<ResponseModel> ConsultarAseguradoId(ConsultaFiltrosModel filtros, int id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var cmd = new SqlCommand("ConsultarAsegurados", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@paginaActual", filtros.PaginaActual);
            cmd.Parameters.AddWithValue("@tamanioPagina", filtros.TamanioPagina);
            cmd.Parameters.AddWithValue("@Id", id);
            await using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            List<AseguradoSeguroModel> asegurado = new List<AseguradoSeguroModel>();
            while (await reader.ReadAsync())
            {
                asegurado.Add(new AseguradoSeguroModel
                {
                    IdAsegurado = Convert.ToInt32(reader["IdAsegurado"]),
                    Nombre = reader["Nombre"].ToString()!,
                    Cedula = reader["Cedula"].ToString()!,
                    Telefono = reader["Telefono"].ToString()!,
                    Edad = CalcularEdad(Convert.ToDateTime(reader["FechaNacimiento"])),
                    NombreSeguro = reader["NombreSeguro"].ToString()!,
                    CodigoSeguro = reader["CodigoSeguro"].ToString()!,
                });
            }

            return new ResponseModel
            {
                Estado = ResponseCode.Success,
                Mensaje = "Asegurado obtenidos exitosamente.",
                Datos = new { asegurado }
            };
        }

        private int CalcularEdad(DateTime fechaNacimiento)
        {
            var hoy = DateTime.Today;
            int edad = hoy.Year - fechaNacimiento.Year;

            if (fechaNacimiento.Date > hoy.AddYears(-edad))
                edad--;

            return edad;
        }

        public async Task<ResponseModel> ActualizarAsegurado(AseguradoModel asegurado)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var cmd = new SqlCommand("ActualizarAsegurado", connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@IdAsegurado", asegurado.IdAsegurado);
            cmd.Parameters.AddWithValue("@Nombre", asegurado.Nombre);
            cmd.Parameters.AddWithValue("@Cedula", asegurado.Cedula);
            cmd.Parameters.AddWithValue("@Telefono", asegurado.Telefono);
            cmd.Parameters.AddWithValue("@FechaNacimiento", asegurado.FechaNacimiento);
            cmd.Parameters.AddWithValue("@Seguros", string.Join(",", asegurado.Seguros));
            cmd.Parameters.AddWithValue("@FechaActualizacion", TimeMethods.EC_Time());
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                int resultado = reader.GetInt32(reader.GetOrdinal("Resultado"));

                if (resultado == 1)
                {
                    return new ResponseModel
                    {
                        Estado = ResponseCode.Success,
                        Mensaje = reader["Mensaje"].ToString()
                    };
                }
                else
                {
                    return new ResponseModel
                    {
                        Estado = ResponseCode.Error,
                        Mensaje = reader["Mensaje"].ToString()
                    };
                }
            }

            return new ResponseModel
            {
                Estado = ResponseCode.Error,
                Mensaje = "Error inesperado al procesar la respuesta del servidor."
            };
        }

        public async Task<ResponseModel> EliminarAsegurado(int idAsegurado)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var cmd = new SqlCommand("EliminarAsegurado", connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@IdAsegurado", idAsegurado);
            cmd.Parameters.AddWithValue("@FechaEliminacion", TimeMethods.EC_Time());

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                int resultado = reader.GetInt32(reader.GetOrdinal("Resultado"));

                if (resultado == 1)
                {
                    return new ResponseModel
                    {
                        Estado = ResponseCode.Success,
                        Mensaje = reader["Mensaje"].ToString()
                    };
                }
                else
                {
                    return new ResponseModel
                    {
                        Estado = ResponseCode.Error,
                        Mensaje = reader["Mensaje"].ToString()
                    };
                }
            }

            return new ResponseModel
            {
                Estado = ResponseCode.Error,
                Mensaje = "Error inesperado al procesar la respuesta del servidor."
            };
        }

    }
}
