using System.Data.Common;
using MinimalApi.Dominio.DTO;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.Infraestrutura.Db;

namespace MinimalApi.Dominio.Servicos;

public class VeiculoServico : IVeiculoServico
{
  private readonly DbContexto _contexto;
  public VeiculoServico(DbContexto contexto)
  {
    _contexto = contexto;
  }

  public void Apagar(Veiculo veiculo)
  {
    _contexto.Veiculos.Remove(veiculo);
    _contexto.SaveChanges();
  }

  public void Atualizar(Veiculo veiculo)
  {
    _contexto.Veiculos.Update(veiculo);
    _contexto.SaveChanges();
  }

  public Veiculo? BuscaPorId(int Id)
  {
    return _contexto.Veiculos.Find(Id);
  }

  public void Incluir(Veiculo veiculo)
  {
    _contexto.Veiculos.Add(veiculo);
    _contexto.SaveChanges();
  }

  public List<Veiculo> Todos(int pagina = 1, string? nome = null, string? marca = null, int veiculosPorPagina = 10)
  {
    var query = _contexto.Veiculos.AsQueryable();

    if (!string.IsNullOrEmpty(nome))
    {
      query = query.Where(v => v.Nome.ToLower().Contains(nome.ToLower()));
    }

    if (!string.IsNullOrEmpty(marca))
    {
      query = query.Where(v => v.Marca.ToLower().Contains(marca.ToLower()));
    }

    query = query.Skip((pagina - 1) * veiculosPorPagina).Take(veiculosPorPagina);

    return query.ToList();
  }

}