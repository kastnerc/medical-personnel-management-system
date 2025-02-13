using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestionPersonnelMedical.Models
{
    public class Medecin
    {
        [Key]
        public int Id { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string Departement { get; set; }
        public string Specialite { get; set; }
        public string Disponibilite { get; set; }
        public string NomComplet => $"{Prenom} {Nom}";
    }
}
