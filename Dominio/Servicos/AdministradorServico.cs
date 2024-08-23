using System.Data.Common;
using MinimalApi.Dominio.DTO;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.Infraestrutura.Db;

namespace MinimalApi.Dominio.Servicos;

public class AdministradorServico : IAdministradorServico
{
  private readonly DbContexto _contexto;
  public AdministradorServico(DbContexto contexto)
  {
    _contexto = contexto;
  }

  public void Apagar(Administrador administrador)
  {
    _contexto.Administradores.Remove(administrador);
    _contexto.SaveChanges();
  }

  public void Atualizar(Administrador administrador)
  {
    _contexto.Administradores.Update(administrador);
    _contexto.SaveChanges();
  }

  public Administrador? BuscaPorId(int Id)
  {
    return _contexto.Administradores.Find(Id);
  }

  public void Incluir(Administrador administrador)
  {
    _contexto.Administradores.Add(administrador);
    _contexto.SaveChanges();
  }

  public Administrador? Login(LoginDTO loginDTO)
  {
    var adms = _contexto.Administradores.Where(adm => (adm.Email == loginDTO.Email && adm.Senha == loginDTO.Senha));
    return adms.FirstOrDefault();
  }

  public List<Administrador> Todos(int pagina = 1, int AdmPorPagina = 10)
  {
    var query = _contexto.Administradores.AsQueryable();


    query = query.Skip((pagina - 1) * AdmPorPagina).Take(AdmPorPagina);

    return query.ToList();
  }
}