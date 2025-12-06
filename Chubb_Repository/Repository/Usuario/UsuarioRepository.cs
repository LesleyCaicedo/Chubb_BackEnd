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
            const string sql = @"
                  SELECT TOP 1 u.Id, u.Nombre, u.Usuario, u.Correo, u.Celular, u.Clave, u.IdRol, r.Nombre as Rol, r.Permisos, u.Activo FROM Usuario u
                  JOIN Rol r ON u.IdRol = r.Id WHERE Usuario = @Usuario;";

            using SqlConnection cn = new(_connectionString);
            await cn.OpenAsync();

            await using SqlCommand cmd = new SqlCommand(sql, cn);
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
            const string sql = @"SELECT TOP 1 Id, Nombre, Usuario, Correo, Celular, Clave, IdRol, Activo FROM Usuario WHERE Id = @IdUsuario;";
            
            using SqlConnection cn = new(_connectionString);
            await cn.OpenAsync();

            await using SqlCommand cmd = new SqlCommand(sql, cn);
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
