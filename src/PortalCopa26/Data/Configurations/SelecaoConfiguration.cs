using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalCopa26.Domain.Entities;

namespace PortalCopa26.Data.Configurations;

public class SelecaoConfiguration : IEntityTypeConfiguration<Selecao>
{
    public void Configure(EntityTypeBuilder<Selecao> builder)
    {
        builder.HasIndex(s => s.Codigo).IsUnique();

        builder.HasOne(s => s.Grupo)
            .WithMany(g => g.Selecoes)
            .HasForeignKey(s => s.GrupoId);
    }
}
