using minimal_api.Domínio.Interfaces;
using MinimalApi.Dominio.Entidade;
using MinimalApi.DTOs;

namespace Test.Mocks;

public class AdministradorServicoMock : IAdministradorServico
{
    private static List<Administrador> administradores = new List<Administrador>()
    {
        new Administrador{
            Id = 1,
            Email = "administrador@teste.com",
            Senha = "123456",
            Perfil = "Adm"
        },

        new Administrador{
            Id = 2,
            Email = "van@gmail.com",
            Senha = "editor",
            Perfil = "Editor"
        }
    };
    public Administrador? BuscaPorId(int id)
    {
        return administradores.Find(a => a.Id == id);
    }

    public Administrador Incluir(Administrador administrador)
    {
        administrador.Id = administradores.Count() + 1;
        administradores.Add(administrador);

        return administrador;
    }

    public Administrador? Login(LoginDTO loginDTO)
    {
        return administradores.Find(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha );
    }

    public List<Administrador> Todos(int? pagina)
    {
        return administradores;
    }
}
