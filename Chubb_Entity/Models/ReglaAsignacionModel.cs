using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chubb_Entity.Models
{
    public class ReglaAsignacionModel
    {
        public int IdSeguro { get; set; }
        public string NombreSeguro { get; set; }
        public decimal Prima { get; set; }
        public bool EsGeneral { get; set; }
        public int? EdadMinima { get; set; }
        public int? EdadMaxima { get; set; }
    }
}
