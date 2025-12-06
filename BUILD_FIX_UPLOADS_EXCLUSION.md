# Build Fix - Exclude Uploads Folder

## Problème
Le build du backend échouait avec des erreurs de compilation concernant des fichiers dans le dossier `uploads`:

```
error CS0246: Le nom de type ou d'espace de noms 'ProjetResponse' est introuvable
error CS0246: Le nom de type ou d'espace de noms 'IProjetService' est introuvable
```

## Cause
Le dossier `uploads` contient les fichiers uploadés par les utilisateurs pour conversion. Certains de ces fichiers sont des fichiers `.cs` (C#) qui ne font pas partie du projet mais que le compilateur essayait de compiler automatiquement.

Par défaut, le SDK .NET inclut tous les fichiers `.cs` dans le projet pour compilation, y compris ceux dans les sous-dossiers.

## Solution
Ajout d'une exclusion explicite du dossier `uploads` dans le fichier `.csproj` pour empêcher la compilation de ces fichiers.

## Changement

### Fichier: `backend/StackOverGrave.Api.csproj`

**Ajouté:**
```xml
<!-- Exclude uploaded files from compilation -->
<ItemGroup>
  <Compile Remove="uploads\**" />
  <Content Remove="uploads\**" />
  <EmbeddedResource Remove="uploads\**" />
  <None Remove="uploads\**" />
</ItemGroup>
```

**Explication:**
- `<Compile Remove="uploads\**" />` - Exclut tous les fichiers du dossier uploads de la compilation
- `<Content Remove="uploads\**" />` - Exclut les fichiers de contenu
- `<EmbeddedResource Remove="uploads\**" />` - Exclut les ressources embarquées
- `<None Remove="uploads\**" />` - Exclut les autres fichiers
- `**` signifie "tous les fichiers et sous-dossiers"

## Dossiers qui devraient être exclus

Les dossiers suivants contiennent des fichiers utilisateur et ne doivent pas être compilés:
- `uploads/` - Fichiers uploadés individuellement
- `repositories/` - Repositories Git clonés ou ZIP extraits (si ce dossier existe)

## Vérification

Après le changement:
```bash
cd backend
dotnet build
```

Résultat: ✅ Build réussi avec seulement des avertissements (warnings), pas d'erreurs

## Note

Cette exclusion n'affecte pas le fonctionnement de l'application. Les fichiers dans `uploads/` sont:
1. Lus par le service de stockage de fichiers
2. Analysés par le service de détection de technologie
3. Convertis par le service AI
4. Jamais compilés ou exécutés

C'est le comportement attendu et sécurisé.
