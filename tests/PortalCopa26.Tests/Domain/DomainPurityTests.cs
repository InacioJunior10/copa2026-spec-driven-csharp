using System.Reflection;
using PortalCopa26.Domain.Entities;
using Xunit;

namespace PortalCopa26.Tests.Domain;

/// <summary>
/// D1: Domain/ deve permanecer livre de EF Core para poder ser extraído
/// para um projeto separado no futuro sem reescrita.
/// </summary>
public class DomainPurityTests
{
    private static IEnumerable<Type> DomainTypes =>
        typeof(Grupo).Assembly.GetTypes()
            .Where(t => t.Namespace is not null && t.Namespace.StartsWith("PortalCopa26.Domain"));

    [Fact]
    public void Domain_Types_Do_Not_Reference_EntityFrameworkCore()
    {
        foreach (var type in DomainTypes)
        {
            Assert.DoesNotContain(type.GetCustomAttributes(), IsEfCoreAttribute);

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                Assert.False(
                    IsEfCoreNamespace(property.PropertyType.Namespace),
                    $"{type.FullName}.{property.Name} referencia {property.PropertyType.FullName}, do namespace Microsoft.EntityFrameworkCore.");

                Assert.DoesNotContain(property.GetCustomAttributes(), IsEfCoreAttribute);
            }
        }
    }

    private static bool IsEfCoreAttribute(Attribute attribute) =>
        IsEfCoreNamespace(attribute.GetType().Namespace);

    private static bool IsEfCoreNamespace(string? ns) =>
        ns is not null && ns.StartsWith("Microsoft.EntityFrameworkCore");
}
