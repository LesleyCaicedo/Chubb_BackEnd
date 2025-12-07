using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chubb_Entity.Models
{
    public class ConsultaSegurosEdadModel
    {
        public int? EdadMin { get; set; }
        public int? EdadMax { get; set; }
        public bool IncluirGenerales { get; set; }
    }
}
