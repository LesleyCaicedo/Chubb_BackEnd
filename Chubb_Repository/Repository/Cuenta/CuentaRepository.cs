using Chubb_Entity.Commons;
using Chubb_Entity.Models;
using Chubb_Entity.Utils;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace Chubb_Repository.Repository.Cuenta
{
    public class CuentaRepository : ICuentaRepository
    {
        private readonly string? _connectionString;

        public CuentaRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");
        }

        public async Task<ResponseModel> CreateAsync(UsuarioModel u)
        {
            string tmpClave = GenerarClaveTemporal();

            using SqlConnection cn = new(_connectionString);
            await cn.OpenAsync();

            await using SqlCommand cmd = new SqlCommand("RegistrarCuenta", cn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Nombre", u.Nombre);
            cmd.Parameters.AddWithValue("@Correo", u.Correo);
            cmd.Parameters.AddWithValue("@Usuario", u.Usuario);
            cmd.Parameters.AddWithValue("@Celular", u.Celular);
            cmd.Parameters.AddWithValue("@Cedula", u.Cedula);
            cmd.Parameters.AddWithValue("@IdRol", u.IdRol);
            cmd.Parameters.AddWithValue("@Clave", EncryptHelper.HashClave(tmpClave));
            cmd.Parameters.AddWithValue("@CreadoPor", u.UsuarioGestor);
            cmd.Parameters.AddWithValue("@FechaCreacion", TimeMethods.EC_Time());

            await using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                bool usuarioDuplicado = reader.GetBoolean(reader.GetOrdinal("UsuarioDuplicado"));
                bool correoDuplicado = reader.GetBoolean(reader.GetOrdinal("CorreoDuplicado"));

                if (usuarioDuplicado || correoDuplicado)
                {
                    string mensaje = usuarioDuplicado && correoDuplicado
                        ? "El usuario y el correo ya están registrados."
                        : usuarioDuplicado
                            ? "El usuario ya está registrado."
                            : "El correo ya está registrado.";

                    return new ResponseModel { Estado = ResponseCode.Error, Mensaje = mensaje };
                }

                return new ResponseModel
                {
                    Estado = ResponseCode.Success,
                    Mensaje = "Usuario registrado exitosamente",
                    Datos = new RegistroExitosoModel { Usuario = u.Usuario, ClaveTemporal = tmpClave }
                };
            }

            return new ResponseModel { Estado = ResponseCode.Error, Mensaje = "Error inesperado al registrar usuario." };
        }

        public async Task<ResponseModel> CambiarClaveAsync(CambioClaveModel model)
        {
            using SqlConnection cn = new(_connectionString);
            await cn.OpenAsync();

            await using SqlCommand cmd = new SqlCommand("ActualizarCuenta", cn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@opcion", "CambiarClave");
            cmd.Parameters.AddWithValue("@NuevaClave", EncryptHelper.HashClave(model.NuevaClave));
            cmd.Parameters.AddWithValue("@IdUsuario", model.IdUsuario);
            cmd.Parameters.AddWithValue("@ActualizadoPor", model.UsuarioModificador);
            cmd.Parameters.AddWithValue("@FechaActualizacion", TimeMethods.EC_Time());
            int rowsAffected = await cmd.ExecuteNonQueryAsync();

            if (rowsAffected > 0)
            {
                return new ResponseModel { Estado = ResponseCode.Success, Mensaje = "Clave cambiada exitosamente." };
            }
            else
            {
                return new ResponseModel { Estado = ResponseCode.Error, Mensaje = "No se pudo cambiar la clave. Verifique que el usuario exista y esté activo." };
            }
        }

        private string GenerarClaveTemporal()
        {
            Random random = new();
            int longitud = random.Next(5, 8);

            StringBuilder clave = new(longitud);
            for (int i = 0; i < longitud; i++)
            {
                int index = random.Next(Constants.caracteresPermitidos.Length);
                clave.Append(Constants.caracteresPermitidos[index]);
            }

            return clave.ToString();
        }
    }
}
