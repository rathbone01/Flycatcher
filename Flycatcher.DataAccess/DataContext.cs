using Flycatcher.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace Flycatcher.DataAccess
{
    public class DataContext : DbContext
    {
        public DataContext()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

        }

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserFriend>()
                .ToTable(t => t.HasCheckConstraint("CK_UserFriend_UserId_NotEqual_FriendId", "[UserId] <> [FriendId]"));

            modelBuilder.Entity<UserFriend>()
                .HasOne(uf => uf.User)
                .WithMany(u => u.UserFriends)
                .HasForeignKey(uf => uf.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserFriend>()
                .HasOne(uf => uf.Friend)
                .WithMany()
                .HasForeignKey(uf => uf.FriendId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<User>()
                .HasMany(u => u.SentFriendRequests)
                .WithOne(fr => fr.Sender)
                .HasForeignKey(fr => fr.SenderId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<User>()
                .HasMany(u => u.ReceivedFriendRequests)
                .WithOne(fr => fr.Receiver)
                .HasForeignKey(fr => fr.ReceiverId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<FriendRequest>()
                .HasOne(fr => fr.Sender)
                .WithMany(u => u.SentFriendRequests)
                .HasForeignKey(fr => fr.SenderId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<FriendRequest>()
                .HasOne(fr => fr.Receiver)
                .WithMany(u => u.ReceivedFriendRequests)
                .HasForeignKey(fr => fr.ReceiverId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ServerInvite>()
                .HasOne(si => si.SenderUser)
                .WithMany()
                .HasForeignKey(si => si.SenderUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<DirectMessage>()
                .HasOne(dm => dm.Sender)
                .WithMany()
                .HasForeignKey(dm => dm.SenderId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<DirectMessage>()
                .HasOne(dm => dm.Receiver)
                .WithMany()
                .HasForeignKey(dm => dm.ReceiverId)
                .OnDelete(DeleteBehavior.NoAction);

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<SiteAdmin> SiteAdmins { get; set; }
        public DbSet<Channel> Channels { get; set; }
        public DbSet<UserFriend> UserFriends { get; set; }
        public DbSet<UserServer> UserServers { get; set; }
        public DbSet<Server> Servers { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<ServerInvite> ServerInvites { get; set; }
        public DbSet<FriendRequest> FriendRequests { get; set; }
        public DbSet<DirectMessage> DirectMessages { get; set; }
    }
}
