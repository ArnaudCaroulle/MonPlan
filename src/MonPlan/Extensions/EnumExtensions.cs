using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace MonPlan.Extensions;

/// <summary>
/// Fournit les libellés français déclarés sur les valeurs d'énumération.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Retourne le libellé Display de la valeur, sans exposer son nom technique à l'interface.
    /// </summary>
    public static string Libelle(this Enum valeur) => valeur.GetType()
        .GetMember(valeur.ToString()).First()
        .GetCustomAttribute<DisplayAttribute>()?.GetName() ?? valeur.ToString();
}
