namespace PortalCopa26.Domain.Entities;

public class Grupo
{
    public int Id { get; set; }
    public char Letra { get; set; }

    public List<Selecao> Selecoes { get; set; } = [];
}
