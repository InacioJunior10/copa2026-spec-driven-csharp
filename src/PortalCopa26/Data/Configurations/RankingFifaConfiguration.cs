using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalCopa26.Domain.Entities;

namespace PortalCopa26.Data.Configurations;

public class RankingFifaConfiguration : IEntityTypeConfiguration<RankingFifa>
{
    public void Configure(EntityTypeBuilder<RankingFifa> builder)
    {
        builder.HasIndex(r => r.Posicao).IsUnique();

        builder.HasOne(r => r.Selecao)
            .WithMany()
            .HasForeignKey(r => r.SelecaoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
