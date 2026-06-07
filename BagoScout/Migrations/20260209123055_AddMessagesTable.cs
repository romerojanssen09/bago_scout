using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BagoScout.Migrations
{
    /// <inheritdoc />
    public partial class AddMessagesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    MessageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SenderId = table.Column<int>(type: "int", nullable: false),
                    ReceiverId = table.Column<int>(type: "int", nullable: false),
                    MessageText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsRead = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.MessageId);
                });

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 12, 30, 54, 887, DateTimeKind.Utc).AddTicks(621));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 12, 30, 54, 887, DateTimeKind.Utc).AddTicks(623));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 12, 30, 54, 887, DateTimeKind.Utc).AddTicks(666));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 12, 30, 54, 887, DateTimeKind.Utc).AddTicks(667));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 12, 30, 54, 887, DateTimeKind.Utc).AddTicks(668));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 12, 30, 54, 887, DateTimeKind.Utc).AddTicks(669));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 12, 30, 54, 887, DateTimeKind.Utc).AddTicks(670));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 12, 30, 54, 887, DateTimeKind.Utc).AddTicks(671));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 12, 8, 27, 452, DateTimeKind.Utc).AddTicks(1178));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 12, 8, 27, 452, DateTimeKind.Utc).AddTicks(1180));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 12, 8, 27, 452, DateTimeKind.Utc).AddTicks(1182));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 12, 8, 27, 452, DateTimeKind.Utc).AddTicks(1183));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 12, 8, 27, 452, DateTimeKind.Utc).AddTicks(1184));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 12, 8, 27, 452, DateTimeKind.Utc).AddTicks(1185));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 12, 8, 27, 452, DateTimeKind.Utc).AddTicks(1186));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 12, 8, 27, 452, DateTimeKind.Utc).AddTicks(1187));
        }
    }
}
