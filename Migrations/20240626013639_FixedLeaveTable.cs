using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace lms.api.Migrations
{
    /// <inheritdoc />
    public partial class FixedLeaveTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LeaveHistories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LeaveHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<long>(type: "bigint", nullable: false),
                    LeaveAppliedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LeaveType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveHistories", x => x.Id);
                });
        }
    }
}
