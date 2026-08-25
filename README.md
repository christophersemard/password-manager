<div align="center">

# 🔐 Password Manager

### Gestionnaire de mots de passe pédagogique en Blazor et ASP.NET Core

![C#](https://img.shields.io/badge/C%23-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Blazor](https://img.shields.io/badge/Blazor-512BD4?style=for-the-badge&logo=blazor&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![SQLite](https://img.shields.io/badge/SQLite-003B57?style=for-the-badge&logo=sqlite&logoColor=white)
![Tests](https://img.shields.io/badge/Tests-xUnit_%26_bUnit-16a34a?style=for-the-badge)

</div>

---

## 🎯 Présentation

Cette application permet de créer un compte et de gérer localement une collection
d'identifiants : ajout, modification, suppression, recherche, catégorisation et
génération de mots de passe.

Le projet associe une interface **Blazor**, une API **ASP.NET Core**, une base
**SQLite** et des tests **xUnit/bUnit**. Il a été réalisé dans un contexte
pédagogique pour explorer l'authentification JWT et le chiffrement côté client.

> Ce dépôt est une démonstration technique et non un gestionnaire destiné à stocker
> de véritables secrets. Voir la section « Sécurité et limites » avant utilisation.

## ✨ Fonctionnalités

- inscription et connexion avec jeton JWT ;
- création, consultation, modification et suppression d'entrées ;
- recherche et filtrage par catégorie ;
- génération de mots de passe paramétrable ;
- chiffrement et déchiffrement côté client ;
- copie d'un identifiant dans le presse-papiers ;
- persistance locale avec SQLite ;
- tests unitaires et tests de composants.

---

## 🧭 Architecture

~~~mermaid
flowchart LR
    U[Utilisateur]
    B[Blazor Web App]
    A[API ASP.NET Core]
    D[(SQLite)]

    U --> B
    B -->|JWT + données chiffrées| A
    A -->|Entity Framework Core| D
~~~

| Projet | Rôle |
|---|---|
| **Web/Web** | Hôte Blazor et rendu interactif |
| **Web/Web.Client** | Interface, authentification et chiffrement côté client |
| **Api** | API REST, JWT et persistance Entity Framework Core |
| **Core** | Modèles et objets de transfert partagés |
| **Web.Tests** | Tests xUnit, bUnit et API |

---

## 🚀 Lancer le projet localement

### Prérequis

- SDK .NET 9 ;
- outil Entity Framework Core CLI.

~~~bash
dotnet tool install --global dotnet-ef
~~~

### Installation

~~~bash
git clone https://github.com/christophersemard/PasswordManager.git
cd PasswordManager
dotnet restore
dotnet ef database update --project Api --startup-project Api
~~~

La migration crée une base SQLite vide avec trois catégories de démonstration.
Aucune base contenant un utilisateur n'est versionnée.

### Démarrer l'API

Dans un premier terminal :

~~~bash
dotnet run --project Api
~~~

L'API écoute par défaut sur <http://localhost:5269>.

### Démarrer l'interface

Dans un second terminal :

~~~bash
dotnet run --project Web/Web
~~~

L'interface est accessible sur <http://localhost:5215>.

### Configuration JWT

La valeur présente dans les fichiers de configuration est uniquement un exemple
local. Pour utiliser une autre clé sans modifier les fichiers suivis :

~~~powershell
$env:Jwt__Secret = "une-cle-locale-longue-et-aleatoire"
~~~

La même clé doit être définie dans les terminaux de l'API et de l'interface.

---

## 🧪 Tests

~~~bash
dotnet test
~~~

Le projet de tests couvre notamment :

- l'inscription et l'authentification ;
- les opérations protégées de l'API ;
- la dérivation et l'utilisation de la clé de chiffrement ;
- certains composants de formulaire Blazor.

---

## 🔒 Sécurité et limites

### Fonctionnement actuel

- le mot de passe du compte est dérivé avec PBKDF2-HMAC-SHA256 et un sel ;
- la clé AES est dérivée du mot de passe principal côté client ;
- chaque chiffrement AES-CBC génère un vecteur d'initialisation aléatoire ;
- l'API reçoit et stocke uniquement la valeur chiffrée ;
- la clé de chiffrement est conservée dans le Session Storage pendant la session.

### Limites connues

- AES-CBC ne fournit pas ici de contrôle d'intégrité authentifié ;
- le Session Storage reste accessible au code exécuté dans le même contexte web ;
- aucune protection avancée contre les tentatives répétées n'est implémentée ;
- la gestion, rotation et récupération des clés ne correspond pas aux exigences
  d'un produit de production ;
- la configuration locale utilise HTTP et une clé JWT de démonstration.

Une version destinée à un usage réel devrait employer un chiffrement authentifié
comme AES-GCM, renforcer la gestion du secret principal, ajouter une politique de
verrouillage et faire l'objet d'un audit de sécurité indépendant.

---

## 📁 Organisation

~~~text
.
├── Api/
│   ├── Controllers/
│   ├── Data/
│   └── Migrations/
├── Core/
│   ├── Dtos/
│   └── Models/
├── Web/
│   ├── Web/
│   └── Web.Client/
├── Web.Tests/
├── PasswordManager.sln
└── README.md
~~~

---

## 👤 Auteur

Projet réalisé par [Christopher Semard](https://github.com/christophersemard)
dans le cadre de sa formation de développeur full-stack.
