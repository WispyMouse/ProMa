using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace ProMa.Models
{
    public partial class ProMaDB : DbContext
    {
		static public IConfigurationRoot Configuration { get; set; }

		public virtual DbSet<CalendarEntry> CalendarEntries { get; set; }
        public virtual DbSet<CompletedChore> CompletedChores { get; set; }
        public virtual DbSet<FriendshipRequest> FriendshipRequests { get; set; }
        public virtual DbSet<Friendship> Friendships { get; set; }
        public virtual DbSet<NoteTypeMembership> NoteTypeMemberships { get; set; }
        public virtual DbSet<NoteType> NoteTypes { get; set; }
        public virtual DbSet<PostedNote> PostedNotes { get; set; }
        public virtual DbSet<ProMaUser> ProMaUsers { get; set; }
        public virtual DbSet<SharedChoreMembership> SharedChoreMemberships { get; set; }
        public virtual DbSet<SharedChore> SharedChores { get; set; }

        // Unable to generate entity type for table 'dbo.Menu'. Please see the warning messages.

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
				IConfigurationBuilder builder = new ConfigurationBuilder()
					.SetBasePath(Directory.GetCurrentDirectory())
					.AddJsonFile("localsettings.json");

				Configuration = builder.Build();

				optionsBuilder.UseSqlServer(Configuration.GetConnectionString("ProMaDb"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CalendarEntry>(entity =>
            {
                entity.HasKey(e => e.CalendarId);

                entity.HasIndex(e => e.UserId)
                    .HasName("IX_UserId");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.CalendarEntries)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_dbo.CalendarEntries_dbo.ProMaUsers_UserId");
            });

            modelBuilder.Entity<CompletedChore>(entity =>
            {
                entity.HasKey(e => new { e.ChoreDate, e.SharedChoreId });

                entity.HasIndex(e => e.SharedChoreId)
                    .HasName("IX_SharedChoreId");

                entity.Property(e => e.ChoreDate).HasColumnType("date");

                entity.Property(e => e.PostedTime).HasDefaultValueSql("('0001-01-01T00:00:00.000+00:00')");

                entity.HasOne(d => d.SharedChore)
                    .WithMany(p => p.CompletedChores)
                    .HasForeignKey(d => d.SharedChoreId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_dbo.ChoreItems_dbo.SharedChores_SharedChoreId");
            });

            modelBuilder.Entity<FriendshipRequest>(entity =>
            {
                entity.HasKey(e => new { e.SenderId, e.RecipientId });

                entity.HasIndex(e => e.RecipientId)
                    .HasName("IX_RecipientId");

                entity.HasIndex(e => e.SenderId)
                    .HasName("IX_SenderId");

                entity.HasOne(d => d.Recipient)
                    .WithMany(p => p.FriendshipRequestsRecipient)
                    .HasForeignKey(d => d.RecipientId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_dbo.FriendshipRequests_dbo.ProMaUsers_RecipientId");

                entity.HasOne(d => d.Sender)
                    .WithMany(p => p.FriendshipRequestsSender)
                    .HasForeignKey(d => d.SenderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_dbo.FriendshipRequests_dbo.ProMaUsers_SenderId");
            });

            modelBuilder.Entity<Friendship>(entity =>
            {
                entity.HasKey(e => new { e.MemberOneId, e.MemberTwoId });

                entity.HasIndex(e => e.MemberOneId)
                    .HasName("IX_MemberOneId");

                entity.HasIndex(e => e.MemberTwoId)
                    .HasName("IX_MemberTwoId");

                entity.HasOne(d => d.MemberOne)
                    .WithMany(p => p.FriendshipsMemberOne)
                    .HasForeignKey(d => d.MemberOneId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_dbo.Friendships_dbo.ProMaUsers_MemberOneId");

                entity.HasOne(d => d.MemberTwo)
                    .WithMany(p => p.FriendshipsMemberTwo)
                    .HasForeignKey(d => d.MemberTwoId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_dbo.Friendships_dbo.ProMaUsers_MemberTwoId");
            });

            modelBuilder.Entity<NoteTypeMembership>(entity =>
            {
                entity.HasKey(e => new { e.NoteTypeId, e.UserId });

                entity.HasIndex(e => e.NoteTypeId)
                    .HasName("IX_NoteTypeId");

                entity.HasIndex(e => e.UserId)
                    .HasName("IX_UserId");

                entity.HasOne(d => d.NoteType)
                    .WithMany(p => p.NoteTypeMemberships)
                    .HasForeignKey(d => d.NoteTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_dbo.NoteTypeMemberships_dbo.NoteTypes_NoteTypeId");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.NoteTypeMemberships)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_dbo.NoteTypeMemberships_dbo.ProMaUsers_UserId");
            });

            modelBuilder.Entity<NoteType>(entity =>
            {
                entity.HasKey(e => e.NoteTypeId);
            });

            modelBuilder.Entity<PostedNote>(entity =>
            {
                entity.HasKey(e => e.NoteId);

                entity.HasIndex(e => e.CompletedUserId)
                    .HasName("IX_CompletedUserId");

                entity.HasIndex(e => e.EditedUserId)
                    .HasName("IX_EditedUserId");

                entity.HasIndex(e => e.NoteTypeId)
                    .HasName("IX_NoteTypeId");

                entity.HasIndex(e => e.UserId)
                    .HasName("IX_UserId");

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.HasOne(d => d.CompletedUser)
                    .WithMany(p => p.PostedNotesCompletedUser)
                    .HasForeignKey(d => d.CompletedUserId)
                    .HasConstraintName("FK_dbo.PostedNotes_dbo.ProMaUsers_CompletedUserId");

                entity.HasOne(d => d.EditedUser)
                    .WithMany(p => p.PostedNotesEditedUser)
                    .HasForeignKey(d => d.EditedUserId)
                    .HasConstraintName("FK_dbo.PostedNotes_dbo.ProMaUsers_EditedUserId");

                entity.HasOne(d => d.NoteType)
                    .WithMany(p => p.PostedNotes)
                    .HasForeignKey(d => d.NoteTypeId)
                    .HasConstraintName("FK_dbo.PostedNotes_dbo.NoteTypes_NoteTypeId");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.PostedNotesUser)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_dbo.PostedNotes_dbo.ProMaUsers_UserId");
            });

            modelBuilder.Entity<ProMaUser>(entity =>
            {
                entity.HasKey(e => e.UserId);
            });

            modelBuilder.Entity<SharedChoreMembership>(entity =>
            {
                entity.HasKey(e => new { e.SharedChoreId, e.UserId });

                entity.HasIndex(e => e.SharedChoreId)
                    .HasName("IX_SharedChoreId");

                entity.HasIndex(e => e.UserId)
                    .HasName("IX_UserId");

                entity.Property(e => e.PersonalSortingOrder).HasDefaultValueSql("((0))");

                entity.HasOne(d => d.SharedChore)
                    .WithMany(p => p.SharedChoreMemberships)
                    .HasForeignKey(d => d.SharedChoreId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_dbo.SharedChoreMemberships_dbo.SharedChores_SharedChoreId");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.SharedChoreMemberships)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_dbo.SharedChoreMemberships_dbo.ProMaUsers_UserId");
            });

            modelBuilder.Entity<SharedChore>(entity =>
            {
                entity.HasKey(e => e.SharedChoreId);
            });
        }
    }
}
