using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chubb_Entity.Models
{
    public class UsuarioModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = default!;
        public string Usuario { get; set; } = default!;
        public string Correo { get; set; } = string.Empty;
        public string Celular { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string Clave { get; set; } = default!;
        public int IdRol { get; set; }
        public string Rol { get; set; } = string.Empty;
        public string Permisos { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
        public string? UsuarioGestor { get; set; }
    }
}
