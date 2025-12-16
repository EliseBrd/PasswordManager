# Contexte
Dans un contexte où la sécurité informatique est au cœur des préoccupations, où les 
attaques sont de plus en plus fréquentes, notamment à cause de l’utilisation d’intelligences 
artificielles et de techniques de phishing, personne n’est à l’abri d’ouvrir (involontairement) 
une brèche dans des systèmes pourtant sécurisés.

Les attaques qui permettent d'extraire des données contenant des informations personnelles, 
comme une adresse e-mail ou un mot de passe sont légion. Toutes ces attaques 
peuvent mener à des recoupements de ces données, permettant à d’autres pirates d’accéder 
à des comptes utilisateur grâce à l’adresse e-mail et à un mot de passe récupéré ailleurs.

Parce qu’il est difficile d’avoir des mots de passe différents pour chaque compte créé 
et de les mémoriser tous, beaucoup choisissent la facilité : utiliser sa date de naissance, 
la combinaison des dates de naissance de ses enfants, le prénom du chien, 
ou des mots de passe utilisés par d’autres personnes parce qu’ils sont « génériques » 
et plus faciles à retenir. Ces mots de passe sont plus faciles à deviner et donc vulnérables.

La mise en place d'un gestionnaire de mots de passe dans une entreprise peut être 
un véritable défi. Bien qu'il existe déjà des solutions, notre entreprise a décidé de créer 
sa propre solution.

# Objectif
Développer une application permettant aux utilisateurs de stocker et gérer leurs mots de passe 
de manière sécurisée.

# Principaux concepts de cryptographie

## Empreinte de hachage
Une **empreinte de hachage** est une suite d'octets produite par une fonction cryptographique 
appelée **fonction de hachage** à partir d'une donnée (par exemple, un mot de passe).

::: mermaid
flowchart LR
PWD(Donnée)
SHA(Fonction de hachage)
HASH(Empreinte de hachage)

PWD --> SHA
SHA --> HASH
:::

Le moindre changement dans la donnée d'origine modifie complètement l'empreinte. 
Les empreintes servent à **vérifier l'intégrité** d'une information ou à stocker des mots de 
passe sans conserver le secret en clair : on compare des empreintes plutôt que des mots de passe.

Pour se protéger contre  les attaques de type **rainbow tables**, on associe à chaque mot de 
passe un **sel aléatoire et unique** avant de calculer le hachage et on utilise un algorithme 
adapté aux mots de passe (par exemple `bcrypt`, `scrypt` ou `Argon2`) plutôt qu’une fonction de 
hachage rapide comme `SHA-256`.

::: mermaid
flowchart LR
PWD("Mot de passe : 'Not24GET!'")
SALT("Sel aléatoire : '...'")
KDF("bcrypt / Argon2 (intégrant sel + coût)")
HASH("Empreinte résultante")
PWD --> KDF
SALT --> KDF
KDF --> HASH
:::

Pour l'ensemble des diagrammes ci-dessous, la communication entre le client et le serveur est 
considérée comme sûre car chiffrée avec `TLS`.

### Création d'un compte utilisateur

::: mermaid
sequenceDiagram
participant U as Utilisateur
participant C as Client (navigateur)
participant S as Serveur

U->>C: Saisie nom du compte (user) et mot de passe (pwd)
C->>S: POST /Account (user + pwd)
S->>S: Générer un sel aléatoire (salt)
S->>S: Calculer l'empreinte de hachage (hash) à l'aide de (pwd, salt)
S->>S: Stocker le compte utilisateur (user, salt, hash)
S-->>C: 200 OK
:::

### Connexion d'un compte utilisateur

::: mermaid
sequenceDiagram
participant U as Utilisateur
participant C as Client (navigateur)
participant S as Serveur

U->>C: Saisie nom du compte (user) et mot de passe (pwd)
C->>S: POST /login (user + pwd)
S->>S: Récupérer le sel de l'utilisateur stocké (salt)
S->>S: Calculer l'empreinte de hachage (hash) à l'aide de (pwd, salt)
S->>S: Compare l'empreinte générée avec l'empreinte stockée
S-->>C: 200 OK / token
:::

