using Microsoft.EntityFrameworkCore;

namespace prid_2021_g06.Models
{
    public class g06Context : DbContext
    {
        public g06Context(DbContextOptions<g06Context> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Board> Boards { get; set; }
        public DbSet<BoardList> BoardLists { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<UserBoard> UsersBoardsRelation { get; set; }
        public DbSet<UserCard> UsersCardsRelation { get; set; }
        public DbSet<Phone> Phones { get; set; }
        public DbSet<Post> Posts { get; set;}
        public DbSet<Tag> Tags { get; set;}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Pseudo)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<UserBoard>()
            .HasKey(bc => new { bc.BoardId, bc.UserId });
            modelBuilder.Entity<UserBoard>()
                .HasOne(bc => bc.User)
                .WithMany(b => b.UsersBoardsRelation)
                .HasForeignKey(bc => bc.UserId);
            modelBuilder.Entity<UserBoard>()
                .HasOne(bc => bc.Board)
                .WithMany(c => c.UsersBoardsRelation)
                .HasForeignKey(bc => bc.BoardId);

            modelBuilder.Entity<UserCard>()
            .HasKey(bc => new { bc.CardId, bc.UserId });
            modelBuilder.Entity<UserCard>()
                .HasOne(bc => bc.User)
                .WithMany(b => b.UsersCardsRelation)
                .HasForeignKey(bc => bc.UserId);
            modelBuilder.Entity<UserCard>()
                .HasOne(bc => bc.Card)
                .WithMany(c => c.UsersCardsRelation)
                .HasForeignKey(bc => bc.CardId);
        }

    }
}
