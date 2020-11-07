using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DaAPI.Infrastructure.StorageEngine.Migrations
{
    public partial class DHCPv6Statistics : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DHCPv6LeaseEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    LeaseId = table.Column<Guid>(nullable: false),
                    Address = table.Column<string>(nullable: true),
                    Prefix = table.Column<string>(nullable: true),
                    PrefixLength = table.Column<byte>(nullable: false),
                    Start = table.Column<DateTime>(nullable: false),
                    End = table.Column<DateTime>(nullable: false),
                    ScopeId = table.Column<Guid>(nullable: false),
                    EndReason = table.Column<int>(nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DHCPv6LeaseEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DHCPv6PacketEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    RequestType = table.Column<byte>(nullable: false),
                    ResponseType = table.Column<byte>(nullable: true),
                    RequestSize = table.Column<ushort>(nullable: false),
                    ResponseSize = table.Column<ushort>(nullable: true),
                    HandledSuccessfully = table.Column<bool>(nullable: false),
                    ErrorCode = table.Column<int>(nullable: false),
                    FilteredBy = table.Column<string>(nullable: true),
                    InvalidRequest = table.Column<bool>(nullable: false),
                    ScopeId = table.Column<Guid>(nullable: true),
                    Timestamp = table.Column<DateTime>(nullable: false),
                    TimestampDay = table.Column<DateTime>(nullable: false),
                    TimestampWeek = table.Column<DateTime>(nullable: false),
                    TimestampMonth = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DHCPv6PacketEntries", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DHCPv6LeaseEntries");

            migrationBuilder.DropTable(
                name: "DHCPv6PacketEntries");
        }
    }
}
