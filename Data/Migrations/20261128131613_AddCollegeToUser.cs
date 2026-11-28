using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IskolarTunes.Data.Migrations;

/// <inheritdoc />
public partial class _20261128131613_AddCollegeToUser : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "College",
            table: "AspNetUsers",
            type: "nvarchar(10)",
            maxLength: 10,
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "College",
            table: "AspNetUsers");
    }
}
