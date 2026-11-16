using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingUserRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "bookings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql("""
                INSERT INTO users ("Id", "Login", "PasswordHash", "Role")
                SELECT
                    gen_random_uuid(),
                    '__legacy_bookings__',
                    'BA==',
                    'User'
                WHERE EXISTS (
                    SELECT 1
                    FROM bookings
                    WHERE "UserId" = '00000000-0000-0000-0000-000000000000'::uuid
                )
                ON CONFLICT ("Login") DO NOTHING;

                UPDATE bookings
                SET "UserId" = (
                    SELECT "Id"
                    FROM users
                    WHERE "Login" = '__legacy_bookings__'
                )
                WHERE "UserId" = '00000000-0000-0000-0000-000000000000'::uuid;

                ALTER TABLE bookings
                ALTER COLUMN "UserId" DROP DEFAULT;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_bookings_UserId",
                table: "bookings",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_bookings_users_UserId",
                table: "bookings",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_bookings_users_UserId",
                table: "bookings");

            migrationBuilder.DropIndex(
                name: "IX_bookings_UserId",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "bookings");
        }
    }
}
