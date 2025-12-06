using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StackOverGrave.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDeprecatedDateToProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeprecatedDate",
                table: "Projects",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeprecatedDate",
                table: "Projects");
        }
    }
}
