using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IpGroups.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePersonIpAddressRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Personeller_Ipler_IpId",
                table: "Personeller");

            migrationBuilder.Sql("UPDATE \"Personeller\" SET \"IpId\" = NULL;");

            migrationBuilder.RenameColumn(
                name: "IpId",
                table: "Personeller",
                newName: "IpAddressId");

            migrationBuilder.RenameIndex(
                name: "IX_Personeller_IpId",
                table: "Personeller",
                newName: "IX_Personeller_IpAddressId");

            migrationBuilder.CreateTable(
                name: "IpAdresler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IpGroupId = table.Column<int>(type: "integer", nullable: false),
                    FullIpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    Octet = table.Column<int>(type: "integer", nullable: false),
                    IsAssigned = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IpAdresler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IpAdresler_Ipler_IpGroupId",
                        column: x => x.IpGroupId,
                        principalTable: "Ipler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IpAdresler_FullIpAddress",
                table: "IpAdresler",
                column: "FullIpAddress",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IpAdresler_IpGroupId_Octet",
                table: "IpAdresler",
                columns: new[] { "IpGroupId", "Octet" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Personeller_IpAdresler_IpAddressId",
                table: "Personeller",
                column: "IpAddressId",
                principalTable: "IpAdresler",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Personeller_IpAdresler_IpAddressId",
                table: "Personeller");

            migrationBuilder.DropTable(
                name: "IpAdresler");

            migrationBuilder.RenameColumn(
                name: "IpAddressId",
                table: "Personeller",
                newName: "IpId");

            migrationBuilder.RenameIndex(
                name: "IX_Personeller_IpAddressId",
                table: "Personeller",
                newName: "IX_Personeller_IpId");

            migrationBuilder.AddForeignKey(
                name: "FK_Personeller_Ipler_IpId",
                table: "Personeller",
                column: "IpId",
                principalTable: "Ipler",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
