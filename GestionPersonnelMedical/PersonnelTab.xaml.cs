using GestionPersonnelMedical.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace GestionPersonnelMedical
{
    /// <summary>
    /// Interaction logic for PersonnelTab.xaml
    /// </summary>
    public partial class PersonnelTab : UserControl
    {
        public PersonnelTab()
        {
            InitializeComponent();
            this.Loaded += async (s, e) =>
            {
                if (TypeActif == TypePersonnel.Medecins)
                    await LoadMedecinsAsync();
                else if (TypeActif == TypePersonnel.Infirmiers)
                    await LoadInfirmiersAsync();
            };
        }

        // Initialisation des départements pour les ComboBox
        public ObservableCollection<Departement> Departements { get; private set; }

        public void SetDepartements(ObservableCollection<Departement> departements)
        {
            Departements = departements;
            cbDepartement.ItemsSource = Departements;
            cbFiltreDep.ItemsSource = Departements;
            cbDepartement.DisplayMemberPath = "Nom";  // Affiche uniquement le nom des départements
            cbFiltreDep.DisplayMemberPath = "Nom";
        }

        // Propriétés de dépendance pour Médecins
        public ObservableCollection<Medecin> ListeMedecins
        {
            get { return (ObservableCollection<Medecin>)GetValue(ListeMedecinsProperty); }
            set { SetValue(ListeMedecinsProperty, value); }
        }

        public static readonly DependencyProperty ListeMedecinsProperty =
            DependencyProperty.Register("ListeMedecins", typeof(ObservableCollection<Medecin>), typeof(PersonnelTab));

        // Propriétés de dépendance pour Infirmiers
        public ObservableCollection<Infirmier> ListeInfirmiers
        {
            get { return (ObservableCollection<Infirmier>)GetValue(ListeInfirmiersProperty); }
            set { SetValue(ListeInfirmiersProperty, value); }
        }

        public static readonly DependencyProperty ListeInfirmiersProperty =
            DependencyProperty.Register("ListeInfirmiers", typeof(ObservableCollection<Infirmier>), typeof(PersonnelTab));

        // Expose des contrôles spécifiques pour une gestion personnalisée
        public TextBox SpecialiteTextBox => tbSpecialite;
        public Label SpecialiteLabel => lbSpecialite;
        public TextBox SpecialiteFiltreTextBox => tbFiltreSpecialite;
        public Label SpecialiteFiltreLabel => lbFiltreSpecialite;
        public DataGridTextColumn SpecialiteDataGridColumn => SpecialiteColumn;

        // Classe pour l'événement AjouterPersonnel
        public class PersonnelEventArgs : EventArgs
        {
            public string Nom { get; set; }
            public string Prenom { get; set; }
            public string Departement { get; set; }
            public string Disponibilite { get; set; }
            public string Specialite { get; set; }
        }
        private async Task LoadMedecinsAsync()
        {
            using (var context = new ApplicationDbContext())
            {
                // Charger les médecins depuis la base de données
                var medecins = await context.Medecins.ToListAsync();
                ListeMedecins = new ObservableCollection<Medecin>(medecins);
                dgPersonnel.ItemsSource = ListeMedecins; // Liaison au DataGrid
            }
        }

        private async Task LoadInfirmiersAsync()
        {
            using (var context = new ApplicationDbContext())
            {
                // Charger les infirmiers depuis la base de données
                var infirmiers = await context.Infirmiers.ToListAsync();
                ListeInfirmiers = new ObservableCollection<Infirmier>(infirmiers);
                dgPersonnel.ItemsSource = ListeInfirmiers; // Liaison au DataGrid
            }
        }

        // Événements pour ajouter un personnel
        public event EventHandler<PersonnelEventArgs> AjouterPersonnel;
        public event EventHandler<string> StatusMessageUpdated;

        // Méthode pour ajouter un personnel
        private void btn_Ajouter(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbNom.Text) &&
                !string.IsNullOrEmpty(tbPrenom.Text) &&
                cbDepartement.SelectedItem != null &&
                cbDisponibilite.SelectedItem != null)
            {
                var selectedDepartement = cbDepartement.SelectedItem as Departement;
                string selectedDisponibilite = (cbDisponibilite.SelectedItem as ComboBoxItem)?.Content.ToString();

                AjouterPersonnel?.Invoke(this, new PersonnelEventArgs
                {
                    Nom = tbNom.Text,
                    Prenom = tbPrenom.Text,
                    Departement = selectedDepartement?.Nom,
                    Disponibilite = selectedDisponibilite,
                    Specialite = tbSpecialite.Text
                });

                // Vider les champs après l'ajout
                tbNom.Clear();
                tbPrenom.Clear();
                cbDepartement.SelectedItem = null;
                cbDisponibilite.SelectedItem = null;
                tbSpecialite.Clear();

                StatusMessageUpdated?.Invoke(this, "StatusAddedSuccessfully");
            }
            else
            {
                StatusMessageUpdated?.Invoke(this, "StatusFillAllFields");
            }
        }

        // Événements pour modifier un personnel
        public event EventHandler<ModifierPersonnelEventArgs> ModifierPersonnel;

        public class ModifierPersonnelEventArgs : EventArgs
        {
            public object AncienPersonnel { get; set; }  // L'ancien personnel avant modification
            public object NouveauPersonnel { get; set; } // Le personnel après modification
        }

        // Méthode pour modifier un personnel
        private void btn_Modifier(object sender, RoutedEventArgs e)
        {
            if (dgPersonnel.SelectedItem == null)
            {
                StatusMessageUpdated?.Invoke(this, "SelectPersonnelPrompt");
                return;
            }

            var selectedDepartement = cbDepartement.SelectedItem as Departement;
            string selectedDisponibilite = (cbDisponibilite.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (string.IsNullOrEmpty(tbNom.Text) || string.IsNullOrEmpty(tbPrenom.Text) || selectedDepartement == null || selectedDisponibilite == null)
            {
                StatusMessageUpdated?.Invoke(this, "StatusFillAllFields");
                return;
            }

            object ancienPersonnel = null;
            object nouveauPersonnel = null;

            if (dgPersonnel.SelectedItem is Medecin medecin)
            {
                ancienPersonnel = new Medecin
                {
                    Id = medecin.Id, // Ajout de l'ID pour traçabilité
                    Nom = medecin.Nom,
                    Prenom = medecin.Prenom,
                    Departement = medecin.Departement,
                    Disponibilite = medecin.Disponibilite,
                    Specialite = medecin.Specialite
                };

                medecin.Nom = tbNom.Text;
                medecin.Prenom = tbPrenom.Text;
                medecin.Departement = selectedDepartement.Id; // Utiliser l'ID du département
                medecin.Disponibilite = selectedDisponibilite;
                medecin.Specialite = tbSpecialite.Text;

                nouveauPersonnel = medecin;
            }
            else if (dgPersonnel.SelectedItem is Infirmier infirmier)
            {
                ancienPersonnel = new Infirmier
                {
                    Id = infirmier.Id, // Ajout de l'ID pour traçabilité
                    Nom = infirmier.Nom,
                    Prenom = infirmier.Prenom,
                    Departement = infirmier.Departement,
                    Disponibilite = infirmier.Disponibilite
                };

                infirmier.Nom = tbNom.Text;
                infirmier.Prenom = tbPrenom.Text;
                infirmier.Departement = selectedDepartement.Id; // Utiliser l'ID du département
                infirmier.Disponibilite = selectedDisponibilite;

                nouveauPersonnel = infirmier;
            }

            ModifierPersonnel?.Invoke(this, new ModifierPersonnelEventArgs
            {
                AncienPersonnel = ancienPersonnel,
                NouveauPersonnel = nouveauPersonnel
            });

            dgPersonnel.Items.Refresh();
            StatusMessageUpdated?.Invoke(this, "ModificationSaved");

            // Vider les champs après modification
            tbNom.Clear();
            tbPrenom.Clear();
            cbDepartement.SelectedItem = null;
            cbDisponibilite.SelectedItem = null;
            tbSpecialite.Clear();
        }

        // Événements pour supprimer un personnel
        public event EventHandler<object> SupprimerPersonnel;

        private void btn_Supprimer(object sender, RoutedEventArgs e)
        {
            var personnelSelectionne = dgPersonnel.SelectedItem;

            if (personnelSelectionne != null)
            {
                SupprimerPersonnel?.Invoke(this, personnelSelectionne);
                StatusMessageUpdated?.Invoke(this, "PersonnelDeletedSuccessfully");
            }
            else
            {
                StatusMessageUpdated?.Invoke(this, "SelectPersonnelPrompt");
            }
        }

        // Gestion de la sélection dans le DataGrid
        private void dgPersonnel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var personnelSelectionne = dgPersonnel.SelectedItem;

            if (personnelSelectionne is Medecin medecin)
            {
                tbNom.Text = medecin.Nom;
                tbPrenom.Text = medecin.Prenom;
                cbDepartement.SelectedItem = Departements.FirstOrDefault(d => d.Nom == medecin.Departement);
                cbDisponibilite.SelectedItem = cbDisponibilite.Items.Cast<ComboBoxItem>()
                    .FirstOrDefault(item => item.Content.ToString() == medecin.Disponibilite);
                tbSpecialite.Text = medecin.Specialite;
            }
            else if (personnelSelectionne is Infirmier infirmier)
            {
                tbNom.Text = infirmier.Nom;
                tbPrenom.Text = infirmier.Prenom;
                cbDepartement.SelectedItem = Departements.FirstOrDefault(d => d.Nom == infirmier.Departement);
                cbDisponibilite.SelectedItem = cbDisponibilite.Items.Cast<ComboBoxItem>()
                    .FirstOrDefault(item => item.Content.ToString() == infirmier.Disponibilite);
            }
        }

        // Recherche et réinitialisation pour Médecins et Infirmiers
        private void btn_Rechercher(object sender, RoutedEventArgs e)
        {
            if (dgPersonnel.ItemsSource == null)
            {
                StatusMessageUpdated?.Invoke(this, "StatusNoDataToFilter");
                return;
            }

            if (TypeActif == TypePersonnel.Medecins)
            {
                var filteredMedecins = ListeMedecins.Where(m =>
                    (cbFiltreDep.SelectedItem == null || m.Departement == (cbFiltreDep.SelectedItem as Departement)?.Nom) &&
                    (cbFiltreDisponibilite.SelectedItem == null || m.Disponibilite == (cbFiltreDisponibilite.SelectedItem as ComboBoxItem)?.Content.ToString()) &&
                    (string.IsNullOrEmpty(tbFiltreSpecialite.Text) ||
                     (m.Specialite != null && m.Specialite.Contains(tbFiltreSpecialite.Text, StringComparison.OrdinalIgnoreCase)))
                ).ToList();

                dgPersonnel.ItemsSource = filteredMedecins;
                StatusMessageUpdated?.Invoke(this, "StatusFilteredResults");
            }
            else if (TypeActif == TypePersonnel.Infirmiers)
            {
                var filteredInfirmiers = ListeInfirmiers.Where(i =>
                    (cbFiltreDep.SelectedItem == null || i.Departement == (cbFiltreDep.SelectedItem as Departement)?.Nom) &&
                    (cbFiltreDisponibilite.SelectedItem == null || i.Disponibilite == (cbFiltreDisponibilite.SelectedItem as ComboBoxItem)?.Content.ToString())
                ).ToList();

                dgPersonnel.ItemsSource = filteredInfirmiers;
                StatusMessageUpdated?.Invoke(this, "StatusFilteredResults");
            }
        }

        private void btn_Reinitialiser(object sender, RoutedEventArgs e)
        {
            cbFiltreDep.SelectedItem = null;
            cbFiltreDisponibilite.SelectedItem = null;
            tbFiltreSpecialite.Clear();

            if (TypeActif == TypePersonnel.Medecins)
            {
                dgPersonnel.ItemsSource = ListeMedecins;
                StatusMessageUpdated?.Invoke(this, "MedecinsListReset");
            }
            else if (TypeActif == TypePersonnel.Infirmiers)
            {
                dgPersonnel.ItemsSource = ListeInfirmiers;
                StatusMessageUpdated?.Invoke(this, "InfirmiersListReset");
            }

            dgPersonnel.Items.Refresh();
        }

        // Méthodes pour changer les vues entre Gestion et Filtrage
        private void btn_TabGestion(object sender, RoutedEventArgs e)
        {
            spGestion.Visibility = Visibility.Visible;
            spFiltre.Visibility = Visibility.Collapsed;
        }

        private void btn_TabFiltrer(object sender, RoutedEventArgs e)
        {
            spGestion.Visibility = Visibility.Collapsed;
            spFiltre.Visibility = Visibility.Visible;
        }

        // Rafraîchissement du DataGrid
        public void RefreshDataGrid()
        {
            dgPersonnel.Items.Refresh();
        }

        // Enum pour le type actif de personnel
        public enum TypePersonnel
        {
            Medecins,
            Infirmiers
        }

        public TypePersonnel TypeActif { get; set; }

        // Gestion des Langues d'affichages
        private void btn_Francais(object sender, RoutedEventArgs e)
        {
            ChangerLanguage?.Invoke(this, "fr");
        }

        private void btn_Anglais(object sender, RoutedEventArgs e)
        {
            ChangerLanguage?.Invoke(this, "en");
        }

        public event EventHandler<string> ChangerLanguage;
    }
}