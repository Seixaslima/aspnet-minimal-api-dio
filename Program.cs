using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalApi.Dominio.DTO;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.Dominio.ModelViews;
using MinimalApi.Dominio.Servicos;
using MinimalApi.Infraestrutura.Db;

#region Builder
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAdministradorServico, AdministradorServico>();
builder.Services.AddScoped<IVeiculoServico, VeiculoServico>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DbContexto>(options =>
{
  options.UseMySql(
    builder.Configuration.GetConnectionString("mysql"),
    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("mysql"))
  );
});

var app = builder.Build();
#endregion


#region Home
app.MapGet("/", () => Results.Json(new Home())).WithTags("Home");
#endregion

#region Administradores
ErrosDeValidacao ValidaAdministradorDTO(AdministradorDTO administradorDTO)
{
  var validacao = new ErrosDeValidacao
  {
    Mensagens = new List<string>()
  };

  if (string.IsNullOrEmpty(administradorDTO.Email))
    validacao.Mensagens.Add("O Email não pode ficar em branco");

  if (string.IsNullOrEmpty(administradorDTO.Senha))
    validacao.Mensagens.Add("A Senha não pode ficar em branco");

  if (string.IsNullOrEmpty(administradorDTO.Perfil))
    validacao.Mensagens.Add("O Perfil não pode ficar em branco");
  else
  {
    if (!(administradorDTO.Perfil == "Adm" || administradorDTO.Perfil == "Editor"))
      validacao.Mensagens.Add("O Perfil tem que ser Adm ou Editor");
  }

  return validacao;
}

AdministradorModelView NormalizaAdmParaApresentacao(Administrador administrador)
{
  var adm = new AdministradorModelView
  {
    Id = administrador.Id,
    Email = administrador.Email,
    Perfil = administrador.Perfil
  };

  return adm;
}

app.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) =>
{
  if (administradorServico.Login(loginDTO) != null)
    return Results.Ok("Login com sucesso");
  else
    return Results.Unauthorized();
}).WithTags("Administradores");

app.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, IAdministradorServico administradorServico) =>
{

  var validacao = ValidaAdministradorDTO(administradorDTO);

  if (validacao.Mensagens.Count > 0)
    return Results.BadRequest(validacao);

  var administrador = new Administrador
  {
    Email = administradorDTO.Email,
    Senha = administradorDTO.Senha,
    Perfil = administradorDTO.Perfil
  };
  administradorServico.Incluir(administrador);

  return Results.Created($"/administradores/{administrador.Id}", NormalizaAdmParaApresentacao(administrador));
}).WithTags("Administradores");

app.MapGet("/administradores", ([FromQuery] int? pagina, IAdministradorServico administradorServico) =>
{

  int paginaTratada = (int)(pagina == null ? 1 : pagina);
  var administradores = administradorServico.Todos(paginaTratada);
  var adms = new List<AdministradorModelView>();
  foreach (var adm in administradores)
  {
    adms.Add(NormalizaAdmParaApresentacao(adm));
  }

  return Results.Ok(adms);
}).WithTags("Administradores");

app.MapGet("/administradores/{id}", ([FromRoute] int Id, IAdministradorServico administradorServico) =>
{
  var administrador = administradorServico.BuscaPorId(Id);

  if (administrador == null) return Results.NotFound();


  return Results.Ok(NormalizaAdmParaApresentacao(administrador));
}).WithTags("Administradores");

app.MapPut("/administradores/{id}", ([FromRoute] int Id, [FromBody] AdministradorDTO administradorDTO, IAdministradorServico administradorServico) =>
{
  var administrador = administradorServico.BuscaPorId(Id);

  if (administrador == null) return Results.NotFound();

  var validacao = ValidaAdministradorDTO(administradorDTO);

  if (validacao.Mensagens.Count > 0)
    return Results.BadRequest(validacao);

  administrador.Email = administradorDTO.Email;
  administrador.Senha = administradorDTO.Senha;
  administrador.Perfil = administradorDTO.Perfil;

  administradorServico.Atualizar(administrador);

  return Results.Ok(NormalizaAdmParaApresentacao(administrador));
}).WithTags("Administradores");

app.MapDelete("/administradores/{id}", ([FromRoute] int Id, IAdministradorServico administradorServico) =>
{
  var administrador = administradorServico.BuscaPorId(Id);

  if (administrador == null) return Results.NotFound();

  administradorServico.Apagar(administrador);

  return Results.NoContent();
}).WithTags("Administradores");

#endregion

#region  Veiculos
ErrosDeValidacao ValidaVeiculoDTO(VeiculoDTO veiculoDTO)
{
  var validacao = new ErrosDeValidacao
  {
    Mensagens = new List<string>()
  };

  if (string.IsNullOrEmpty(veiculoDTO.Nome))
    validacao.Mensagens.Add("O nome não pode ficar em branco");

  if (string.IsNullOrEmpty(veiculoDTO.Marca))
    validacao.Mensagens.Add("A marca não pode ficar em branco");

  if (veiculoDTO.Ano < 1950)
    validacao.Mensagens.Add("Veiculo muito antigo, aceito somente veiculos a partir de 1950");

  return validacao;
}

app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{

  var validacao = ValidaVeiculoDTO(veiculoDTO);

  if (validacao.Mensagens.Count > 0)
    return Results.BadRequest(validacao);

  var veiculo = new Veiculo
  {
    Nome = veiculoDTO.Nome,
    Ano = veiculoDTO.Ano,
    Marca = veiculoDTO.Marca
  };
  veiculoServico.Incluir(veiculo);

  return Results.Created($"/veiculos/{veiculo.Id}", veiculo);
}).WithTags("Veiculos");

app.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) =>
{
  int paginaTratada = (int)(pagina == null ? 1 : pagina);
  var veiculos = veiculoServico.Todos(paginaTratada);

  return Results.Ok(veiculos);
}).WithTags("Veiculos");

app.MapGet("/veiculos/{id}", ([FromRoute] int Id, IVeiculoServico veiculoServico) =>
{
  var veiculo = veiculoServico.BuscaPorId(Id);

  if (veiculo == null) return Results.NotFound();


  return Results.Ok(veiculo);
}).WithTags("Veiculos");

app.MapPut("/veiculos/{id}", ([FromRoute] int Id, [FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{
  var veiculo = veiculoServico.BuscaPorId(Id);

  if (veiculo == null) return Results.NotFound();

  var validacao = ValidaVeiculoDTO(veiculoDTO);

  if (validacao.Mensagens.Count > 0)
    return Results.BadRequest(validacao);

  veiculo.Nome = veiculoDTO.Nome;
  veiculo.Marca = veiculoDTO.Marca;
  veiculo.Ano = veiculoDTO.Ano;

  veiculoServico.Atualizar(veiculo);

  return Results.Ok(veiculo);
}).WithTags("Veiculos");

app.MapDelete("/veiculos/{id}", ([FromRoute] int Id, IVeiculoServico veiculoServico) =>
{
  var veiculo = veiculoServico.BuscaPorId(Id);

  if (veiculo == null) return Results.NotFound();

  veiculoServico.Apagar(veiculo);

  return Results.NoContent();
}).WithTags("Veiculos");
#endregion

#region App
app.UseSwagger();
app.UseSwaggerUI();
app.Run();
#endregion

