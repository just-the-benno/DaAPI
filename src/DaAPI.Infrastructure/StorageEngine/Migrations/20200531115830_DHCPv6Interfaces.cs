using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DaAPI.Infrastructure.StorageEngine.Migrations
{
    public partial class DHCPv6Interfaces : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DHCPv6Interfaces",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    IPv6Address = table.Column<string>(nullable: true),
                    InterfaceId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DHCPv6Interfaces", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DHCPv6Interfaces");
        }
    }
}
