using MinimalApi.Dominio.DTO;
using MinimalApi.Dominio.Entidades;

namespace MinimalApi.Dominio.Interfaces;

public interface IAdministradorServico
{
  Administrador? Login(LoginDTO loginDTO);

  List<Administrador> Todos(int pagina = 1, int AdmPorPagina = 10);

  Administrador? BuscaPorId(int Id);
  void Incluir(Administrador administrador);
  void Atualizar(Administrador administrador);
  void Apagar(Administrador administrador);
}