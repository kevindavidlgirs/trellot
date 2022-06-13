# Projet PRID 2021 - Groupe 06 - Marie-Hélène Devogeleer et Kevin David L. Girs. 2020 - 2021

### Liste des utilisateurs et mots de passes

  * Utilisateur `admin`, mot de passe `admin`
  * Utilisateur `ben`, mot de passe `ben`
  * Utilisateur `bruno`, mot de passe `bruno`
  * Utilisateur `mhd`, mot de passe `ABC123"`
  * Utilisateur `kdg`, mot de passe `ABC123"`
  * Utilisateur `ve`, mot de passe `ABC123"`
  * Utilisateur `bg`, mot de passe `ABC123"`
  * Utilisateur `rms`, mot de passe `ABC123"`

### Liste des bugs connus

  * Lors d'un ajout de post dans une Card, il y a un retour "NotFound()". Il est cependant gérable avec un SaveChanges() simple.
  * Il arrive que, lors d'un refresh d'un tableau, une erreur "owner.undefined" survienne. Nous sommes dans l'impossibilité de reproduire et d'isoler le bug, et donc, de le résoudre.

### Liste des fonctionnalités supplémentaires

  * Refresh Token
    Les tokens se rafraichissent automatiquement (après 10 minutes).
  
  * Websocket 
    Permet aux utilisateurs de se connecter à un hub.
	Si un utilisateur effectue une modification, les changements se produiront également chez les autres.
	Est testable en lancant deux fois l'application, en se connectant à deux utilisateurs liés sur un tableau, et en observant qu'une modification se reflete chez l'autre en direct.
    
  * Post sur card (texte ou image)
    Sur chaque card, il est possible d'ajouter des "posts", sous forme soit textuelle, soit imagée.
	Pour réaliser ceci, il est nécessaire d'être ajouté à la carte (ou alors d'en être le propriétaire).
	
  * Tags sur card
    Sur chaque card, il est possible d'ajouter (et de supprimer) des tags.
	Pour réaliser ceci, il est nécessaire d'être ajouté à la carte (ou alors d'en être le propriétaire).
	
  * Profil User
	Utilisateur peut supprimer son profil.
    
### Divers
	Au cours du travail, nous avons totalement migré vers MySQL.

### Fonctionnalités manquantes
    Lors d'un Sign Up, il n'y a pas de vérification pour la date de naissance.
