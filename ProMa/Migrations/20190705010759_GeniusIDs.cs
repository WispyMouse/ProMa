using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace ProMa.Migrations
{
    public partial class GeniusIDs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_MusicJunkSongs_ArtistId_SongID",
                table: "MusicJunkSongs");

            migrationBuilder.RenameColumn(
                name: "SongID",
                table: "MusicJunkSongs",
                newName: "SongId");

            migrationBuilder.AddColumn<int>(
                name: "GeniusId",
                table: "MusicJunkSongs",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GeniusId",
                table: "MusicJunkArtists",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_MusicJunkSongs_ArtistId_SongId",
                table: "MusicJunkSongs",
                columns: new[] { "ArtistId", "SongId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_MusicJunkSongs_ArtistId_SongId",
                table: "MusicJunkSongs");

            migrationBuilder.DropColumn(
                name: "GeniusId",
                table: "MusicJunkSongs");

            migrationBuilder.DropColumn(
                name: "GeniusId",
                table: "MusicJunkArtists");

            migrationBuilder.RenameColumn(
                name: "SongId",
                table: "MusicJunkSongs",
                newName: "SongID");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_MusicJunkSongs_ArtistId_SongID",
                table: "MusicJunkSongs",
                columns: new[] { "ArtistId", "SongID" });
        }
    }
}
