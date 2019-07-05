using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace ProMa.Migrations
{
    public partial class ArtistsSongs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MusicJunkArtists",
                columns: table => new
                {
                    ArtistId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ArtistName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicJunkArtists", x => x.ArtistId);
                });

            migrationBuilder.CreateTable(
                name: "MusicJunkSongs",
                columns: table => new
                {
                    SongID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ArtistId = table.Column<int>(nullable: false),
                    Lyrics = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicJunkSongs", x => x.SongID);
                    table.UniqueConstraint("AK_MusicJunkSongs_ArtistId_SongID", x => new { x.ArtistId, x.SongID });
                    table.ForeignKey(
                        name: "FK_dbo.MusicJunkSongs_dbo.MusicJunkArtists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "MusicJunkArtists",
                        principalColumn: "ArtistId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArtistId",
                table: "MusicJunkSongs",
                column: "ArtistId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MusicJunkSongs");

            migrationBuilder.DropTable(
                name: "MusicJunkArtists");
        }
    }
}
