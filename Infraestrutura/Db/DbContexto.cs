using Microsoft.EntityFrameworkCore;
using MinimalApi.Dominio.Entidades;

namespace MinimalApi.Infraestrutura.Db;

public class DbContexto : DbContext
{
  private readonly IConfiguration _ConfiguracaoAppSettings;

  public DbContexto(IConfiguration ConfiguracaoAppSettings)
  {
    _ConfiguracaoAppSettings = ConfiguracaoAppSettings;
  }
  public DbSet<Administrador> Administradores { get; set; } = default!;

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    if (!optionsBuilder.IsConfigured)
    {
      var stringDeConexao = _ConfiguracaoAppSettings.GetConnectionString("mysql")?.ToString();
      if (!string.IsNullOrEmpty(stringDeConexao))
      {
        optionsBuilder.UseMySql(
          stringDeConexao,
          ServerVersion.AutoDetect(stringDeConexao)
        );

      }
    }


  }
}