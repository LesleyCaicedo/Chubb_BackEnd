using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chubb_Entity.Models
{
    public class LoginResponseModel
    {
        public string Usuario { get; set; } = string.Empty;
        public string TokenDeAcceso { get; set; } = string.Empty;
        public int ExpiraEn { get; set; }
    }
}
