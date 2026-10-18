# LocatCam Mobile — .NET MAUI

Application mobile de consultation d'annonces immobilières LocatCam, consommant
l'API .NET hébergée sur `http://169.58.69.36:5206/`.

## État du projet

| Phase | Branche | Statut | Description |
|-------|---------|--------|-------------|
| A | `phase/a-setup` | ✅ Terminé | Refit, HttpClient, DI, SecureStorage, IApiClient, TokenHandler squelette |
| B | — | ⏳ En attente | Authentification JWT + TokenHandler avec refresh auto |
| C | — | ⏳ En attente | DTOs, services métier, base SQLite hors-ligne |
| D | — | ⏳ En attente | Design system + écrans Accueil, Détail, Favoris, Profil |
| E | — | ⏳ En attente | Carte Leaflet + OSM |
| F | — | ⏳ En attente | Publication annonces + messagerie in-app |
| G | — | ⏳ En attente | Mode sombre, hors-ligne, accessibilité, empty states |
| H | — | ⏳ En attente | Tests finaux, documentation, build Release |

## Stack technique

- **.NET MAUI** (net10.0-android)
- **MVVM** avec `CommunityToolkit.Mvvm` 8.4.0
- **CommunityToolkit.Maui** 8.0.0
- **Refit** 7.0.0 + `Refit.HttpClientFactory` (client API typé)
- **sqlite-net-pcl** 1.9.172 (base locale hors-ligne)
- **Polly** 8.4.1 (retry réseau)
- **CloudinaryDotNet** 1.22.0 (préparé, non activé — voir anomalie B1)
- **SecureStorage** (stockage des tokens JWT)
- **WebView + Leaflet.js + OpenStreetMap** (carte interactive)

## URL de l'API

`http://169.58.69.36:5206/`

## Anomalies API connues (contournées côté mobile)

| Réf | Anomalie | Contournement mobile |
|-----|----------|----------------------|
| B1 | `POST /api/photos/signature` sans auth | Upload photo désactivé tant que non corrigé |
| B2 | `POST /api/auth/refresh` cassé (400/503) | Logique de refresh impliquée, redirection login si échec |
| B3 | `POST /api/signalements` accepte raison vide | Validation côté mobile (min 5 caractères) |
| M1 | `DELETE /api/favoris/{id}` retourne 200 | Accepter 200 et 204 |
| M3 | Fuite détails 500 | Message générique "Erreur serveur" en UI |

## Build

```bash
# Restaurer les packages NuGet
dotnet restore

# Compiler pour Android (Debug)
dotnet build -f net10.0-android -c Debug

# Publier en Release
dotnet publish -f net10.0-android -c Release
```

> **Note :** Le SDK .NET MAUI avec la charge de travail Android (`maui-android`) doit être installé sur la machine de build.

## Workflow Git

Chaque phase est développée sur sa propre branche (`phase/a-setup`, `phase/b-auth`, etc.),
puis mergée dans `main` avec `--no-ff` après validation.

Voir `AUDIT_01_OPENAPI.md` à `AUDIT_04_STARTER_KIT.md` à la racine du dépôt parent
pour les rapports d'audit complets de l'API.
