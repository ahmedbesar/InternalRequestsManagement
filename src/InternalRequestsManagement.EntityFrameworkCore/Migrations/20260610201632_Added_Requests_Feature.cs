using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternalRequestsManagement.Migrations
{
    /// <inheritdoc />
    public partial class Added_Requests_Feature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    RequestTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RequesterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Justification = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppRequestTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    OrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RequiresJustification = table.Column<bool>(type: "bit", nullable: false),
                    RequiresDueDate = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppRequestTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppRequestStatusHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromStatus = table.Column<int>(type: "int", nullable: false),
                    ToStatus = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ChangedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppRequestStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppRequestStatusHistories_AppRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "AppRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppRequests_AssignedUserId",
                table: "AppRequests",
                column: "AssignedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppRequests_DueDate",
                table: "AppRequests",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_AppRequests_OrganizationUnitId",
                table: "AppRequests",
                column: "OrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_AppRequests_RequesterId",
                table: "AppRequests",
                column: "RequesterId");

            migrationBuilder.CreateIndex(
                name: "IX_AppRequests_Status",
                table: "AppRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AppRequestStatusHistories_RequestId",
                table: "AppRequestStatusHistories",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_AppRequestTypes_IsActive",
                table: "AppRequestTypes",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AppRequestTypes_OrganizationUnitId",
                table: "AppRequestTypes",
                column: "OrganizationUnitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppRequestStatusHistories");

            migrationBuilder.DropTable(
                name: "AppRequestTypes");

            migrationBuilder.DropTable(
                name: "AppRequests");
        }
    }
}
