using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestionPersonnelMedical.Models
{
    public class Consultation
    {
        [Key]
        public string Patient { get; set; }
        public string Medecin { get; set; }
        public DateTime Date { get; set; }
        public string Heure { get; set; }
    }
}
