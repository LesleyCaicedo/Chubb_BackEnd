namespace Chubb_Entity.Models
{
    public class AseguradoModel
    {
        public int IdAsegurado { get; set; }
        public string Nombre { get; set; }  
        public string Cedula { get; set; }
        public string Telefono { get; set; }
        public int Edad { get; set; }
        public DateOnly FechaNacimiento { get; set; }
        public List<int> Seguros { get; set; }
        public bool Eliminado { get; set; }
        public string UsuarioGestor { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaActualizacion { get; set; }
        public DateTime FechaEliminacion { get; set; }

    }
}
