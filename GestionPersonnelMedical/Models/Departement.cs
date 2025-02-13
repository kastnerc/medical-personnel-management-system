using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GestionPersonnelMedical.Models
{
    public class Departement
    {
        [Key]
        public string Id { get; set; } // Sera défini manuellement à partir du Nom
        public string Nom { get; set; }
        public string Description { get; set; }
        public int NombreMedecins { get; set; }
        public int NombreInfirmiers { get; set; }
    }
}
