using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Skillock.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PartyMembers_Users_UserId1",
                table: "PartyMembers");

            migrationBuilder.DropIndex(
                name: "IX_PartyMembers_UserId1",
                table: "PartyMembers");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "PartyMembers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "PartyMembers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartyMembers_UserId1",
                table: "PartyMembers",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_PartyMembers_Users_UserId1",
                table: "PartyMembers",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
