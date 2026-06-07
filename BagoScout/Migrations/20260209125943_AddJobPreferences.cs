using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BagoScout.Migrations
{
    /// <inheritdoc />
    public partial class AddJobPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobPreferences",
                columns: table => new
                {
                    JobPreferenceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PreferredJobType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PreferredJobTitles = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MinSalary = table.Column<int>(type: "int", nullable: true),
                    MaxSalary = table.Column<int>(type: "int", nullable: true),
                    PreferredLocation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MaxDistance = table.Column<int>(type: "int", nullable: true),
                    PreferredExperienceLevel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPreferences", x => x.JobPreferenceId);
                    table.ForeignKey(
                        name: "FK_JobPreferences_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 12, 59, 43, 586, DateTimeKind.Utc).AddTicks(1332));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 12, 59, 43, 586, DateTimeKind.Utc).AddTicks(1333));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 12, 59, 43, 586, DateTimeKind.Utc).AddTicks(1335));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 12, 59, 43, 586, DateTimeKind.Utc).AddTicks(1336));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 12, 59, 43, 586, DateTimeKind.Utc).AddTicks(1338));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 12, 59, 43, 586, DateTimeKind.Utc).AddTicks(1338));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 12, 59, 43, 586, DateTimeKind.Utc).AddTicks(1339));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 12, 59, 43, 586, DateTimeKind.Utc).AddTicks(1340));

            migrationBuilder.CreateIndex(
                name: "IX_JobPreferences_UserId",
                table: "JobPreferences",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobPreferences");

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 12, 50, 45, 423, DateTimeKind.Utc).AddTicks(1589));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 12, 50, 45, 423, DateTimeKind.Utc).AddTicks(1617));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 12, 50, 45, 423, DateTimeKind.Utc).AddTicks(1618));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 12, 50, 45, 423, DateTimeKind.Utc).AddTicks(1619));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 12, 50, 45, 423, DateTimeKind.Utc).AddTicks(1621));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 12, 50, 45, 423, DateTimeKind.Utc).AddTicks(1622));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 12, 50, 45, 423, DateTimeKind.Utc).AddTicks(1623));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 12, 50, 45, 423, DateTimeKind.Utc).AddTicks(1624));
        }
    }
}
