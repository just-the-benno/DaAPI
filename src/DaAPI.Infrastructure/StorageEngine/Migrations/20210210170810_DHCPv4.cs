using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DaAPI.Infrastructure.StorageEngine.Migrations
{
    public partial class DHCPv4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DHCPv4Interfaces",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    IPv6Address = table.Column<string>(type: "TEXT", nullable: true),
                    InterfaceId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DHCPv4Interfaces", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DHCPv4PacketEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RequestType = table.Column<int>(type: "INTEGER", nullable: false),
                    ResponseType = table.Column<int>(type: "INTEGER", nullable: true),
                    RequestSize = table.Column<ushort>(type: "INTEGER", nullable: false),
                    ResponseSize = table.Column<ushort>(type: "INTEGER", nullable: true),
                    HandledSuccessfully = table.Column<bool>(type: "INTEGER", nullable: false),
                    ErrorCode = table.Column<int>(type: "INTEGER", nullable: false),
                    FilteredBy = table.Column<string>(type: "TEXT", nullable: true),
                    InvalidRequest = table.Column<bool>(type: "INTEGER", nullable: false),
                    ScopeId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimestampDay = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimestampWeek = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimestampMonth = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DHCPv4PacketEntries", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DHCPv4Interfaces");

            migrationBuilder.DropTable(
                name: "DHCPv4PacketEntries");
        }
    }
}
