
using prid_2021_g06.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.Extensions.Logging;
using System.Linq;
using Microsoft.EntityFrameworkCore;

public class DbInitializer {
    public static void Initialize(g06Context context, IServiceProvider services) {

        var logger = services.GetRequiredService<ILogger<DbInitializer>>();

        context.Database.EnsureCreated();

        if (context.Users.Any()) {

            logger.LogInformation("The database was already seeded");
            return;
        }
        logger.LogInformation("Start seeding the database.");

        // Création des "Users"
        var u1 = new User { Id = 1, Pseudo = "admin", Password = "admin", Email = "admin@test.be", Role = Role.Admin };
        var u2 = new User { Id = 2, Pseudo = "ben", Password = "ben", Email = "ben@test.be", Role = Role.Admin };
        var u3 = new User { Id = 3, Pseudo = "bruno", Password = "bruno", Email = "bruno@test.be", Role = Role.User };
        var u4 = new User { Id = 4, Pseudo = "mhd", Password = "ABC123", Email = "mhdevogeleer@gmail.com", LastName = "Devogeleer", FirstName = "Marie-Hélène", Role = Role.User };
        var u5 = new User { Id = 5, Pseudo = "kdg", Password = "ABC123", Email = "kevindavidgirs@outlook.com", LastName = "Girs", FirstName = "Kevin", Role = Role.User, BirthDate = new DateTime(1990, 6, 21) };
        var u6 = new User { Id = 6, Pseudo = "ve", Password = "ABC123", Email = "virginie.efira@outlook.com", LastName = "Efira", FirstName = "Virginie", Role = Role.User, BirthDate = new DateTime(1977, 5, 5) };
        var u7 = new User { Id = 7, Pseudo = "bg", Password = "ABC123", Email = "williamhenrygates@outlook.com", LastName = "Gates", FirstName = "Bill", Role = Role.User, BirthDate = new DateTime(1955, 10, 28) };
        var u8 = new User { Id = 8, Pseudo = "rms", Password = "ABC123", Email = "richardmatthewstallman@outlook.com", LastName = "Stallman", FirstName = "Richard Matthew", Role = Role.Admin, BirthDate = new DateTime(1953, 3, 16) };

        context.Users.AddRange(u1, u2, u3, u4, u5, u6, u7, u8);

        //////////////////
        // EXPLICATIONS //
        //////////////////
        // Ce qui suit est nécessaire pour pouvoir ajouter les éléments avec leurs id's.
        // Sinon, les éléments s'ajoutent mais nous n'avons plus le contrôle sur les id's (admin --> id = 5 par exemple).
        //////////////////
        // Par le fait que ma MySql ne nécessite pas les options suivante nous les mettons sous condidition.
        ////////////////// 
        if (context.Database.IsSqlServer()) {
            context.Database.OpenConnection();
            try {
                context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Users ON");
                context.SaveChanges();
                context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Users OFF");
                context.SaveChanges();
            } finally {
                context.Database.CloseConnection();
            }
        }

        // Création de "Boards"
        var b1 = new Board { Id = 1, Name = "Board1", Owner = context.Users.Find(1)};
        var b2 = new Board { Id = 2, Name = "Board2", Owner = context.Users.Find(2)};
        var b3 = new Board { Id = 3, Name = "Board3", Owner = context.Users.Find(3) };
        context.Boards.AddRange(b1, b2, b3);

        if (context.Database.IsSqlServer()) {
            context.Database.OpenConnection();
            try {
                context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Boards ON");
                context.SaveChanges();
                context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Boards OFF");
                context.SaveChanges();
            } finally {
                context.Database.CloseConnection();
            }
        }

        // Création des relations entre "Users" et "Boards" 
        var ub1 = new UserBoard(){ BoardId = 1, UserId = 2 };
        var ub2 = new UserBoard(){ BoardId = 1, UserId = 3 };
        var ub3 = new UserBoard(){ BoardId = 3, UserId = 2 };
        var ub4 = new UserBoard(){ BoardId = 3, UserId = 1 };
        var ub5 = new UserBoard(){ BoardId = 2, UserId = 4 };
        context.UsersBoardsRelation.AddRange(ub1, ub2, ub3, ub4, ub5);
        context.SaveChanges();

        //Création des "BoardLists"
        var bl1 = new BoardList { Id = 1, Name = "bl1", Board = context.Boards.Find(1) };
        var bl2 = new BoardList { Id = 2, Name = "bl2", Board = context.Boards.Find(1) };
        var bl3 = new BoardList { Id = 3, Name = "bl3", Board = context.Boards.Find(1) };
        var bl4 = new BoardList { Id = 4, Name = "bl4", Board = context.Boards.Find(1) };
        var bl5 = new BoardList { Id = 5, Name = "bl5", Board = context.Boards.Find(3) };
        var bl6 = new BoardList { Id = 6, Name = "bl6", Board = context.Boards.Find(2) };
        var bl7 = new BoardList { Id = 7, Name = "bl7", Board = context.Boards.Find(2) };
        context.BoardLists.AddRange(bl1, bl2, bl3, bl4, bl5, bl6, bl7);

        if (context.Database.IsSqlServer()) {
            context.Database.OpenConnection();
            try {
                context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.BoardLists ON");
                context.SaveChanges();
                context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.BoardLists OFF");
                context.SaveChanges();
            } finally {
                context.Database.CloseConnection();
            }
        }

        // Création des "Cards"
        var c1 = new Card { Id = 1, Name = "card1", Owner = context.Users.Find(2), indexIntoBoardList = 2, BoardList = context.BoardLists.Find(1) };
        var c2 = new Card { Id = 2, Name = "card2", Owner = context.Users.Find(3), indexIntoBoardList = 0, BoardList = context.BoardLists.Find(1) };
        var c3 = new Card { Id = 3, Name = "card3", Owner = context.Users.Find(1), indexIntoBoardList = 1, BoardList = context.BoardLists.Find(1) };
        var c4 = new Card { Id = 4, Name = "card4", Owner = context.Users.Find(3), indexIntoBoardList = 0, BoardList = context.BoardLists.Find(2) };
        var c5 = new Card { Id = 5, Name = "card5", Owner = context.Users.Find(2), indexIntoBoardList = 0, BoardList = context.BoardLists.Find(5) };
        context.Cards.AddRange(c1, c2, c3, c4, c5);

        if (context.Database.IsSqlServer()) {
            context.Database.OpenConnection();
            try {
                context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Cards ON");
                context.SaveChanges();
                context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Cards OFF");
                context.SaveChanges();
            } finally {
                context.Database.CloseConnection();
            }
        }

        // Création des relations entre "Users" et "Cards"
        var uc1 = new UserCard(){ CardId = 5, UserId = 2 };
        var uc2 = new UserCard(){ CardId = 1, UserId = 3 };
        var uc3 = new UserCard(){ CardId = 3, UserId = 2 };
        context.UsersCardsRelation.AddRange(uc1, uc2, uc3);
        context.SaveChanges();


        var p1 = new Post { Id = 1, Text = "Post1", Card = context.Cards.Find(1) };
        var p2 = new Post { Id = 2, Text = "Post2", Card = context.Cards.Find(1) };
        var p3 = new Post { Id = 3, Text = "Post3", Card = context.Cards.Find(2) };
        context.Posts.AddRange(p1, p2, p3);
        if (context.Database.IsSqlServer()) {
            context.Database.OpenConnection();
            try {
                context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Posts ON");
                context.SaveChanges();
                context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Posts OFF");
                context.SaveChanges();
            } finally {
                context.Database.CloseConnection();
            }
        }

        var t1 = new Tag { Id = 1, Name = "Tag1", Card = context.Cards.Find(1) };
        var t2 = new Tag { Id = 2, Name = "Tag2", Card = context.Cards.Find(1) };
        var t3 = new Tag { Id = 3, Name = "Tag3", Card = context.Cards.Find(2) };
        context.Tags.AddRange(t1, t2, t3);
        if (context.Database.IsSqlServer()) {
            context.Database.OpenConnection();
            try {
                context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Tags ON");
                context.SaveChanges();
                context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Tags OFF");
                context.SaveChanges();
            } finally {
                context.Database.CloseConnection();
            }
        }

        // context.Users.Find(1).Boards.Add(context.Boards.Find(1));
        // context.Users.Find(1).Boards.Add(context.Boards.Find(2));
        // context.Users.Find(3).Boards.Add(context.Boards.Find(3));

        // context.Boards.Find(1).BoardLists.Add(context.BoardLists.Find(1));
        // context.Boards.Find(1).BoardLists.Add(context.BoardLists.Find(2));
        // context.Boards.Find(1).BoardLists.Add(context.BoardLists.Find(3));
        // context.Boards.Find(1).BoardLists.Add(context.BoardLists.Find(4));
        // context.Boards.Find(2).BoardLists.Add(context.BoardLists.Find(7));
        // context.Boards.Find(2).BoardLists.Add(context.BoardLists.Find(6));
        // context.Boards.Find(3).BoardLists.Add(context.BoardLists.Find(5));

        // context.BoardLists.Find(1).Cards.Add(context.Cards.Find(1));
        // context.BoardLists.Find(1).Cards.Add(context.Cards.Find(2));
        // context.BoardLists.Find(1).Cards.Add(context.Cards.Find(3));
        // context.BoardLists.Find(2).Cards.Add(context.Cards.Find(4));
        // context.BoardLists.Find(5).Cards.Add(context.Cards.Find(5));

        logger.LogInformation("Finished seeding the database.");
    }
}

