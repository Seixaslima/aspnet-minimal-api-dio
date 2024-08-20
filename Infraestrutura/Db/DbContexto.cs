using Microsoft.EntityFrameworkCore;
using MinimalApi.Dominio.Entidades;

namespace MinimalApi.Infraestrutura.Db;

public class DbContexto : DbContext
{
  private readonly IConfiguration _ConfiguracaoAppSettings;

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<Administrador>().HasData(
      new Administrador
      {
        Id = 1,
        Email = "administrador@teste.com",
        Senha = "123456",
        Perfil = "Adm"
      }
    );
  }
  public DbContexto(IConfiguration ConfiguracaoAppSettings)
  {
    _ConfiguracaoAppSettings = ConfiguracaoAppSettings;
  }
  public DbSet<Administrador> Administradores { get; set; } = default!;
  public DbSet<Veiculo> Veiculos { get; set; } = default!;

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