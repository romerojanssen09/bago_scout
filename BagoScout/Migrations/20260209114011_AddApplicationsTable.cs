using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BagoScout.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Applications",
                columns: table => new
                {
                    ApplicationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobId = table.Column<int>(type: "int", nullable: false),
                    SeekerId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Pending"),
                    CoverLetter = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AppliedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.ApplicationId);
                });

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 11, 40, 11, 315, DateTimeKind.Utc).AddTicks(9674));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 11, 40, 11, 315, DateTimeKind.Utc).AddTicks(9678));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 11, 40, 11, 315, DateTimeKind.Utc).AddTicks(9706));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 11, 40, 11, 315, DateTimeKind.Utc).AddTicks(9707));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 11, 40, 11, 315, DateTimeKind.Utc).AddTicks(9708));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 11, 40, 11, 315, DateTimeKind.Utc).AddTicks(9709));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 11, 40, 11, 315, DateTimeKind.Utc).AddTicks(9710));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 11, 40, 11, 315, DateTimeKind.Utc).AddTicks(9711));

            migrationBuilder.CreateIndex(
                name: "IX_Applications_JobId_SeekerId",
                table: "Applications",
                columns: new[] { "JobId", "SeekerId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Applications");

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 10, 10, 1, 733, DateTimeKind.Utc).AddTicks(5856));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 10, 10, 1, 733, DateTimeKind.Utc).AddTicks(5859));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 10, 10, 1, 733, DateTimeKind.Utc).AddTicks(5860));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 10, 10, 1, 733, DateTimeKind.Utc).AddTicks(5861));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 10, 10, 1, 733, DateTimeKind.Utc).AddTicks(5862));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 10, 10, 1, 733, DateTimeKind.Utc).AddTicks(5863));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 10, 10, 1, 733, DateTimeKind.Utc).AddTicks(5864));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 10, 10, 1, 733, DateTimeKind.Utc).AddTicks(5865));
        }
    }
}
