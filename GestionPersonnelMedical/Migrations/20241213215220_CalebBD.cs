using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionPersonnelMedical.Migrations
{
    /// <inheritdoc />
    public partial class CalebBD : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Infirmiers_Departements_Departement",
                table: "Infirmiers");

            migrationBuilder.DropForeignKey(
                name: "FK_Medecins_Departements_Departement",
                table: "Medecins");

            migrationBuilder.AlterColumn<string>(
                name: "Nom",
                table: "Departements",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Departements_Nom",
                table: "Departements",
                column: "Nom");

            migrationBuilder.CreateIndex(
                name: "IX_Departements_Nom",
                table: "Departements",
                column: "Nom",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Infirmiers_Departements_Departement",
                table: "Infirmiers",
                column: "Departement",
                principalTable: "Departements",
                principalColumn: "Nom",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Medecins_Departements_Departement",
                table: "Medecins",
                column: "Departement",
                principalTable: "Departements",
                principalColumn: "Nom",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Infirmiers_Departements_Departement",
                table: "Infirmiers");

            migrationBuilder.DropForeignKey(
                name: "FK_Medecins_Departements_Departement",
                table: "Medecins");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Departements_Nom",
                table: "Departements");

            migrationBuilder.DropIndex(
                name: "IX_Departements_Nom",
                table: "Departements");

            migrationBuilder.AlterColumn<string>(
                name: "Nom",
                table: "Departements",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_Infirmiers_Departements_Departement",
                table: "Infirmiers",
                column: "Departement",
                principalTable: "Departements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Medecins_Departements_Departement",
                table: "Medecins",
                column: "Departement",
                principalTable: "Departements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
