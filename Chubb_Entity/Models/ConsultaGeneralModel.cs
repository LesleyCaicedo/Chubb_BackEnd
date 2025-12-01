using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chubb_Entity.Models
{
    public class ConsultaGeneralModel
    {
        public int IdAsegurado { get; set; }
        public string Cedula { get; set; } = string.Empty;
        public string NombreAsegurado { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public DateTime FechaNacimiento { get; set; }
        public int Edad { get; set; }

        public int? IdSeguro { get; set; }
        public string NombreSeguro { get; set; } = string.Empty;
        public string CodigoSeguro { get; set; } = string.Empty;
        public decimal SumaAsegurada { get; set; }
        public decimal Prima { get; set; }
        public int? EdadMin { get; set; }
        public int? EdadMax { get; set; }
    }
}
