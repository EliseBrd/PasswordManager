using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PasswordManager.API.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailToAppUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "isShared",
                table: "Vaults",
                newName: "IsShared");

            migrationBuilder.RenameColumn(
                name: "encryptKey",
                table: "Vaults",
                newName: "EncryptKey");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Identifier",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "Email",
                value: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "IsShared",
                table: "Vaults",
                newName: "isShared");

            migrationBuilder.RenameColumn(
                name: "EncryptKey",
                table: "Vaults",
                newName: "encryptKey");
        }
    }
}
