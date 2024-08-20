using MinimalApi.Dominio.DTO;
using MinimalApi.Dominio.Entidades;

namespace MinimalApi.Dominio.Interfaces;

public interface IAdministradorServico
{
  Administrador? Login(LoginDTO loginDTO);
}