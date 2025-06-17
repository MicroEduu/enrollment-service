using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnrollmentService.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEnrollmentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Matriculas",
                table: "Matriculas");

            migrationBuilder.RenameTable(
                name: "Matriculas",
                newName: "Enrollments");

            migrationBuilder.RenameColumn(
                name: "IdCurso",
                table: "Enrollments",
                newName: "CourseId");

            migrationBuilder.RenameColumn(
                name: "IdAluno",
                table: "Enrollments",
                newName: "StudentId");

            migrationBuilder.RenameColumn(
                name: "DataMatricula",
                table: "Enrollments",
                newName: "EnrollmentDate");

            migrationBuilder.RenameIndex(
                name: "IX_Matriculas_IdAluno_IdCurso",
                table: "Enrollments",
                newName: "IX_Enrollment_Student_Course");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Enrollments",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Enrollments",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Enrollments",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Enrollments",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "Enrolled");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Enrollments",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Enrollments",
                table: "Enrollments",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_Active_Status",
                table: "Enrollments",
                columns: new[] { "IsActive", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_Course",
                table: "Enrollments",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_Date",
                table: "Enrollments",
                column: "EnrollmentDate");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_IsActive",
                table: "Enrollments",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_Status",
                table: "Enrollments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_Student",
                table: "Enrollments",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Enrollments",
                table: "Enrollments");

            migrationBuilder.DropIndex(
                name: "IX_Enrollment_Active_Status",
                table: "Enrollments");

            migrationBuilder.DropIndex(
                name: "IX_Enrollment_Course",
                table: "Enrollments");

            migrationBuilder.DropIndex(
                name: "IX_Enrollment_Date",
                table: "Enrollments");

            migrationBuilder.DropIndex(
                name: "IX_Enrollment_IsActive",
                table: "Enrollments");

            migrationBuilder.DropIndex(
                name: "IX_Enrollment_Status",
                table: "Enrollments");

            migrationBuilder.DropIndex(
                name: "IX_Enrollment_Student",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Enrollments");

            migrationBuilder.RenameTable(
                name: "Enrollments",
                newName: "Matriculas");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "Matriculas",
                newName: "IdAluno");

            migrationBuilder.RenameColumn(
                name: "EnrollmentDate",
                table: "Matriculas",
                newName: "DataMatricula");

            migrationBuilder.RenameColumn(
                name: "CourseId",
                table: "Matriculas",
                newName: "IdCurso");

            migrationBuilder.RenameIndex(
                name: "IX_Enrollment_Student_Course",
                table: "Matriculas",
                newName: "IX_Matriculas_IdAluno_IdCurso");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Matriculas",
                table: "Matriculas",
                column: "Id");
        }
    }
}
