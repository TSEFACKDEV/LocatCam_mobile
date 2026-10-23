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
    private static readonly Stack<string> _historique = new();

    /// <summary>
    /// Notifie les pages qu'une route vient d'être affichée. Les pages Shell
    /// restent en mémoire : OnAppearing n'est donc pas toujours rejoué et une
    /// page comme la messagerie doit pouvoir se resynchroniser explicitement.
    /// </summary>
    public static event Action<string>? PageAffichee;

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
        var ok = Naviguer(route);

        // Navigation directe (onglet, connexion, démarrage) : la pile de
        // retour précédente n'a plus de sens.
        if (ok)
            _historique.Clear();

        return ok;
    }

    private static bool Naviguer(string route)
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
                        PageAffichee?.Invoke(route);
                        return true;
                    }
                }
            }
        }

        // Repli : route absolue à trois slashs (///Route pour un ShellContent racine).
        _ = shell.GoToAsync("///" + route);
        PageAffichee?.Invoke(route);
        return false;
    }

    /// <summary>Va vers une page en empilant la page d'origine pour le retour.</summary>
    public static void AllerVersAvecRetour(string route, string? origine = null)
    {
        var precedent = !string.IsNullOrWhiteSpace(origine) ? origine : RouteCourante;

        if (!string.IsNullOrWhiteSpace(precedent) &&
            !string.Equals(precedent, route, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(precedent, "StartupPage", StringComparison.OrdinalIgnoreCase))
        {
            _historique.Push(precedent);
        }

        Naviguer(route);
    }

    /// <summary>Revient à la page précédente (accueil par défaut).</summary>
    public static void Revenir()
    {
        while (_historique.Count > 0)
        {
            var precedent = _historique.Pop();

            // StartupPage ne doit jamais être une destination « retour » : elle
            // ne rejoue pas sa logique quand on revient dessus.
            if (string.IsNullOrWhiteSpace(precedent) ||
                string.Equals(precedent, "StartupPage", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(precedent, RouteCourante, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (Naviguer(precedent))
                return;
        }

        Naviguer("HomePage");
    }
}
