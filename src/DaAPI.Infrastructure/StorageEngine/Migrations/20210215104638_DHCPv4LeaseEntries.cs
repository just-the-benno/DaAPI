using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DaAPI.Infrastructure.StorageEngine.Migrations
{
    public partial class DHCPv4LeaseEntries : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IPv6Address",
                table: "DHCPv4Interfaces",
                newName: "IPv4Address");

            migrationBuilder.CreateTable(
                name: "DHCPv4LeaseEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LeaseId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Address = table.Column<string>(type: "TEXT", nullable: true),
                    Start = table.Column<DateTime>(type: "TEXT", nullable: false),
                    End = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ScopeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EndReason = table.Column<int>(type: "INTEGER", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DHCPv4LeaseEntries", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DHCPv4LeaseEntries");

            migrationBuilder.RenameColumn(
                name: "IPv4Address",
                table: "DHCPv4Interfaces",
                newName: "IPv6Address");
        }
    }
}