## Chiffrement et déchiffrement à partir d'une clef symétrique
Une **clef symétrique** est une suite d'octets utilisée pour **chiffrer** et **déchiffrer** des données.
Elle doit être générée avec une **forte entropie** (aléatoire de haute qualité) et **rester secrète**.

### Chiffrement
::: mermaid
flowchart
CLEAR(Donnée à chiffrer : 'test')
KEY(Clef symétrique : '3697AF44...')
IV(Vecteur d'initialisation aléatoire: 'D43EF5A4...')
AES(AES-GCM)
CYPHER(Donnée chiffrée : '1787BD6CA...')
AUTHTAG(Tag d'authentification : '5079B494...')

CLEAR --> AES
KEY --> AES
IV --> AES
AES --> CYPHER
AES --> AUTHTAG
:::

Le **vecteur d'initialisation** (IV) est une valeur aléatoire générée de façon 
cryptographiquement sûre.
Il est essentiel pour garantir la sécurité de `AES-GCM` : chaque IV doit être 
**unique pour chaque chiffrement**.
`AES-GCM` retourne à la fois la donnée chiffrée ainsi qu'un **tag d'authentification** 
qui permet de vérifier l'intégrité des données lors du déchiffrement.

La **donnée chiffrée**, l'**IV** et le **tag d'authentification** doivent être stockés 
ensemble pour permettre un déchiffrement.

### Déchiffrement
::: mermaid
flowchart
CYPHER(Donnée à déchiffrer : '1787BD6CA...')
KEY(Clef symétrique : '3697AF44...')
IV(Vecteur d'initialisation aléatoire: 'D43EF5A4...')
AES(AES-GCM)
CLEAR(Donnée déchiffrée : 'test')
AUTHTAG(Tag d'authentification : '5079B494...')

CYPHER --> AES
AUTHTAG --> AES
IV --> AES
KEY --> AES
AES --> CLEAR
:::



### Dérivation d'un mot de passe
Pour se sécuriser des attaques par force brut, on utilise jamais un mot de passe brut comme clef 
symétrique pour chiffrer ou déchiffrer des données. On applique une **fonction de dérivation 
de clef (KDF)** au mot de passe pour augmenter le temps nécessaire pour générer la clef symétrique.

Par exemple, `PBKDF2` applique plusieurs milliers d'itérations à un mot de passe combiné à un 
**sel aléatoire unique** pour produire une clef symétrique :

::: mermaid
flowchart
PWD(Mot de passe : 'Not24GET!')
SALT(Sel aléatoire: 'AD7E70F1...')
ITERATION(Nombre d'itération: 50000)
PBKDF2(PBKDF2)
KEY(Clef privée : '3697AF44...')

PWD --> PBKDF2
SALT --> PBKDF2
ITERATION --> PBKDF2
PBKDF2 --> KEY
:::

Le **sel** est une valeur aléatoire générée de façon cryptographiquement sûre, qui doit être unique 
pour chaque dérivation de clef. Le nombre d’itérations doit être suffisamment élevé pour rendre la 
dérivation coûteuse et ainsi protéger efficacement contre les attaques par force brute.

## Clef asymétrique
Une **clef asymétrique** se compose d’une **paire de clefs** liées mathématiquement : une **clef publique** et une **clef privée**.
La **clef publique** peut être partagée librement et sert à **chiffrer des données** ou à **vérifier une signature**.
La **clef privée** doit **rester secrète** et sert à **déchiffrer les données chiffrées** avec la clef publique ou à **signer des messages**.

Ce mécanisme permet de garantir la confidentialité, l’authenticité et l’intégrité des communications sans partager de secret préalable.

### Chiffrement et déchiffrement asymétrique

::: mermaid
flowchart LR
CLEAR(Donnée claire : 'Bonjour')
PUBKEY(Clef publique)
ENC(Chiffrement asymétrique)
CYPHER(Donnée chiffrée)

CLEAR --> ENC
PUBKEY --> ENC
ENC --> CYPHER
:::

Seule la **clef privée associée à la clef publique** peut déchiffrer la donnée chiffrée, assurant que seul le destinataire peut accéder à l’information.

::: mermaid
flowchart LR
CLEAR(Donnée claire : 'Bonjour')
PRIVKEY(Clef privée)
ENC(Déchiffrement asymétrique)
CYPHER(Donnée chiffrée)

CYPHER --> ENC
PRIVKEY --> ENC
ENC --> CLEAR
:::

# Fonctionnalités attendues

## Gestion de l'identité utilisateur
L'application doit s'appuyer entièrement sur l'authentification **Microsoft Entra ID**.
Seuls les utilisateurs affectés à l'application doivent pouvoir y accéder et la gestion 
des rôles doit également s'appuyer sur Microsoft Entra ID.

## Coffres-forts 
Chaque utilisateur peut créer un ou plusieurs coffres-forts de mots de passe.
Chaque coffre-fort est protégé par un mot de passe général, aussi appelé mot de passe maître, 
qui doit servir à **chiffrer et déchiffrer** les données sensibles du coffre.

## Entrées d'un coffre
Un coffre est constitué de plusieurs entrées qui contiennent les informations 
nécessaires pour représenter une identité numérique :
- Nom de l’entrée
- Nom d’utilisateur
- Mot de passe
- URL
- Commentaire
- Date de création
- Date de dernière modification

## Générateur de mot de passe
Lors de la création d’une entrée, le site doit proposer à l’utilisateur un mot de passe 
généré automatiquement. L’utilisateur doit pouvoir spécifier son propre mot de passe.
Lors de la création d’un mot de passe (que ce soit celui du coffre ou celui d’une entrée), 
l’utilisateur doit avoir un score calculé qui lui indique la complexité de son mot de passe.

## Force du mot de passe
L’application proposera une fonctionnalité de calcul de la force d’un mot de passe, ainsi 
que la possibilité de générer un mot de passe fort.
Lorsqu’un utilisateur s’inscrit, et lorsqu’il ajoute ou modifie une entrée, une vérification 
de la vulnérabilité du mot de passe est réalisée. Il aura alors la possibilité d’adapter son 
mot de passe pour l’améliorer, ou bien d’utiliser une fonctionnalité de génération d’un 
mot de passe fort.

## Chiffrement et déchiffrement des informations
L'ensemble des informations des entrées d'un coffre doit être **chiffré par le client** 
avant d'être envoyé au serveur.
De même, les opérations de **déchiffrement seront réalisées par le client** et non pas 
par le serveur. C'est la notion de chiffrement de bout en bout.

Les opérations de chiffrement et de déchiffrement des données vont être réalisées à l'aide 
d'une **clé privée symétrique** et de l'algorithme `AES-GCM`. La clé privée symétrique 
va être calculée à partir du mot de passe du coffre-fort à l'aide d'une opération de dérivation 
en utilisant l'algorithme `PBKDF2`.

Ceci permet de garantir que seul un client capable de fournir le mot de passe du coffre sera 
en mesure de déchiffrer les entrées d'un coffre.

## Partage d’un coffre
Un utilisateur doit pouvoir **partager un coffre** avec un ou plusieurs autres utilisateurs 
de l’application.
À noter que seul le créateur d’un coffre a la possibilité d’ajouter ou de supprimer un 
utilisateur à son coffre.
L'utilisateur garde la responsabilité de founrir le mot de passe maître du coffre à celui 
avec qui il souhaite partager un coffre.

## Traçabilité d’accès
Les actions des utilisateurs doivent être tracées (création d'une entrée, modification d'une 
entrée, visualisation d'un mot de passe).
La lecture de ces informations de traçabilité sera accessible uniquement par les utilisateurs 
ayant un rôle **Administrateur**.

## J'ai tout terminé et j'en veux encore
Certains sites demandent une authentification à double facteur (2FA) à l'aide du protocole 
`TOTP` en s'appuyant sur des applications de type **Authenticator**.
Il serait intéressant de permettre à l'utilisateur de sauvegarder la clef TOTP d'un compte 
de manière chiffrée au niveau d'une entrée d'un coffre et de générer, à partir de la clef, 
le code aléatoire.

# Contraintes techniques
L’architecture du projet doit respecter les contraintes suivantes :
- Utilisation de **Microsoft SQL Server 2022** ou **SQLite** pour le stockage des données
- Utilisation de **Entity Framework Core** pour l'accès aux données par l'API
- Développement d'une API REST à l'aide de **ASP .NET Core**
- Sécuriser les accès à l'API avec **Microsoft Entra ID**
- Développement d'une Application Blazor Web App en utilisant **Interactive Server** ou **Interactive Web Assembly**