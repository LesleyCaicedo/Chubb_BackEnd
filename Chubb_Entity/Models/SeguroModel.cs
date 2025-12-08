namespace Chubb_Entity.Models
{
    public class SeguroModel
    {
        public int IdSeguro { get; set; }
        public string Nombre { get; set; }
        public string Codigo { get; set; }
        public decimal SumaAsegurada { get; set; }
        public decimal Prima { get; set; }
        public int? EdadMin { get; set; }
        public int? EdadMax { get; set; }
        public bool Eliminado { get; set; }
        public string? UsuarioGestor { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaActualizacion { get; set; }
        public DateTime FechaEliminacion { get; set; }
    }
}
