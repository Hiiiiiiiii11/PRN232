using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddConsultantCalendarSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create ConsultantSchedules table
            migrationBuilder.CreateTable(
                name: "ConsultantSchedules",
                columns: table => new
                {
                    ScheduleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConsultantID = table.Column<int>(type: "int", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    SlotDurationMinutes = table.Column<int>(type: "int", nullable: false, defaultValue: 30),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultantSchedules", x => x.ScheduleID);
                    table.ForeignKey(
                        name: "FK_ConsultantSchedules_Consultants_ConsultantID",
                        column: x => x.ConsultantID,
                        principalTable: "Consultants",
                        principalColumn: "ConsultantID",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create ConsultantAvailabilities table
            migrationBuilder.CreateTable(
                name: "ConsultantAvailabilities",
                columns: table => new
                {
                    AvailabilityID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConsultantID = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Available"),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultantAvailabilities", x => x.AvailabilityID);
                    table.ForeignKey(
                        name: "FK_ConsultantAvailabilities_Consultants_ConsultantID",
                        column: x => x.ConsultantID,
                        principalTable: "Consultants",
                        principalColumn: "ConsultantID",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create AppointmentSlots table
            migrationBuilder.CreateTable(
                name: "AppointmentSlots",
                columns: table => new
                {
                    SlotID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConsultantID = table.Column<int>(type: "int", nullable: false),
                    SlotDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false, defaultValue: 30),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Available"),
                    AppointmentID = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentSlots", x => x.SlotID);
                    table.ForeignKey(
                        name: "FK_AppointmentSlots_Consultants_ConsultantID",
                        column: x => x.ConsultantID,
                        principalTable: "Consultants",
                        principalColumn: "ConsultantID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppointmentSlots_Appointments_AppointmentID",
                        column: x => x.AppointmentID,
                        principalTable: "Appointments",
                        principalColumn: "AppointmentID",
                        onDelete: ReferentialAction.SetNull);
                });

            // Create indexes for better performance
            migrationBuilder.CreateIndex(
                name: "IX_ConsultantSchedules_ConsultantID_DayOfWeek",
                table: "ConsultantSchedules",
                columns: new[] { "ConsultantID", "DayOfWeek" });

            migrationBuilder.CreateIndex(
                name: "IX_ConsultantAvailabilities_ConsultantID_Date",
                table: "ConsultantAvailabilities",
                columns: new[] { "ConsultantID", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentSlots_ConsultantID_SlotDateTime",
                table: "AppointmentSlots",
                columns: new[] { "ConsultantID", "SlotDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentSlots_Status_SlotDateTime",
                table: "AppointmentSlots",
                columns: new[] { "Status", "SlotDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentSlots_AppointmentID",
                table: "AppointmentSlots",
                column: "AppointmentID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AppointmentSlots");
            migrationBuilder.DropTable(name: "ConsultantAvailabilities");
            migrationBuilder.DropTable(name: "ConsultantSchedules");
        }
    }
}