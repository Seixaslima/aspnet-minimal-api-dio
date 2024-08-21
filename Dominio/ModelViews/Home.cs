namespace MinimalApi.Dominio.ModelViews;

public struct Home
{
  public string Mensagem { get => "Bem vindo a minh API"; }
  public string Documentacao { get => "/swagger"; }
}