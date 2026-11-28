using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IskolarTunes.Data.Migrations;

/// <inheritdoc />
public partial class _20261126144342_AddLikesAndPlays : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "PlayCount",
            table: "Tracks",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.CreateTable(
            name: "TrackLikes",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                TrackId = table.Column<int>(type: "int", nullable: false),
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                LikedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TrackLikes", x => x.Id);
                table.ForeignKey(
                    name: "FK_TrackLikes_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_TrackLikes_Tracks_TrackId",
                    column: x => x.TrackId,
                    principalTable: "Tracks",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "TrackPlays",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                TrackId = table.Column<int>(type: "int", nullable: false),
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                PlayedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TrackPlays", x => x.Id);
                table.ForeignKey(
                    name: "FK_TrackPlays_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_TrackPlays_Tracks_TrackId",
                    column: x => x.TrackId,
                    principalTable: "Tracks",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_TrackLikes_TrackId",
            table: "TrackLikes",
            column: "TrackId");

        migrationBuilder.CreateIndex(
            name: "IX_TrackLikes_UserId_TrackId",
            table: "TrackLikes",
            columns: new[] { "UserId", "TrackId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_TrackPlays_TrackId_PlayedAt",
            table: "TrackPlays",
            columns: new[] { "TrackId", "PlayedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_TrackPlays_UserId",
            table: "TrackPlays",
            column: "UserId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "TrackLikes");

        migrationBuilder.DropTable(
            name: "TrackPlays");

        migrationBuilder.DropColumn(
            name: "PlayCount",
            table: "Tracks");
    }
}
