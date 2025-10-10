using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using minimal_api.Domínio.Interfaces;
using MinimalApi.Dominio.Entidade;
using MinimalApi.DTOs;
using MinimalApi.Infraestrutura.Db;

namespace minimal_api.Domínio.Serviços;

public class AdministradorServico : IAdministradorServico
{
    private readonly DbContexto _contexto;
    public AdministradorServico(DbContexto contexto)
    {
        _contexto = contexto;
    }

    public Administrador? BuscaPorId(int id)
    {
        return _contexto.Administradores.Where(adm => adm.Id == id).FirstOrDefault();
    }

    public Administrador Incluir(Administrador administrador)
    {
        _contexto.Administradores.Add(administrador);
        _contexto.SaveChanges();

        return administrador;
    }

    public Administrador? Login(LoginDTO loginDTO)
    {
        var adm = _contexto.Administradores.FirstOrDefault(a => a.Email == loginDTO.Email
        && a.Senha == loginDTO.Senha);
        return adm;
    }

    public List<Administrador> Todos(int? pagina)
    {
        var query = _contexto.Administradores.AsQueryable();

        int itensPorPagina = 10;

        if (pagina != null)
            query = query.Skip((int)((pagina - 1) * itensPorPagina)).Take(itensPorPagina);
        
        return query.ToList();
    }
}
