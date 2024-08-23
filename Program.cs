using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MinimalApi.Dominio.DTO;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.Dominio.ModelViews;
using MinimalApi.Dominio.Servicos;
using MinimalApi.Infraestrutura.Db;

#region Builder
var builder = WebApplication.CreateBuilder(args);

var key = builder.Configuration.GetSection("Jwt")["Key"]?.ToString();
if (string.IsNullOrEmpty(key)) key = "123456";

builder.Services.AddAuthentication(options =>
{
  options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
  options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
  options.TokenValidationParameters = new TokenValidationParameters
  {
    ValidateLifetime = true,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
    ValidateIssuer = false,
    ValidateAudience = false
  };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<IAdministradorServico, AdministradorServico>();
builder.Services.AddScoped<IVeiculoServico, VeiculoServico>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
  options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
  {
    Name = "UseAuthorization",
    Type = SecuritySchemeType.Http,
    Scheme = "Bearer",
    BearerFormat = "JWT",
    In = ParameterLocation.Header,
    Description = "Insira seu token JWT aqui"
  });

  options.AddSecurityRequirement(new OpenApiSecurityRequirement
  {
    {
      new OpenApiSecurityScheme{
        Reference = new OpenApiReference
        {
          Type = ReferenceType.SecurityScheme,
          Id = "Bearer"
        }
      },
      new string[] {}
    }
  });
});

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
app.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");
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

string GerarTokerJWT(Administrador administrador)
{
  if (string.IsNullOrEmpty(key)) return string.Empty;

  var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
  var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

  var claims = new List<Claim>(){
    new Claim("Email",administrador.Email),
    new Claim("Perfil", administrador.Perfil)
  };

  var token = new JwtSecurityToken(
    claims: claims,
    expires: DateTime.Now.AddDays(1),
    signingCredentials: credentials
  );

  return new JwtSecurityTokenHandler().WriteToken(token);
}

app.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) =>
{
  var adm = administradorServico.Login(loginDTO);
  if (adm != null)
  {
    string token = GerarTokerJWT(adm);
    return Results.Ok(new AdministradorLogadoModelView
    {
      Email = adm.Email,
      Perfil = adm.Perfil,
      Token = token
    });
  }
  else
    return Results.Unauthorized();
}).AllowAnonymous().WithTags("Administradores");

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
}).RequireAuthorization().WithTags("Administradores");

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
}).RequireAuthorization().WithTags("Administradores");

app.MapGet("/administradores/{id}", ([FromRoute] int Id, IAdministradorServico administradorServico) =>
{
  var administrador = administradorServico.BuscaPorId(Id);

  if (administrador == null) return Results.NotFound();


  return Results.Ok(NormalizaAdmParaApresentacao(administrador));
}).RequireAuthorization().WithTags("Administradores");

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
}).RequireAuthorization().WithTags("Administradores");

app.MapDelete("/administradores/{id}", ([FromRoute] int Id, IAdministradorServico administradorServico) =>
{
  var administrador = administradorServico.BuscaPorId(Id);

  if (administrador == null) return Results.NotFound();

  administradorServico.Apagar(administrador);

  return Results.NoContent();
}).RequireAuthorization().WithTags("Administradores");

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
}).RequireAuthorization().WithTags("Veiculos");

app.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) =>
{
  int paginaTratada = (int)(pagina == null ? 1 : pagina);
  var veiculos = veiculoServico.Todos(paginaTratada);

  return Results.Ok(veiculos);
}).RequireAuthorization().WithTags("Veiculos");

app.MapGet("/veiculos/{id}", ([FromRoute] int Id, IVeiculoServico veiculoServico) =>
{
  var veiculo = veiculoServico.BuscaPorId(Id);

  if (veiculo == null) return Results.NotFound();


  return Results.Ok(veiculo);
}).RequireAuthorization().WithTags("Veiculos");

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
}).RequireAuthorization().WithTags("Veiculos");

app.MapDelete("/veiculos/{id}", ([FromRoute] int Id, IVeiculoServico veiculoServico) =>
{
  var veiculo = veiculoServico.BuscaPorId(Id);

  if (veiculo == null) return Results.NotFound();

  veiculoServico.Apagar(veiculo);

  return Results.NoContent();
}).RequireAuthorization().WithTags("Veiculos");
#endregion

#region App
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();
app.Run();
#endregion

