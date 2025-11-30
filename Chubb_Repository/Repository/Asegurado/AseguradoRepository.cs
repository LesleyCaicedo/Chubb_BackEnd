using Chubb_Entity.Commons;
using Chubb_Entity.Models;
using Chubb_Entity.Utils;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                Mensaje = inserted ? "Seguro registrado exitosamente." : "Ya existe un seguro con el mismo código.",
                Datos = null
            };
        }
    }
}
