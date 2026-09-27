using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalCopa26.Domain.Entities;

namespace PortalCopa26.Data.Configurations;

public class JogadorConfiguration : IEntityTypeConfiguration<Jogador>
{
    public void Configure(EntityTypeBuilder<Jogador> builder)
    {
        builder.Property(j => j.Posicao).HasConversion<string>();

        builder.HasOne(j => j.Selecao)
            .WithMany(s => s.Jogadores)
            .HasForeignKey(j => j.SelecaoId);
    }
}
