using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LotoMln.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Options = table.Column<string[]>(type: "jsonb", nullable: false),
                    CorrectIndex = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    HostId = table.Column<Guid>(type: "uuid", nullable: false),
                    State = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TurnNumber = table.Column<int>(type: "integer", nullable: false),
                    WinnerId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MaxPlayers = table.Column<int>(type: "integer", nullable: false),
                    TurnDurationSec = table.Column<int>(type: "integer", nullable: false),
                    StealTimeoutSec = table.Column<int>(type: "integer", nullable: false),
                    NameMaxLength = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "StealAttempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomCode = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    SlotId = table.Column<Guid>(type: "uuid", nullable: false),
                    Answer = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StealAttempts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CalledNumbers",
                columns: table => new
                {
                    RoomCode = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    CalledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CalledByPlayerId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalledNumbers", x => new { x.RoomCode, x.Number });
                    table.ForeignKey(
                        name: "FK_CalledNumbers_Rooms_RoomCode",
                        column: x => x.RoomCode,
                        principalTable: "Rooms",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomCode = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    Grid = table.Column<int[][]>(type: "jsonb", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cards_Rooms_RoomCode",
                        column: x => x.RoomCode,
                        principalTable: "Rooms",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameStates",
                columns: table => new
                {
                    RoomCode = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    Phase = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CurrentDrawerId = table.Column<Guid>(type: "uuid", nullable: true),
                    CurrentSlotId = table.Column<Guid>(type: "uuid", nullable: true),
                    PhaseStartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Deadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PlayerQueue = table.Column<List<Guid>>(type: "jsonb", nullable: false),
                    QueueIndex = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameStates", x => x.RoomCode);
                    table.ForeignKey(
                        name: "FK_GameStates_Rooms_RoomCode",
                        column: x => x.RoomCode,
                        principalTable: "Rooms",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KinhClaims",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomCode = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Verified = table.Column<bool>(type: "boolean", nullable: false),
                    WinType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    WinIndex = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KinhClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KinhClaims_Rooms_RoomCode",
                        column: x => x.RoomCode,
                        principalTable: "Rooms",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestionSlots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomCode = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedNumber = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AnsweredByPlayerId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionSlots_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuestionSlots_Rooms_RoomCode",
                        column: x => x.RoomCode,
                        principalTable: "Rooms",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomCode = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    Name = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    CardId = table.Column<Guid>(type: "uuid", nullable: true),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Online = table.Column<bool>(type: "boolean", nullable: false),
                    UsedRedemption = table.Column<bool>(type: "boolean", nullable: false),
                    MarkedNumbers = table.Column<List<int>>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Players_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Players_Rooms_RoomCode",
                        column: x => x.RoomCode,
                        principalTable: "Rooms",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cards_RoomCode",
                table: "Cards",
                column: "RoomCode");

            migrationBuilder.CreateIndex(
                name: "IX_KinhClaims_RoomCode_ClaimedAt",
                table: "KinhClaims",
                columns: new[] { "RoomCode", "ClaimedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Players_CardId",
                table: "Players",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_RoomCode",
                table: "Players",
                column: "RoomCode");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionSlots_QuestionId",
                table: "QuestionSlots",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionSlots_RoomCode_AssignedNumber",
                table: "QuestionSlots",
                columns: new[] { "RoomCode", "AssignedNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionSlots_RoomCode_Position",
                table: "QuestionSlots",
                columns: new[] { "RoomCode", "Position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StealAttempts_RoomCode_SlotId",
                table: "StealAttempts",
                columns: new[] { "RoomCode", "SlotId" });

            migrationBuilder.CreateIndex(
                name: "IX_StealAttempts_Timestamp",
                table: "StealAttempts",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalledNumbers");

            migrationBuilder.DropTable(
                name: "GameStates");

            migrationBuilder.DropTable(
                name: "KinhClaims");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "QuestionSlots");

            migrationBuilder.DropTable(
                name: "StealAttempts");

            migrationBuilder.DropTable(
                name: "Cards");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "Rooms");
        }
    }
}
