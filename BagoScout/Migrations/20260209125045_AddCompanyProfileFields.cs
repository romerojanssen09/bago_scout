using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BagoScout.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompanyAddress",
                table: "Users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyDescription",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyIndustry",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "CompanyLatitude",
                table: "Users",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyLogoPath",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "CompanyLongitude",
                table: "Users",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "Users",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanySize",
                table: "Users",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyWebsite",
                table: "Users",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyAddress",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CompanyDescription",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CompanyIndustry",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CompanyLatitude",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CompanyLogoPath",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CompanyLongitude",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CompanySize",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CompanyWebsite",
                table: "Users");

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
    }
}
