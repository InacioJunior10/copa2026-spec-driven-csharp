namespace PortalCopa26.Domain.Entities;

public class Selecao
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Tecnico { get; set; } = string.Empty;
    public bool CabecaDeChave { get; set; }

    public int GrupoId { get; set; }
    public Grupo Grupo { get; set; } = null!;

    public List<Jogador> Jogadores { get; set; } = [];
}
