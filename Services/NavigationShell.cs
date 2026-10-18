using Microsoft.Maui.Controls;

namespace LocatCam_mobile.Services;

/// <summary>
/// Navigation déterministe entre les pages racine du Shell.
///
/// Le Shell de LocatCam déclare ses pages directement en tant que ShellContent
/// (onboarding, accueil, carte, favoris, publier, profil, connexion, inscription,
/// détail). Dans cette configuration, la navigation par route ("//Route") est
/// fragile (parents implicites, échecs silencieux). On sélectionne donc
/// directement le ShellContent cible — exactement comme le ferait un tap sur un
/// onglet : synchrone, fiable, sans exception.
/// </summary>
public static class NavigationShell
{
    private static string _retour = "HomePage";

    /// <summary>Route de la page actuellement affichée (ou chaîne vide).</summary>
    public static string RouteCourante
    {
        get
        {
            var shell = Shell.Current;
            if (shell == null)
                return string.Empty;

            foreach (var item in shell.Items)
            {
                foreach (var section in item.Items)
                {
                    if (item.CurrentItem != section)
                        continue;

                    foreach (var content in section.Items)
                    {
                        if (section.CurrentItem == content)
                            return content.Route;
                    }
                }
            }

            return string.Empty;
        }
    }

    /// <summary>
    /// Va vers une page racine du Shell identifiée par sa route.
    /// Retourne true si la page a été trouvée dans la hiérarchie.
    /// </summary>
    public static bool AllerVers(string route)
    {
        var shell = Shell.Current;
        if (shell == null || string.IsNullOrWhiteSpace(route))
            return false;

        foreach (var item in shell.Items)
        {
            foreach (var section in item.Items)
            {
                foreach (var content in section.Items)
                {
                    if (string.Equals(content.Route, route, StringComparison.OrdinalIgnoreCase))
                    {
                        section.CurrentItem = content;
                        item.CurrentItem = section;
                        shell.CurrentItem = item;
                        return true;
                    }
                }
            }
        }

        // Repli : route absolue à trois slashs (///Route pour un ShellContent racine).
        _ = shell.GoToAsync("///" + route);
        return false;
    }

    /// <summary>Va vers une page en mémorisant la page courante (bouton Retour).</summary>
    public static void AllerVersAvecRetour(string route, string? origine = null)
    {
        if (!string.IsNullOrWhiteSpace(origine))
        {
            _retour = origine;
        }
        else
        {
            var courante = RouteCourante;
            if (!string.IsNullOrEmpty(courante) &&
                !string.Equals(courante, route, StringComparison.OrdinalIgnoreCase))
            {
                _retour = courante;
            }
        }

        AllerVers(route);
    }

    /// <summary>Revient à la page précédente mémorisée (accueil par défaut).</summary>
    public static void Revenir()
    {
        // StartupPage ne doit jamais être une destination « retour » : elle ne
        // rejoue pas sa logique quand on revient dessus.
        if (string.IsNullOrWhiteSpace(_retour) ||
            string.Equals(_retour, "StartupPage", StringComparison.OrdinalIgnoreCase))
        {
            _retour = "HomePage";
        }

        AllerVers(_retour);
    }
}
