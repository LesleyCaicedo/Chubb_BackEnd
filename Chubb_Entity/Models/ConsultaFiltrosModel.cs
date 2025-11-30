using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chubb_Entity.Models
{
    public class ConsultaFiltrosModel
    {
        public string Termino { get; set; } = string.Empty;
        public int PaginaActual { get; set; }
        public int TamanioPagina { get; set; }
    }
}
