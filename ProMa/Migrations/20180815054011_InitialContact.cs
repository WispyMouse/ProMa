using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace ProMa.Migrations
{
    public partial class InitialContact : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NoteTypes",
                columns: table => new
                {
                    NoteTypeId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Hibernated = table.Column<bool>(nullable: false),
                    NoteTypeName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoteTypes", x => x.NoteTypeId);
                });

            migrationBuilder.CreateTable(
                name: "ProMaUsers",
                columns: table => new
                {
                    UserId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    EmailAddress = table.Column<string>(nullable: true),
                    EnterIsNewLinePref = table.Column<bool>(nullable: false),
                    HashedPassword = table.Column<string>(nullable: true),
                    IsAdmin = table.Column<bool>(nullable: false),
                    IsDemo = table.Column<bool>(nullable: false),
                    JoinTime = table.Column<DateTimeOffset>(nullable: false),
                    UserName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProMaUsers", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "SharedChores",
                columns: table => new
                {
                    SharedChoreId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ChoreName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedChores", x => x.SharedChoreId);
                });

            migrationBuilder.CreateTable(
                name: "CalendarEntries",
                columns: table => new
                {
                    CalendarId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CalendarName = table.Column<string>(nullable: true),
                    ForDate = table.Column<DateTimeOffset>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    Yearly = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarEntries", x => x.CalendarId);
                    table.ForeignKey(
                        name: "FK_dbo.CalendarEntries_dbo.ProMaUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "ProMaUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FriendshipRequests",
                columns: table => new
                {
                    SenderId = table.Column<int>(nullable: false),
                    RecipientId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FriendshipRequests", x => new { x.SenderId, x.RecipientId });
                    table.UniqueConstraint("AK_FriendshipRequests_RecipientId_SenderId", x => new { x.RecipientId, x.SenderId });
                    table.ForeignKey(
                        name: "FK_dbo.FriendshipRequests_dbo.ProMaUsers_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "ProMaUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_dbo.FriendshipRequests_dbo.ProMaUsers_SenderId",
                        column: x => x.SenderId,
                        principalTable: "ProMaUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Friendships",
                columns: table => new
                {
                    MemberOneId = table.Column<int>(nullable: false),
                    MemberTwoId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Friendships", x => new { x.MemberOneId, x.MemberTwoId });
                    table.ForeignKey(
                        name: "FK_dbo.Friendships_dbo.ProMaUsers_MemberOneId",
                        column: x => x.MemberOneId,
                        principalTable: "ProMaUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_dbo.Friendships_dbo.ProMaUsers_MemberTwoId",
                        column: x => x.MemberTwoId,
                        principalTable: "ProMaUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NoteTypeMemberships",
                columns: table => new
                {
                    NoteTypeId = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    CanUseNotes = table.Column<bool>(nullable: false),
                    IsCreator = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoteTypeMemberships", x => new { x.NoteTypeId, x.UserId });
                    table.ForeignKey(
                        name: "FK_dbo.NoteTypeMemberships_dbo.NoteTypes_NoteTypeId",
                        column: x => x.NoteTypeId,
                        principalTable: "NoteTypes",
                        principalColumn: "NoteTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_dbo.NoteTypeMemberships_dbo.ProMaUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "ProMaUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PostedNotes",
                columns: table => new
                {
                    NoteId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Active = table.Column<bool>(nullable: false, defaultValueSql: "((1))"),
                    Completed = table.Column<bool>(nullable: false),
                    CompletedTime = table.Column<DateTimeOffset>(nullable: true),
                    CompletedUserId = table.Column<int>(nullable: true),
                    EditedTime = table.Column<DateTimeOffset>(nullable: true),
                    EditedUserId = table.Column<int>(nullable: true),
                    Highlighted = table.Column<bool>(nullable: false),
                    NoteText = table.Column<string>(nullable: true),
                    NoteTitle = table.Column<string>(nullable: true),
                    NoteTypeId = table.Column<int>(nullable: true),
                    PostedTime = table.Column<DateTimeOffset>(nullable: false),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostedNotes", x => x.NoteId);
                    table.ForeignKey(
                        name: "FK_dbo.PostedNotes_dbo.ProMaUsers_CompletedUserId",
                        column: x => x.CompletedUserId,
                        principalTable: "ProMaUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_dbo.PostedNotes_dbo.ProMaUsers_EditedUserId",
                        column: x => x.EditedUserId,
                        principalTable: "ProMaUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_dbo.PostedNotes_dbo.NoteTypes_NoteTypeId",
                        column: x => x.NoteTypeId,
                        principalTable: "NoteTypes",
                        principalColumn: "NoteTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_dbo.PostedNotes_dbo.ProMaUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "ProMaUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompletedChores",
                columns: table => new
                {
                    ChoreDate = table.Column<DateTime>(type: "date", nullable: false),
                    SharedChoreId = table.Column<int>(nullable: false),
                    Completed = table.Column<bool>(nullable: false),
                    PostedTime = table.Column<DateTimeOffset>(nullable: true, defaultValueSql: "('0001-01-01T00:00:00.000+00:00')"),
                    UserId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompletedChores", x => new { x.ChoreDate, x.SharedChoreId });
                    table.ForeignKey(
                        name: "FK_dbo.ChoreItems_dbo.SharedChores_SharedChoreId",
                        column: x => x.SharedChoreId,
                        principalTable: "SharedChores",
                        principalColumn: "SharedChoreId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompletedChores_ProMaUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "ProMaUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SharedChoreMemberships",
                columns: table => new
                {
                    SharedChoreId = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    AlertHour = table.Column<int>(nullable: true),
                    AlertMinute = table.Column<int>(nullable: true),
                    PersonalSortingOrder = table.Column<int>(nullable: false, defaultValueSql: "((0))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedChoreMemberships", x => new { x.SharedChoreId, x.UserId });
                    table.ForeignKey(
                        name: "FK_dbo.SharedChoreMemberships_dbo.SharedChores_SharedChoreId",
                        column: x => x.SharedChoreId,
                        principalTable: "SharedChores",
                        principalColumn: "SharedChoreId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_dbo.SharedChoreMemberships_dbo.ProMaUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "ProMaUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserId",
                table: "CalendarEntries",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedChoreId",
                table: "CompletedChores",
                column: "SharedChoreId");

            migrationBuilder.CreateIndex(
                name: "IX_CompletedChores_UserId",
                table: "CompletedChores",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipientId",
                table: "FriendshipRequests",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_SenderId",
                table: "FriendshipRequests",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberOneId",
                table: "Friendships",
                column: "MemberOneId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberTwoId",
                table: "Friendships",
                column: "MemberTwoId");

            migrationBuilder.CreateIndex(
                name: "IX_NoteTypeId",
                table: "NoteTypeMemberships",
                column: "NoteTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserId",
                table: "NoteTypeMemberships",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CompletedUserId",
                table: "PostedNotes",
                column: "CompletedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EditedUserId",
                table: "PostedNotes",
                column: "EditedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_NoteTypeId",
                table: "PostedNotes",
                column: "NoteTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserId",
                table: "PostedNotes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedChoreId",
                table: "SharedChoreMemberships",
                column: "SharedChoreId");

            migrationBuilder.CreateIndex(
                name: "IX_UserId",
                table: "SharedChoreMemberships",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalendarEntries");

            migrationBuilder.DropTable(
                name: "CompletedChores");

            migrationBuilder.DropTable(
                name: "FriendshipRequests");

            migrationBuilder.DropTable(
                name: "Friendships");

            migrationBuilder.DropTable(
                name: "NoteTypeMemberships");

            migrationBuilder.DropTable(
                name: "PostedNotes");

            migrationBuilder.DropTable(
                name: "SharedChoreMemberships");

            migrationBuilder.DropTable(
                name: "NoteTypes");

            migrationBuilder.DropTable(
                name: "SharedChores");

            migrationBuilder.DropTable(
                name: "ProMaUsers");
        }
    }
}
