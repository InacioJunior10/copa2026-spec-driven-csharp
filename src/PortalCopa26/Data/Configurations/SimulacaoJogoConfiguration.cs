using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalCopa26.Domain.Entities;

namespace PortalCopa26.Data.Configurations;

public class SimulacaoJogoConfiguration : IEntityTypeConfiguration<SimulacaoJogo>
{
    public void Configure(EntityTypeBuilder<SimulacaoJogo> builder)
    {
        builder.HasIndex(sj => new { sj.SimulacaoId, sj.JogoId }).IsUnique();

        builder.HasOne(sj => sj.Simulacao)
            .WithMany(s => s.Jogos)
            .HasForeignKey(sj => sj.SimulacaoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sj => sj.Jogo)
            .WithMany()
            .HasForeignKey(sj => sj.JogoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_SimulacaoJogo_GolsMandante_Faixa", "GolsMandante >= 0 AND GolsMandante <= 30"));
        builder.ToTable(t => t.HasCheckConstraint(
            "CK_SimulacaoJogo_GolsVisitante_Faixa", "GolsVisitante >= 0 AND GolsVisitante <= 30"));
    }
}
