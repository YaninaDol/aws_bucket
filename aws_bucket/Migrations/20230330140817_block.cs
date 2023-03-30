using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aws_bucket.Migrations
{
    /// <inheritdoc />
    public partial class block : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "blockedUntil",
                table: "UserInfos",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "blockedUntil",
                table: "UserInfos");
        }
    }
}
