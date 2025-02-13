using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionPersonnelMedical.Migrations
{
    /// <inheritdoc />
    public partial class AjoutDonneesFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ajout des départements
            migrationBuilder.InsertData(
                table: "Departements",
                columns: new[] { "Id", "Nom", "Description", "NombreMedecins", "NombreInfirmiers" },
                values: new object[,]
                {
            { "Cardiologie", "Cardiologie", "Département de la cardiologie", 1, 1 },
            { "Neurologie", "Neurologie", "Département de la neurologie", 1, 1 },
            { "Pédiatrie", "Pédiatrie", "Département de la pédiatrie", 0, 0 }
                });

            // Ajout des médecins
            migrationBuilder.InsertData(
                table: "Medecins",
                columns: new[] { "Id", "Nom", "Prenom", "Departement", "Specialite", "Disponibilite" },
                values: new object[,]
                {
            { 1, "Dupont", "Jean", "Cardiologie", "Cardiologue", "Disponible" },
            { 2, "Durand", "Marie", "Neurologie", "Neurologue", "Indisponible" }
                });

            // Ajout des infirmiers
            migrationBuilder.InsertData(
                table: "Infirmiers",
                columns: new[] { "Id", "Nom", "Prenom", "Departement", "Disponibilite" },
                values: new object[,]
                {
            { 1, "Leclerc", "Paul", "Cardiologie", "Disponible" },
            { 2, "Moreau", "Anne", "Neurologie", "Indisponible" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Suppression des données ajoutées
            migrationBuilder.DeleteData(
                table: "Medecins",
                keyColumn: "Id",
                keyValues: new object[] { 1, 2 });

            migrationBuilder.DeleteData(
                table: "Infirmiers",
                keyColumn: "Id",
                keyValues: new object[] { 1, 2 });

            migrationBuilder.DeleteData(
                table: "Departements",
                keyColumn: "Id",
                keyValues: new object[] { "Cardio", "Neuro", "Pedia" });
        }
    }
}
