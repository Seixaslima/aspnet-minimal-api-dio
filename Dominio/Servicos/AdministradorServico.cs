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
  public Administrador? Login(LoginDTO loginDTO)
  {
    var adms = _contexto.Administradores.Where(adm => (adm.Email == loginDTO.Email && adm.Senha == loginDTO.Senha));
    return adms.FirstOrDefault();
  }
}