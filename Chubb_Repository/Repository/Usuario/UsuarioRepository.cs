using Chubb_Entity.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Chubb_Repository.Repository.Usuario
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly string? _connectionString;

        public UsuarioRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");
        }

        public async Task<UsuarioModel> GetByUsuarioAsync(string usuario)
        {
            using SqlConnection cn = new(_connectionString);
            await cn.OpenAsync();

            await using SqlCommand cmd = new SqlCommand("ObtenerUsuario", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@opcion", "ObtenerPorUsuario");
            cmd.Parameters.AddWithValue("@Usuario", usuario);
            await using SqlDataReader rdr = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
            if (!await rdr.ReadAsync()) return null;

            return new UsuarioModel
            {
                Id = rdr.GetInt32(rdr.GetOrdinal("Id")),
                Nombre = rdr.GetString(rdr.GetOrdinal("Nombre")),
                Usuario = rdr.GetString(rdr.GetOrdinal("Usuario")),
                Correo = rdr.IsDBNull(rdr.GetOrdinal("Correo")) ? string.Empty : rdr.GetString(rdr.GetOrdinal("Correo")),
                Celular = rdr.IsDBNull(rdr.GetOrdinal("Celular")) ? string.Empty : rdr.GetString(rdr.GetOrdinal("Celular")),
                Clave = rdr.GetString(rdr.GetOrdinal("Clave")),
                IdRol = rdr.GetInt32(rdr.GetOrdinal("IdRol")),
                Rol = rdr.GetString(rdr.GetOrdinal("Rol")),
                Permisos = Convert.IsDBNull(rdr.GetOrdinal("Permisos")) ? rdr.GetString(rdr.GetOrdinal("Permisos")) : string.Empty,
                Activo = rdr.GetBoolean(rdr.GetOrdinal("Activo"))
            };
        }

        public async Task<UsuarioModel?> GetByIdAsync(int idUsuario)
        {
            using SqlConnection cn = new(_connectionString);
            await cn.OpenAsync();

            await using SqlCommand cmd = new SqlCommand("ObtenerUsuario", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@opcion", "ObtenerPorId");
            cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
            await using SqlDataReader rdr = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
            if (!await rdr.ReadAsync()) return null;


            return new UsuarioModel
            {
                Id = rdr.GetInt32(rdr.GetOrdinal("Id")),
                Nombre = rdr.GetString(rdr.GetOrdinal("Nombre")),
                Usuario = rdr.GetString(rdr.GetOrdinal("Usuario")),
                Correo = rdr.IsDBNull(rdr.GetOrdinal("Correo")) ? string.Empty : rdr.GetString(rdr.GetOrdinal("Correo")),
                Celular = rdr.IsDBNull(rdr.GetOrdinal("Celular")) ? string.Empty : rdr.GetString(rdr.GetOrdinal("Celular")),
                Clave = rdr.GetString(rdr.GetOrdinal("Clave")),
                IdRol = rdr.GetInt32(rdr.GetOrdinal("IdRol")),
                Activo = rdr.GetBoolean(rdr.GetOrdinal("Activo"))
            };
        }
    }
}
