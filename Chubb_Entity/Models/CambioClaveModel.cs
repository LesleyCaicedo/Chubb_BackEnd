using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chubb_Entity.Models
{
    public class CambioClaveModel
    {
        public int IdUsuario { get; set; }
        public string ClaveActual { get; set; } = string.Empty;
        public string NuevaClave { get; set; } = string.Empty;
        public string UsuarioModificador { get; set; } = null!;
    }
}
