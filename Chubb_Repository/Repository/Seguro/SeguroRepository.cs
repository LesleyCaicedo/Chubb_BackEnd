using Chubb_Entity.Commons;
using Chubb_Entity.Models;
using Chubb_Entity.Utils;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Reflection.PortableExecutable;

namespace Chubb_Repository.Repository.Seguro
{
    public class SeguroRepository : ISeguroRepository
    {
        private readonly string? _connectionString;

        public SeguroRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");
        }

        public async Task<ResponseModel> RegistrarSeguro(SeguroModel seguro)
        {
            using SqlConnection connection = new(_connectionString);
            await connection.OpenAsync();

            await using SqlCommand cmd = new("RegistrarSeguro", connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Codigo", seguro.Codigo);
            cmd.Parameters.AddWithValue("@Nombre", seguro.Nombre);
            cmd.Parameters.AddWithValue("@SumaAsegurada", seguro.SumaAsegurada);
            cmd.Parameters.AddWithValue("@Prima", seguro.Prima);
            AddNullableParameter(cmd, "@EdadMin", seguro.EdadMin);
            AddNullableParameter(cmd, "@EdadMax", seguro.EdadMax);
            cmd.Parameters.AddWithValue("@FechaCreacion", TimeMethods.EC_Time());
            bool inserted = Convert.ToBoolean(await cmd.ExecuteScalarAsync());

            return new ResponseModel
            {
                Estado = inserted ? ResponseCode.Success : ResponseCode.Error,
                Mensaje = inserted ? "Seguro registrado exitosamente." : "Ya existe un seguro con el mismo código.",
                Datos = null
            };
        }

        public async Task<ResponseModel> ConsultarSeguros(ConsultaFiltrosModel filtros)
        {
            using SqlConnection connection = new(_connectionString);
            await connection.OpenAsync();

            await using SqlCommand cmd = new("ConsultarSeguros", connection);
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
            List<SeguroModel> seguros = new List<SeguroModel>();
            while (await reader.ReadAsync()) 
            {
                seguros.Add(new SeguroModel
                { 
                    IdSeguro = Convert.ToInt32(reader["IdSeguro"]),
                    Codigo = reader["Codigo"].ToString()!,
                    Nombre = reader["Nombre"].ToString()!,
                    SumaAsegurada = Convert.ToDecimal(reader["SumaAsegurada"]),
                    Prima = Convert.ToDecimal(reader["Prima"]),
                    EdadMin = reader["EdadMin"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["EdadMin"]),
                    EdadMax = reader["EdadMax"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["EdadMax"]),
                });
            }

            return new ResponseModel
            {
                Estado = ResponseCode.Success,
                Mensaje = "Seguros obtenidos exitosamente.",
                Datos = new { seguros, registrosFiltrados, registrosTotales }
            };
        }

        public async Task<ResponseModel> ConsultarSeguroId(ConsultaFiltrosModel filtros, int id)
        {
            using SqlConnection connection = new(_connectionString);
            await connection.OpenAsync();

            await using SqlCommand cmd = new("ConsultarSeguros", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@paginaActual", filtros.PaginaActual);
            cmd.Parameters.AddWithValue("@tamanioPagina", filtros.TamanioPagina);
            await using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            List<SeguroAseguradoModel> seguros = new List<SeguroAseguradoModel>();
            while (await reader.ReadAsync()) 
            {
                seguros.Add(new SeguroAseguradoModel
                { 
                    IdSeguro = Convert.ToInt32(reader["IdSeguro"]),
                    CodigoSeguro = reader["CodigoSeguro"].ToString()!,
                    NombreSeguro = reader["NombreSeguro"].ToString()!,
                    SumaAsegurada = Convert.ToDecimal(reader["SumaAsegurada"]),
                    Prima = Convert.ToDecimal(reader["Prima"]),
                    EdadMin = reader["EdadMin"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["EdadMin"]),
                    EdadMax = reader["EdadMax"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["EdadMax"]),
                    IdAsegurado = Convert.ToInt32(reader["IdAsegurado"]),
                    Nombre = reader["NombreAsegurado"].ToString()!,
                    Cedula = reader["Cedula"].ToString()!,
                    Telefono = reader["Telefono"].ToString()!,
                    Edad = CalcularEdad(Convert.ToDateTime(reader["FechaNacimiento"])),
                });
            }

            return new ResponseModel
            {
                Estado = ResponseCode.Success,
                Mensaje = "Seguros obtenidos exitosamente.",
                Datos = new { seguros }
            };
        }

        private int CalcularEdad(DateTime fechaNacimiento)
        {
            DateTime hoy = DateTime.Today;
            int edad = hoy.Year - fechaNacimiento.Year;

            if (fechaNacimiento.Date > hoy.AddYears(-edad))
                edad--;

            return edad;
        }

        public async Task<ResponseModel> ActualizarSeguro(SeguroModel seguro)
        {
            using SqlConnection connection = new(_connectionString);
            await connection.OpenAsync();

            await using SqlCommand cmd = new("ActualizarSeguro", connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@IdSeguro", seguro.IdSeguro);
            cmd.Parameters.AddWithValue("@Nombre", seguro.Nombre);
            cmd.Parameters.AddWithValue("@Codigo", seguro.Codigo);
            cmd.Parameters.AddWithValue("@SumaAsegurada", seguro.SumaAsegurada);
            cmd.Parameters.AddWithValue("@Prima", seguro.Prima);
            AddNullableParameter(cmd, "@EdadMin", seguro.EdadMin);
            AddNullableParameter(cmd, "@EdadMax", seguro.EdadMax);
            cmd.Parameters.AddWithValue("@FechaActualizacion", TimeMethods.EC_Time());

            await using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                int resultado = reader.GetInt32(0);
                string mensaje = reader.GetString(1);

                return new ResponseModel
                {
                    Estado = resultado == 1 ? ResponseCode.Success : ResponseCode.Error,
                    Mensaje = mensaje
                };
            }

            return new ResponseModel
            {
                Estado = ResponseCode.Error,
                Mensaje = "Error inesperado al actualizar el seguro."
            };
        }

        public async Task<ResponseModel> EliminarSeguro(int id)
        {
            using SqlConnection connection = new(_connectionString);
            await connection.OpenAsync();

            await using SqlCommand cmd = new("EliminarSeguro", connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@IdSeguro", id);
            cmd.Parameters.AddWithValue("@FechaEliminacion", TimeMethods.EC_Time());
            await using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                int resultado = reader.GetInt32(0);
                string mensaje = reader.GetString(1);

                return new ResponseModel
                {
                    Estado = resultado == 1 ? ResponseCode.Success : ResponseCode.Error,
                    Mensaje = mensaje
                };
            }

            return new ResponseModel
            {
                Estado = ResponseCode.Error,
                Mensaje = "Error inesperado al eliminar el seguro."
            };
        }

        public async Task<ResponseModel> ConsultaGeneral(ConsultaFiltrosModel filtros, string cedula, string codigo)
        {
            using SqlConnection connection = new(_connectionString);
            await connection.OpenAsync();

            await using SqlCommand cmd = new("BuscarAseguradosOSeguros", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@termino", (object?)filtros.Termino ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@paginaActual", filtros.PaginaActual);
            cmd.Parameters.AddWithValue("@tamanioPagina", filtros.TamanioPagina);
            cmd.Parameters.AddWithValue("@Cedula", cedula);
            cmd.Parameters.AddWithValue("@CodigoSeguro", codigo);
            await using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            List<ConsultaGeneralModel> seguros = new List<ConsultaGeneralModel>();
            while (await reader.ReadAsync())
            {
                seguros.Add(new ConsultaGeneralModel
                {
                    IdSeguro = Convert.ToInt32(reader["IdSeguro"]),
                    CodigoSeguro = reader["CodigoSeguro"].ToString()!,
                    NombreSeguro = reader["NombreSeguro"].ToString()!,
                    SumaAsegurada = Convert.ToDecimal(reader["SumaAsegurada"]),
                    Prima = Convert.ToDecimal(reader["Prima"]),
                    EdadMin = reader["EdadMin"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["EdadMin"]),
                    EdadMax = reader["EdadMax"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["EdadMax"]),

                    IdAsegurado = Convert.ToInt32(reader["IdAsegurado"]),
                    NombreAsegurado = reader["NombreAsegurado"].ToString()!,
                    Cedula = reader["Cedula"].ToString()!,
                    Telefono = reader["Telefono"].ToString()!,
                    Edad = CalcularEdad(Convert.ToDateTime(reader["FechaNacimiento"])),
                    FechaNacimiento = Convert.ToDateTime(reader["FechaNacimiento"])
                });
            }

            return new ResponseModel
            {
                Estado = ResponseCode.Success,
                Mensaje = "Seguros obtenidos exitosamente.",
                Datos = new { seguros }
            };
        }

        private void AddNullableParameter(SqlCommand cmd, string paramName, int? value)
        {
            SqlParameter param = new(paramName, SqlDbType.Int);
            param.Value = value.HasValue ? value.Value : DBNull.Value;
            cmd.Parameters.Add(param);
        }
    }
}

