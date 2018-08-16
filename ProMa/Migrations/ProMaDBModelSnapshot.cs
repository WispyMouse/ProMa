﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using ProMa.Models;
using System;

namespace ProMa.Migrations
{
    [DbContext(typeof(ProMaDB))]
    partial class ProMaDBModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.1-rtm-125")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("ProMa.Models.CalendarEntry", b =>
                {
                    b.Property<int>("CalendarId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CalendarName");

                    b.Property<DateTimeOffset>("ForDate");

                    b.Property<int>("UserId");

                    b.Property<bool>("Yearly");

                    b.HasKey("CalendarId");

                    b.HasIndex("UserId")
                        .HasName("IX_UserId");

                    b.ToTable("CalendarEntries");
                });

            modelBuilder.Entity("ProMa.Models.CompletedChore", b =>
                {
                    b.Property<DateTime>("ChoreDate")
                        .HasColumnType("date");

                    b.Property<int>("SharedChoreId");

                    b.Property<bool>("Completed");

                    b.Property<DateTimeOffset?>("PostedTime")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("('0001-01-01T00:00:00.000+00:00')");

                    b.Property<int?>("UserId");

                    b.HasKey("ChoreDate", "SharedChoreId");

                    b.HasIndex("SharedChoreId")
                        .HasName("IX_SharedChoreId");

                    b.HasIndex("UserId");

                    b.ToTable("CompletedChores");
                });

            modelBuilder.Entity("ProMa.Models.Friendship", b =>
                {
                    b.Property<int>("MemberOneId");

                    b.Property<int>("MemberTwoId");

                    b.HasKey("MemberOneId", "MemberTwoId");

                    b.HasIndex("MemberOneId")
                        .HasName("IX_MemberOneId");

                    b.HasIndex("MemberTwoId")
                        .HasName("IX_MemberTwoId");

                    b.ToTable("Friendships");
                });

            modelBuilder.Entity("ProMa.Models.FriendshipRequest", b =>
                {
                    b.Property<int>("SenderId");

                    b.Property<int>("RecipientId");

                    b.HasKey("SenderId", "RecipientId");

                    b.HasAlternateKey("RecipientId", "SenderId");

                    b.HasIndex("RecipientId")
                        .HasName("IX_RecipientId");

                    b.HasIndex("SenderId")
                        .HasName("IX_SenderId");

                    b.ToTable("FriendshipRequests");
                });

            modelBuilder.Entity("ProMa.Models.NoteType", b =>
                {
                    b.Property<int>("NoteTypeId")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("Hibernated");

                    b.Property<string>("NoteTypeName");

                    b.HasKey("NoteTypeId");

                    b.ToTable("NoteTypes");
                });

            modelBuilder.Entity("ProMa.Models.NoteTypeMembership", b =>
                {
                    b.Property<int>("NoteTypeId");

                    b.Property<int>("UserId");

                    b.Property<bool>("CanUseNotes");

                    b.Property<bool>("IsCreator");

                    b.HasKey("NoteTypeId", "UserId");

                    b.HasIndex("NoteTypeId")
                        .HasName("IX_NoteTypeId");

                    b.HasIndex("UserId")
                        .HasName("IX_UserId");

                    b.ToTable("NoteTypeMemberships");
                });

            modelBuilder.Entity("ProMa.Models.PostedNote", b =>
                {
                    b.Property<int>("NoteId")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("Active")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("((1))");

                    b.Property<bool>("Completed");

                    b.Property<DateTimeOffset?>("CompletedTime");

                    b.Property<int?>("CompletedUserId");

                    b.Property<DateTimeOffset?>("EditedTime");

                    b.Property<int?>("EditedUserId");

                    b.Property<bool>("Highlighted");

                    b.Property<string>("NoteText");

                    b.Property<string>("NoteTitle");

                    b.Property<int?>("NoteTypeId");

                    b.Property<DateTimeOffset>("PostedTime");

                    b.Property<int>("UserId");

                    b.HasKey("NoteId");

                    b.HasIndex("CompletedUserId")
                        .HasName("IX_CompletedUserId");

                    b.HasIndex("EditedUserId")
                        .HasName("IX_EditedUserId");

                    b.HasIndex("NoteTypeId")
                        .HasName("IX_NoteTypeId");

                    b.HasIndex("UserId")
                        .HasName("IX_UserId");

                    b.ToTable("PostedNotes");
                });

            modelBuilder.Entity("ProMa.Models.ProMaUser", b =>
                {
                    b.Property<int>("UserId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("EmailAddress");

                    b.Property<bool>("EnterIsNewLinePref");

                    b.Property<string>("HashedPassword");

                    b.Property<bool>("IsAdmin");

                    b.Property<bool>("IsDemo");

                    b.Property<DateTimeOffset>("JoinTime");

                    b.Property<string>("UserName");

                    b.HasKey("UserId");

                    b.ToTable("ProMaUsers");
                });

            modelBuilder.Entity("ProMa.Models.SharedChore", b =>
                {
                    b.Property<int>("SharedChoreId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ChoreName");

                    b.HasKey("SharedChoreId");

                    b.ToTable("SharedChores");
                });

            modelBuilder.Entity("ProMa.Models.SharedChoreMembership", b =>
                {
                    b.Property<int>("SharedChoreId");

                    b.Property<int>("UserId");

                    b.Property<int?>("AlertHour");

                    b.Property<int?>("AlertMinute");

                    b.Property<int>("PersonalSortingOrder")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("((0))");

                    b.HasKey("SharedChoreId", "UserId");

                    b.HasIndex("SharedChoreId")
                        .HasName("IX_SharedChoreId");

                    b.HasIndex("UserId")
                        .HasName("IX_UserId");

                    b.ToTable("SharedChoreMemberships");
                });

            modelBuilder.Entity("ProMa.Models.CalendarEntry", b =>
                {
                    b.HasOne("ProMa.Models.ProMaUser", "PostedUser")
                        .WithMany("CalendarEntries")
                        .HasForeignKey("UserId")
                        .HasConstraintName("FK_dbo.CalendarEntries_dbo.ProMaUsers_UserId")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("ProMa.Models.CompletedChore", b =>
                {
                    b.HasOne("ProMa.Models.SharedChore", "SharedChore")
                        .WithMany("CompletedChores")
                        .HasForeignKey("SharedChoreId")
                        .HasConstraintName("FK_dbo.ChoreItems_dbo.SharedChores_SharedChoreId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("ProMa.Models.ProMaUser", "CompletedUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("ProMa.Models.Friendship", b =>
                {
                    b.HasOne("ProMa.Models.ProMaUser", "MemberOne")
                        .WithMany("FriendshipsMemberOne")
                        .HasForeignKey("MemberOneId")
                        .HasConstraintName("FK_dbo.Friendships_dbo.ProMaUsers_MemberOneId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("ProMa.Models.ProMaUser", "MemberTwo")
                        .WithMany("FriendshipsMemberTwo")
                        .HasForeignKey("MemberTwoId")
                        .HasConstraintName("FK_dbo.Friendships_dbo.ProMaUsers_MemberTwoId")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("ProMa.Models.FriendshipRequest", b =>
                {
                    b.HasOne("ProMa.Models.ProMaUser", "Recipient")
                        .WithMany("FriendshipRequestsRecipient")
                        .HasForeignKey("RecipientId")
                        .HasConstraintName("FK_dbo.FriendshipRequests_dbo.ProMaUsers_RecipientId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("ProMa.Models.ProMaUser", "Sender")
                        .WithMany("FriendshipRequestsSender")
                        .HasForeignKey("SenderId")
                        .HasConstraintName("FK_dbo.FriendshipRequests_dbo.ProMaUsers_SenderId")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("ProMa.Models.NoteTypeMembership", b =>
                {
                    b.HasOne("ProMa.Models.NoteType", "NoteType")
                        .WithMany("NoteTypeMemberships")
                        .HasForeignKey("NoteTypeId")
                        .HasConstraintName("FK_dbo.NoteTypeMemberships_dbo.NoteTypes_NoteTypeId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("ProMa.Models.ProMaUser", "MemberUser")
                        .WithMany("NoteTypeMemberships")
                        .HasForeignKey("UserId")
                        .HasConstraintName("FK_dbo.NoteTypeMemberships_dbo.ProMaUsers_UserId")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("ProMa.Models.PostedNote", b =>
                {
                    b.HasOne("ProMa.Models.ProMaUser", "CompletedUser")
                        .WithMany("PostedNotesCompletedUser")
                        .HasForeignKey("CompletedUserId")
                        .HasConstraintName("FK_dbo.PostedNotes_dbo.ProMaUsers_CompletedUserId");

                    b.HasOne("ProMa.Models.ProMaUser", "EditedUser")
                        .WithMany("PostedNotesEditedUser")
                        .HasForeignKey("EditedUserId")
                        .HasConstraintName("FK_dbo.PostedNotes_dbo.ProMaUsers_EditedUserId");

                    b.HasOne("ProMa.Models.NoteType", "TypeOfNote")
                        .WithMany("PostedNotes")
                        .HasForeignKey("NoteTypeId")
                        .HasConstraintName("FK_dbo.PostedNotes_dbo.NoteTypes_NoteTypeId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("ProMa.Models.ProMaUser", "PostedUser")
                        .WithMany("PostedNotesUser")
                        .HasForeignKey("UserId")
                        .HasConstraintName("FK_dbo.PostedNotes_dbo.ProMaUsers_UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ProMa.Models.SharedChoreMembership", b =>
                {
                    b.HasOne("ProMa.Models.SharedChore", "SharedChore")
                        .WithMany("SharedChoreMemberships")
                        .HasForeignKey("SharedChoreId")
                        .HasConstraintName("FK_dbo.SharedChoreMemberships_dbo.SharedChores_SharedChoreId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("ProMa.Models.ProMaUser", "MemberUser")
                        .WithMany("SharedChoreMemberships")
                        .HasForeignKey("UserId")
                        .HasConstraintName("FK_dbo.SharedChoreMemberships_dbo.ProMaUsers_UserId")
                        .OnDelete(DeleteBehavior.Restrict);
                });
#pragma warning restore 612, 618
        }
    }
}