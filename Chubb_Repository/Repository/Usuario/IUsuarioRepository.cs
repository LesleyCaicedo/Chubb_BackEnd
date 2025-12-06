using Chubb_Entity.Models;

namespace Chubb_Repository.Repository.Usuario
{
    public interface IUsuarioRepository
    {
        Task<UsuarioModel> GetByUsuarioAsync(string usuario);
        Task<UsuarioModel?> GetByIdAsync(int idUsuario);
    }
}
