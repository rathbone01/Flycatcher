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

            modelBuilder.Entity<Role>()
                .HasOne(r => r.Server)
                .WithMany()
                .HasForeignKey(r => r.ServerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RolePermissions>()
                .HasOne(rp => rp.Role)
                .WithOne(r => r.Permissions)
                .HasForeignKey<RolePermissions>(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany()
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRole>()
                .HasIndex(ur => new { ur.UserId, ur.RoleId })
                .IsUnique();

            modelBuilder.Entity<ChannelRolePermission>()
                .HasOne(crp => crp.Channel)
                .WithMany()
                .HasForeignKey(crp => crp.ChannelId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChannelRolePermission>()
                .HasOne(crp => crp.Role)
                .WithMany(r => r.ChannelRolePermissions)
                .HasForeignKey(crp => crp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChannelRolePermission>()
                .HasIndex(crp => new { crp.ChannelId, crp.RoleId })
                .IsUnique();

            modelBuilder.Entity<UserReport>()
                .HasOne(ur => ur.ReportedUser)
                .WithMany()
                .HasForeignKey(ur => ur.ReportedUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserReport>()
                .HasOne(ur => ur.Reporter)
                .WithMany()
                .HasForeignKey(ur => ur.ReporterUserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserReport>()
                .HasOne(ur => ur.ReviewedByAdmin)
                .WithMany()
                .HasForeignKey(ur => ur.ReviewedByAdminUserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserBan>()
                .HasOne(ub => ub.User)
                .WithMany()
                .HasForeignKey(ub => ub.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserBan>()
                .HasOne(ub => ub.BannedByAdmin)
                .WithMany()
                .HasForeignKey(ub => ub.BannedByAdminUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserBan>()
                .HasOne(ub => ub.AppealReviewedByAdmin)
                .WithMany()
                .HasForeignKey(ub => ub.AppealReviewedByAdminUserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserTimeout>()
                .HasOne(ut => ut.User)
                .WithMany()
                .HasForeignKey(ut => ut.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserTimeout>()
                .HasOne(ut => ut.Server)
                .WithMany()
                .HasForeignKey(ut => ut.ServerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserTimeout>()
                .HasOne(ut => ut.TimeoutByUser)
                .WithMany()
                .HasForeignKey(ut => ut.TimeoutByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.DeletedByUser)
                .WithMany()
                .HasForeignKey(m => m.DeletedByUserId)
                .IsRequired(false)
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
        public DbSet<Role> Roles { get; set; }
        public DbSet<RolePermissions> RolePermissions { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<ChannelRolePermission> ChannelRolePermissions { get; set; }
        public DbSet<UserReport> UserReports { get; set; }
        public DbSet<UserBan> UserBans { get; set; }
        public DbSet<UserTimeout> UserTimeouts { get; set; }
    }
}
