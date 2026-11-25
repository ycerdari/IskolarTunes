using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IskolarTunes.Data.Migrations;

/// <inheritdoc />
public partial class _20261125051451_AddTracksAndPlaylists : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Playlists",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                IsPublic = table.Column<bool>(type: "bit", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                OwnerId = table.Column<string>(type: "nvarchar(450)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Playlists", x => x.Id);
                table.ForeignKey(
                    name: "FK_Playlists_AspNetUsers_OwnerId",
                    column: x => x.OwnerId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Tracks",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                Genre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                AudioFilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                CoverImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UploaderId = table.Column<string>(type: "nvarchar(450)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Tracks", x => x.Id);
                table.ForeignKey(
                    name: "FK_Tracks_AspNetUsers_UploaderId",
                    column: x => x.UploaderId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "PlaylistTracks",
            columns: table => new
            {
                PlaylistId = table.Column<int>(type: "int", nullable: false),
                TrackId = table.Column<int>(type: "int", nullable: false),
                Position = table.Column<int>(type: "int", nullable: false),
                AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PlaylistTracks", x => new { x.PlaylistId, x.TrackId });
                table.ForeignKey(
                    name: "FK_PlaylistTracks_Playlists_PlaylistId",
                    column: x => x.PlaylistId,
                    principalTable: "Playlists",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_PlaylistTracks_Tracks_TrackId",
                    column: x => x.TrackId,
                    principalTable: "Tracks",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Playlists_OwnerId",
            table: "Playlists",
            column: "OwnerId");

        migrationBuilder.CreateIndex(
            name: "IX_PlaylistTracks_TrackId",
            table: "PlaylistTracks",
            column: "TrackId");

        migrationBuilder.CreateIndex(
            name: "IX_Tracks_UploaderId",
            table: "Tracks",
            column: "UploaderId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "PlaylistTracks");

        migrationBuilder.DropTable(
            name: "Playlists");

        migrationBuilder.DropTable(
            name: "Tracks");
    }
}
