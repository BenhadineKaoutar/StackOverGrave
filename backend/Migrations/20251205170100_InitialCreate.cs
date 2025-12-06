using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StackOverGrave.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OriginalFilename = table.Column<string>(type: "TEXT", nullable: false),
                    Technology = table.Column<int>(type: "INTEGER", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    FileSize = table.Column<long>(type: "INTEGER", nullable: false),
                    LinesOfCode = table.Column<int>(type: "INTEGER", nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RepositoryProjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Source = table.Column<int>(type: "INTEGER", nullable: false),
                    SourceUrl = table.Column<string>(type: "TEXT", nullable: true),
                    OriginalFilename = table.Column<string>(type: "TEXT", nullable: false),
                    FileSize = table.Column<long>(type: "INTEGER", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ProjectSize = table.Column<int>(type: "INTEGER", nullable: false),
                    SourceTechnology = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetTechnology = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalFiles = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalLinesOfCode = table.Column<int>(type: "INTEGER", nullable: false),
                    ConvertedFiles = table.Column<int>(type: "INTEGER", nullable: false),
                    EstimatedCost = table.Column<decimal>(type: "TEXT", nullable: false),
                    PackagePath = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepositoryProjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConversionResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OriginalCode = table.Column<string>(type: "TEXT", nullable: false),
                    ConvertedCode = table.Column<string>(type: "TEXT", nullable: false),
                    MigrationNotes = table.Column<string>(type: "TEXT", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversionResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConversionResults_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RepositoryFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RepositoryProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RelativePath = table.Column<string>(type: "TEXT", nullable: false),
                    LineCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CriticalityScore = table.Column<int>(type: "INTEGER", nullable: false),
                    SelectedForConversion = table.Column<bool>(type: "INTEGER", nullable: false),
                    ConversionSucceeded = table.Column<bool>(type: "INTEGER", nullable: false),
                    ConversionError = table.Column<string>(type: "TEXT", nullable: true),
                    ConvertedPath = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepositoryFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RepositoryFiles_RepositoryProjects_RepositoryProjectId",
                        column: x => x.RepositoryProjectId,
                        principalTable: "RepositoryProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConversionResults_ProjectId",
                table: "ConversionResults",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_RepositoryFiles_RepositoryProjectId",
                table: "RepositoryFiles",
                column: "RepositoryProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConversionResults");

            migrationBuilder.DropTable(
                name: "RepositoryFiles");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "RepositoryProjects");
        }
    }
}
