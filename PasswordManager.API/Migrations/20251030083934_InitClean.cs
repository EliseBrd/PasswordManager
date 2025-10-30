using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PasswordManager.API.Migrations
{
    /// <inheritdoc />
    public partial class InitClean : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Identifier = table.Column<Guid>(type: "TEXT", nullable: false),
                    entraId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Identifier);
                });

            migrationBuilder.CreateTable(
                name: "Vaults",
                columns: table => new
                {
                    Identifier = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatorIdentifier = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastUpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    isShared = table.Column<bool>(type: "INTEGER", nullable: false),
                    MasterSalt = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Salt = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    encryptKey = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    Password = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vaults", x => x.Identifier);
                    table.ForeignKey(
                        name: "FK_Vaults_Users_CreatorIdentifier",
                        column: x => x.CreatorIdentifier,
                        principalTable: "Users",
                        principalColumn: "Identifier",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppUserVault",
                columns: table => new
                {
                    SharedUsersIdentifier = table.Column<Guid>(type: "TEXT", nullable: false),
                    SharedVaultsIdentifier = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUserVault", x => new { x.SharedUsersIdentifier, x.SharedVaultsIdentifier });
                    table.ForeignKey(
                        name: "FK_AppUserVault_Users_SharedUsersIdentifier",
                        column: x => x.SharedUsersIdentifier,
                        principalTable: "Users",
                        principalColumn: "Identifier",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppUserVault_Vaults_SharedVaultsIdentifier",
                        column: x => x.SharedVaultsIdentifier,
                        principalTable: "Vaults",
                        principalColumn: "Identifier",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VaultEntries",
                columns: table => new
                {
                    Identifier = table.Column<Guid>(type: "TEXT", nullable: false),
                    VaultIdentifier = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatorIdentifier = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastUpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CypherPassword = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    CypherData = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false),
                    TagPasswords = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    TagData = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    IVPassword = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    IVData = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaultEntries", x => x.Identifier);
                    table.ForeignKey(
                        name: "FK_VaultEntries_Users_CreatorIdentifier",
                        column: x => x.CreatorIdentifier,
                        principalTable: "Users",
                        principalColumn: "Identifier",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VaultEntries_Vaults_VaultIdentifier",
                        column: x => x.VaultIdentifier,
                        principalTable: "Vaults",
                        principalColumn: "Identifier",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Identifier", "entraId" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), new Guid("00000000-0000-0000-0000-000000000001") });

            migrationBuilder.CreateIndex(
                name: "IX_AppUserVault_SharedVaultsIdentifier",
                table: "AppUserVault",
                column: "SharedVaultsIdentifier");

            migrationBuilder.CreateIndex(
                name: "IX_VaultEntries_CreatorIdentifier",
                table: "VaultEntries",
                column: "CreatorIdentifier");

            migrationBuilder.CreateIndex(
                name: "IX_VaultEntries_VaultIdentifier",
                table: "VaultEntries",
                column: "VaultIdentifier");

            migrationBuilder.CreateIndex(
                name: "IX_Vaults_CreatorIdentifier_Name",
                table: "Vaults",
                columns: new[] { "CreatorIdentifier", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppUserVault");

            migrationBuilder.DropTable(
                name: "VaultEntries");

            migrationBuilder.DropTable(
                name: "Vaults");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
