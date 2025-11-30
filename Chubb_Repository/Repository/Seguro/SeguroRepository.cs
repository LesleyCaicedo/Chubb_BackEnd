using Chubb_Entity.Commons;
using Chubb_Entity.Models;
using Chubb_Entity.Utils;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
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
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            string sql = @"
            IF NOT EXISTS(SELECT 1 FROM Seguros WHERE Codigo = @Codigo)
            BEGIN
                INSERT INTO Seguros(Codigo, Nombre, SumaAsegurada, Prima, Eliminado, FechaCreacion)
                           VALUES(@Codigo, @Nombre, @SumaAsegurada, @Prima, 0 , @FechaCreacion);

                SELECT 1;
            END
            ELSE
            BEGIN
                SELECT 0;
            END";

            await using SqlCommand cmd = new SqlCommand(sql, connection);

            cmd.Parameters.AddWithValue("@Codigo", seguro.Codigo);
            cmd.Parameters.AddWithValue("@Nombre", seguro.Nombre);
            cmd.Parameters.AddWithValue("@SumaAsegurada", seguro.SumaAsegurada);
            cmd.Parameters.AddWithValue("@Prima", seguro.Prima);
            cmd.Parameters.AddWithValue("@FechaCreacion", TimeMethods.EC_Time());
            bool inserted = Convert.ToBoolean(await cmd.ExecuteScalarAsync());

            return new ResponseModel
            {
                Estado = inserted ? ResponseCode.Success : ResponseCode.Error,
                Mensaje = inserted ? "Seguro registrado exitosamente." : "Ya existe un seguro con el mismo código.",
                Datos = null
            };
        }

        public async Task<ResponseModel> ConsultarSeguros()
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            string sql = @"
            SELECT IdSeguro, Codigo, Nombre, SumaAsegurada, Prima FROM Seguros WHERE Eliminado = 0;"
            ;

            await using SqlCommand cmd = new SqlCommand(sql, connection);
            await using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            List<SeguroModel> seguros = new List<SeguroModel>();
            while (await reader.ReadAsync()) 
            {
                seguros.Add(new SeguroModel
                { 
                    IdSeguro = reader.GetInt32(0),
                    Codigo = reader.GetString(1),
                    Nombre = reader.GetString(2),
                    SumaAsegurada = reader.GetDecimal(3),
                    Prima = reader.GetDecimal(4)
                });
            }

            return new ResponseModel
            {
                Estado = ResponseCode.Success,
                Mensaje = "Seguros obtenidos exitosamente.",
                Datos = new { seguros }
            };
        }

        public async Task<ResponseModel> ActualizarSeguro(SeguroModel seguro)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            string sql = @"
            UPDATE Seguros SET Nombre = @Nombre,
                               Codigo = @Codigo,
                               SumaAsegurada = @SumaAsegurada,
                               Prima = @Prima,
                               FechaActualizacion = @FechaActualizacion
            WHERE IdSeguro = @IdSeguro";

            await using SqlCommand cmd = new SqlCommand(sql, connection);

            cmd.Parameters.AddWithValue("@IdSeguro", seguro.IdSeguro);
            cmd.Parameters.AddWithValue("@Nombre", seguro.Nombre);
            cmd.Parameters.AddWithValue("@Codigo", seguro.Codigo);
            cmd.Parameters.AddWithValue("@SumaAsegurada", seguro.SumaAsegurada);
            cmd.Parameters.AddWithValue("@Prima", seguro.Prima);
            cmd.Parameters.AddWithValue("@FechaActualizacion", TimeMethods.EC_Time());
            int rowsAffected = await cmd.ExecuteNonQueryAsync();

            if (rowsAffected > 0)
            {
                return new ResponseModel
                {
                    Estado = ResponseCode.Success,
                    Mensaje = "Seguro actualizado exitosamente.",
                    Datos = null
                };
            }
            else
            {
                return new ResponseModel
                {
                    Estado = ResponseCode.Error,
                    Mensaje = "No se pudo actualizar el Seguro. Verifique que el seguro exista.",
                    Datos = null
                };
            }
        }

        public async Task<ResponseModel> EliminarSeguro(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            string sql = @"
            UPDATE Seguros SET Eliminado = 1,
                               FechaEliminacion = @FechaEliminacion
            WHERE IdSeguro = @IdSeguro;";

            await using SqlCommand cmd = new SqlCommand(sql, connection);

            cmd.Parameters.AddWithValue("@IdSeguro", id);
            cmd.Parameters.AddWithValue("@FechaEliminacion", TimeMethods.EC_Time());
            int rowsAffected = await cmd.ExecuteNonQueryAsync();

            if (rowsAffected > 0)
            {
                return new ResponseModel
                {
                    Estado = ResponseCode.Success,
                    Mensaje = "Seguro eliminado exitosamente."
                };
            }
            else
            {
                return new ResponseModel
                {
                    Estado = ResponseCode.Error,
                    Mensaje = "No se pudo eliminar el Seguro."
                };
            }
        }

    }
}

