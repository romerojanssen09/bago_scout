using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BagoScout.Migrations
{
    /// <inheritdoc />
    public partial class AddMobileAuthTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthToken",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AuthTokenExpiry",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FcmToken",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 4, 9, 6, 3, 332, DateTimeKind.Utc).AddTicks(9804));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 4, 9, 6, 3, 332, DateTimeKind.Utc).AddTicks(9807));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 4, 9, 6, 3, 332, DateTimeKind.Utc).AddTicks(9808));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 4, 9, 6, 3, 332, DateTimeKind.Utc).AddTicks(9809));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 4, 9, 6, 3, 332, DateTimeKind.Utc).AddTicks(9810));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 4, 9, 6, 3, 332, DateTimeKind.Utc).AddTicks(9811));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 4, 9, 6, 3, 332, DateTimeKind.Utc).AddTicks(9812));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 4, 9, 6, 3, 332, DateTimeKind.Utc).AddTicks(9813));

            migrationBuilder.CreateIndex(
                name: "IX_Applications_SeekerId",
                table: "Applications",
                column: "SeekerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Jobs_JobId",
                table: "Applications",
                column: "JobId",
                principalTable: "Jobs",
                principalColumn: "JobId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Users_SeekerId",
                table: "Applications",
                column: "SeekerId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Jobs_JobId",
                table: "Applications");

            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Users_SeekerId",
                table: "Applications");

            migrationBuilder.DropIndex(
                name: "IX_Applications_SeekerId",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "AuthToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AuthTokenExpiry",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FcmToken",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 13, 57, 46, 123, DateTimeKind.Utc).AddTicks(3955));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 13, 57, 46, 123, DateTimeKind.Utc).AddTicks(3956));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 13, 57, 46, 123, DateTimeKind.Utc).AddTicks(3958));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 13, 57, 46, 123, DateTimeKind.Utc).AddTicks(3959));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 13, 57, 46, 123, DateTimeKind.Utc).AddTicks(3960));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 13, 57, 46, 123, DateTimeKind.Utc).AddTicks(3961));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 13, 57, 46, 123, DateTimeKind.Utc).AddTicks(3962));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "SkillId",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 9, 13, 57, 46, 123, DateTimeKind.Utc).AddTicks(3963));
        }
    }
}
