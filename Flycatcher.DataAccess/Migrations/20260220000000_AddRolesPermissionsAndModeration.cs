using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flycatcher.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddRolesPermissionsAndModeration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "Messages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedByUserId",
                table: "Messages",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServerId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    ColorHex = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Position = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Roles_Servers_ServerId",
                        column: x => x.ServerId,
                        principalTable: "Servers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    SendMessages = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleteOthersMessages = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    TimeoutUser = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    BanUser = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    EditChannels = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AddChannels = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    EditServerSettings = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ManageRoles = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AssignRoles = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserBans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    BannedByAdminUserId = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    BannedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AppealStatus = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    AppealReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AppealSubmittedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AppealReviewedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AppealReviewedByAdminUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserBans_Users_AppealReviewedByAdminUserId",
                        column: x => x.AppealReviewedByAdminUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserBans_Users_BannedByAdminUserId",
                        column: x => x.BannedByAdminUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserBans_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportedUserId = table.Column<int>(type: "int", nullable: false),
                    ReporterUserId = table.Column<int>(type: "int", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedByAdminUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserReports_Users_ReportedUserId",
                        column: x => x.ReportedUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserReports_Users_ReporterUserId",
                        column: x => x.ReporterUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserReports_Users_ReviewedByAdminUserId",
                        column: x => x.ReviewedByAdminUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserTimeouts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ServerId = table.Column<int>(type: "int", nullable: false),
                    TimeoutByUserId = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TimeoutAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTimeouts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserTimeouts_Servers_ServerId",
                        column: x => x.ServerId,
                        principalTable: "Servers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserTimeouts_Users_TimeoutByUserId",
                        column: x => x.TimeoutByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserTimeouts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ChannelRolePermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChannelId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    SendMessages = table.Column<bool>(type: "bit", nullable: true),
                    DeleteOthersMessages = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelRolePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChannelRolePermissions_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChannelRolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    AssignedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_DeletedByUserId",
                table: "Messages",
                column: "DeletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelRolePermissions_ChannelId_RoleId",
                table: "ChannelRolePermissions",
                columns: new[] { "ChannelId", "RoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChannelRolePermissions_RoleId",
                table: "ChannelRolePermissions",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_ServerId",
                table: "Roles",
                column: "ServerId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId",
                table: "RolePermissions",
                column: "RoleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserBans_AppealReviewedByAdminUserId",
                table: "UserBans",
                column: "AppealReviewedByAdminUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBans_BannedByAdminUserId",
                table: "UserBans",
                column: "BannedByAdminUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBans_UserId",
                table: "UserBans",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserReports_ReportedUserId",
                table: "UserReports",
                column: "ReportedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserReports_ReporterUserId",
                table: "UserReports",
                column: "ReporterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserReports_ReviewedByAdminUserId",
                table: "UserReports",
                column: "ReviewedByAdminUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId_RoleId",
                table: "UserRoles",
                columns: new[] { "UserId", "RoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserTimeouts_ServerId",
                table: "UserTimeouts",
                column: "ServerId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTimeouts_TimeoutByUserId",
                table: "UserTimeouts",
                column: "TimeoutByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTimeouts_UserId",
                table: "UserTimeouts",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Users_DeletedByUserId",
                table: "Messages",
                column: "DeletedByUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Users_DeletedByUserId",
                table: "Messages");

            migrationBuilder.DropTable(
                name: "ChannelRolePermissions");

            migrationBuilder.DropTable(
                name: "UserBans");

            migrationBuilder.DropTable(
                name: "UserReports");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "UserTimeouts");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Messages_DeletedByUserId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Messages");
        }
    }
}
