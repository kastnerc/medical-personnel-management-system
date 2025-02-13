using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestionPersonnelMedical.Models
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Consultation> Consultations { get; set; }
        public DbSet<Departement> Departements { get; set; }
        public DbSet<Infirmier> Infirmiers { get; set; }
        public DbSet<Medecin> Medecins { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer("Server=CALEBLAPTOP\\SQLEXPRESS;Initial Catalog=GestionPersonnelMedical;Integrated Security=True;Trust Server Certificate=True;");
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuration de la table Departement
            modelBuilder.Entity<Departement>()
                .HasKey(d => d.Id); // Clé primaire
            modelBuilder.Entity<Departement>()
                .Property(d => d.Id)
                .ValueGeneratedOnAdd(); // SQL Server génère l'ID automatiquement
            modelBuilder.Entity<Departement>()
                .HasIndex(d => d.Nom) // Assure l'unicité du Nom
                .IsUnique();

            // Configuration de la table Medecin
            modelBuilder.Entity<Medecin>()
                .HasKey(m => m.Id); // Clé primaire
            modelBuilder.Entity<Medecin>()
                .HasOne<Departement>() // Relation Medecin -> Departement
                .WithMany() // Un département peut avoir plusieurs médecins
                .HasForeignKey(m => m.Departement) // Clé étrangère basée sur Nom
                .HasPrincipalKey(d => d.Nom) // Utilise le Nom comme clé principale
                .OnDelete(DeleteBehavior.Restrict); // Pas de suppression en cascade

            // Configuration de la table Infirmier
            modelBuilder.Entity<Infirmier>()
                .HasKey(i => i.Id); // Clé primaire
            modelBuilder.Entity<Infirmier>()
                .HasOne<Departement>() // Relation Infirmier -> Departement
                .WithMany() // Un département peut avoir plusieurs infirmiers
                .HasForeignKey(i => i.Departement) // Clé étrangère basée sur Nom
                .HasPrincipalKey(d => d.Nom) // Utilise le Nom comme clé principale
                .OnDelete(DeleteBehavior.Restrict); // Pas de suppression en cascade

            // Configuration de la table Consultation
            modelBuilder.Entity<Consultation>()
                .HasKey(c => new { c.Patient, c.Date }); // Clé composite
            modelBuilder.Entity<Consultation>()
                .Property(c => c.Heure)
                .HasMaxLength(5) // Format "HH:mm"
                .IsRequired(); // Champ obligatoire

            base.OnModelCreating(modelBuilder);
        }
    }
}
