using Chubb_Entity.Commons;

namespace Chubb_Entity.Models
{
    public class ResponseModel
    {
        public ResponseCode Estado { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public object? Datos { get; set; }
    }
}
