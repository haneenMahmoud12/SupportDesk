using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SupportDesk.Infrastructure.Data;

#nullable disable

namespace SupportDesk.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260902130000_NormalizeTicketStatuses")]
public sealed class NormalizeTicketStatuses : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // The former Resolved value was 2 and the former Closed value was 3.
        migrationBuilder.Sql("UPDATE Tickets SET Status = 2 WHERE Status IN (2, 3)");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Closed was stored as 3 before the three-status model was introduced.
        migrationBuilder.Sql("UPDATE Tickets SET Status = 3 WHERE Status = 2");
    }
}
