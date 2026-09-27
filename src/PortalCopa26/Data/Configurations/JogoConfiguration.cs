using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalCopa26.Domain.Entities;

namespace PortalCopa26.Data.Configurations;

public class JogoConfiguration : IEntityTypeConfiguration<Jogo>
{
    public void Configure(EntityTypeBuilder<Jogo> builder)
    {
        builder.HasIndex(j => j.Numero).IsUnique();

        builder.Property(j => j.Fase).HasConversion<string>();

        builder.HasOne(j => j.Grupo)
            .WithMany()
            .HasForeignKey(j => j.GrupoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(j => j.Mandante)
            .WithMany()
            .HasForeignKey(j => j.MandanteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(j => j.Visitante)
            .WithMany()
            .HasForeignKey(j => j.VisitanteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
