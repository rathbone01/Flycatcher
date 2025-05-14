using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flycatcher.Migrations
{
    /// <inheritdoc />
    public partial class Updated_field : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SernderUserId",
                table: "ServerInvites");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SernderUserId",
                table: "ServerInvites",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
