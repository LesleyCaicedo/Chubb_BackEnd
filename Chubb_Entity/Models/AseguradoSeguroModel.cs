using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chubb_Entity.Models
{
    public class AseguradoSeguroModel
    {
        public int IdAsegurado { get; set; }
        public string Nombre { get; set; }
        public string Cedula { get; set; }
        public string Telefono { get; set; }
        public int Edad { get; set; }
        public string NombreSeguro { get; set; }
        public string CodigoSeguro { get; set; }
    }

    public class SeguroAseguradoModel : AseguradoSeguroModel
    {
        public int IdSeguro { get; set; }
        public decimal SumaAsegurada { get; set; }
        public decimal Prima { get; set; }
    }
}
