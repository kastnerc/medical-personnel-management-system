using GestionPersonnelMedical.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using static GestionPersonnelMedical.PersonnelTab;

namespace GestionPersonnelMedical
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Collections de données principales
        public ObservableCollection<Medecin> ListeMedecins { get; set; }
        public ObservableCollection<Infirmier> ListeInfirmiers { get; set; }
        public ObservableCollection<Departement> ListeDepartements { get; set; }
        public ObservableCollection<Consultation> ListeConsultations { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            // Initialisation des contrôles et bindings
            InitialiserInterface();

            // Liaison des événements pour les onglets Médecins et Infirmiers
            ConfigurerPersonnelTabs();
        }

        // Initialisation de l'interface utilisateur
        private void InitialiserInterface()
        {
            cbMedecin.ItemsSource = ListeMedecins;
            cbMedecin.DisplayMemberPath = "NomComplet"; // Propriété à afficher dans le ComboBox
            cbMedecin.SelectedValuePath = "NomComplet"; // Propriété sélectionnée

            dgConsultation.ItemsSource = ListeConsultations;
            dgDepartement.ItemsSource = ListeDepartements;

            DataContext = this; // Définir le DataContext global
            this.Loaded += async (s, e) => await ChargerDonneesAsync();
        }
        private async Task ChargerDonneesAsync()
        {
            using (var context = new ApplicationDbContext())
            {
                // Charger les départements
                var departements = await context.Departements.ToListAsync();
                ListeDepartements = new ObservableCollection<Departement>(departements);

                // Charger les médecins
                var medecins = await context.Medecins.ToListAsync();
                ListeMedecins = new ObservableCollection<Medecin>(medecins);

                // Charger les infirmiers
                var infirmiers = await context.Infirmiers.ToListAsync();
                ListeInfirmiers = new ObservableCollection<Infirmier>(infirmiers);

                // Charger les consultations
                var consultations = await context.Consultations.ToListAsync();
                ListeConsultations = new ObservableCollection<Consultation>(consultations);
            }

            // Rafraîchir les contrôles de l'interface utilisateur
            RafraichirInterface();
        }

        private void RafraichirInterface()
        {
            cbMedecin.ItemsSource = ListeMedecins;
            dgConsultation.ItemsSource = ListeConsultations;
            dgDepartement.ItemsSource = ListeDepartements;

            // Rafraîchir les onglets Médecins et Infirmiers
            ConfigurerPersonnelTabs();
            DataContext = this; // Mise à jour du contexte de données
        }

        // Configuration des onglets Médecins et Infirmiers
        private void ConfigurerPersonnelTabs()
        {
            var personnelTabMedecins = (PersonnelTab)tabMedecins.Content;
            ConfigurerPersonnelTab(personnelTabMedecins, ListeMedecins, TypePersonnel.Medecins);

            var personnelTabInfirmiers = (PersonnelTab)tabInfirmiers.Content;
            ConfigurerPersonnelTab(personnelTabInfirmiers, ListeInfirmiers, TypePersonnel.Infirmiers);
        }

        private void ConfigurerPersonnelTab(PersonnelTab tab, object liste, TypePersonnel type)
        {
            // Désabonnez-vous des événements pour éviter les abonnements multiples
            tab.AjouterPersonnel -= PersonnelTab_AjouterPersonnel;
            tab.SupprimerPersonnel -= PersonnelTab_SupprimerPersonnel;
            tab.ModifierPersonnel -= PersonnelTab_ModifierPersonnel;
            tab.StatusMessageUpdated -= UpdateStatusBar;
            tab.ChangerLanguage -= PersonnelTab_ChangeLanguage;

            // Réabonnez-vous aux événements
            tab.AjouterPersonnel += PersonnelTab_AjouterPersonnel;
            tab.SupprimerPersonnel += PersonnelTab_SupprimerPersonnel;
            tab.ModifierPersonnel += PersonnelTab_ModifierPersonnel;
            tab.StatusMessageUpdated += UpdateStatusBar;
            tab.ChangerLanguage += PersonnelTab_ChangeLanguage;

            // Configuration spécifique au type actif (Médecins ou Infirmiers)
            if (type == TypePersonnel.Medecins)
            {
                // Activer les champs liés aux spécialités
                tab.SpecialiteTextBox.IsEnabled = true;
                tab.SpecialiteLabel.IsEnabled = true;
                tab.SpecialiteFiltreTextBox.IsEnabled = true;
                tab.SpecialiteFiltreLabel.IsEnabled = true;
                tab.SpecialiteDataGridColumn.Visibility = Visibility.Visible;

                // Lier la liste des médecins
                tab.ListeMedecins = (ObservableCollection<Medecin>)liste;
                tab.dgPersonnel.ItemsSource = tab.ListeMedecins; // Liaison au DataGrid
            }
            else
            {
                // Désactiver les champs liés aux spécialités
                tab.SpecialiteTextBox.IsEnabled = false;
                tab.SpecialiteLabel.IsEnabled = false;
                tab.SpecialiteFiltreTextBox.IsEnabled = false;
                tab.SpecialiteFiltreLabel.IsEnabled = false;
                tab.SpecialiteDataGridColumn.Visibility = Visibility.Collapsed;

                // Lier la liste des infirmiers
                tab.ListeInfirmiers = (ObservableCollection<Infirmier>)liste;
                tab.dgPersonnel.ItemsSource = tab.ListeInfirmiers; // Liaison au DataGrid
            }

            // Mettre à jour le type actif et lier les départements
            tab.TypeActif = type;
            tab.SetDepartements(ListeDepartements);

            // Rafraîchir l'affichage
            tab.RefreshDataGrid();
        }

        // Gestion de la StatusBar
        private void UpdateStatusBar(object sender, string resourceKey)
        {
            sbStatus.Text = Properties.Resources.ResourceManager.GetString(resourceKey, Properties.Resources.Culture);
        }

        // Gestion des Médecins et Infirmiers
        private void RefreshPersonnelTabs()
        {
            ((PersonnelTab)tabMedecins.Content).RefreshDataGrid();
            ((PersonnelTab)tabInfirmiers.Content).RefreshDataGrid();
        }

        private async void PersonnelTab_AjouterPersonnel(object sender, PersonnelTab.PersonnelEventArgs e)
        {
            using (var context = new ApplicationDbContext())
            {
                // Recherche le département dans la base de données en utilisant le Nom
                var departement = await context.Departements.FirstOrDefaultAsync(d => d.Id == e.Departement);

                // Vérifie si le département existe
                if (departement == null)
                {
                    UpdateStatusBar(this, "ErrorDepartmentNotFound");
                    return;
                }

                // Ajouter un médecin ou un infirmier selon l'onglet actif
                if (tabControl.SelectedItem == tabMedecins)
                {
                    var nouveauMedecin = new Medecin
                    {
                        Nom = e.Nom,
                        Prenom = e.Prenom,
                        Departement = departement.Id, // Utilise le Nom du département
                        Disponibilite = e.Disponibilite,
                        Specialite = e.Specialite
                    };

                    context.Medecins.Add(nouveauMedecin);
                    departement.NombreMedecins++; // Met à jour le nombre de médecins
                }
                else if (tabControl.SelectedItem == tabInfirmiers)
                {
                    var nouvelInfirmier = new Infirmier
                    {
                        Nom = e.Nom,
                        Prenom = e.Prenom,
                        Departement = departement.Id, // Utilise le Nom du département
                        Disponibilite = e.Disponibilite
                    };

                    context.Infirmiers.Add(nouvelInfirmier);
                    departement.NombreInfirmiers++; // Met à jour le nombre d'infirmiers
                }

                // Sauvegarde les modifications dans la base de données
                await context.SaveChangesAsync();

                // Recharge les données dans l'interface utilisateur
                await ChargerDonneesAsync();
                RafraichirInterface();

                // Mise à jour du statut
                UpdateStatusBar(this, "StatusAddedSuccessfully");
            }
        }

        private async void PersonnelTab_SupprimerPersonnel(object sender, object personnel)
        {
            if (personnel == null)
            {
                UpdateStatusBar(this, "SelectPersonnelPrompt");
                return;
            }

            using (var context = new ApplicationDbContext())
            {
                if (tabControl.SelectedItem == tabMedecins && personnel is Medecin medecin)
                {
                    var medecinDb = await context.Medecins.FindAsync(medecin.Id);
                    if (medecinDb != null)
                    {
                        context.Medecins.Remove(medecinDb);

                        // Mettre à jour le département associé
                        var departement = await context.Departements.FindAsync(medecin.Departement);
                        if (departement != null && departement.NombreMedecins > 0)
                        {
                            departement.NombreMedecins--;
                        }

                        await context.SaveChangesAsync();

                        // Supprimer de la liste en mémoire
                        ListeMedecins.Remove(medecin);
                        UpdateStatusBar(this, "PersonnelDeletedSuccessfully");
                    }
                }
                else if (tabControl.SelectedItem == tabInfirmiers && personnel is Infirmier infirmier)
                {
                    var infirmierDb = await context.Infirmiers.FindAsync(infirmier.Id);
                    if (infirmierDb != null)
                    {
                        context.Infirmiers.Remove(infirmierDb);

                        // Mettre à jour le département associé
                        var departement = await context.Departements.FindAsync(infirmier.Departement);
                        if (departement != null && departement.NombreInfirmiers > 0)
                        {
                            departement.NombreInfirmiers--;
                        }

                        await context.SaveChangesAsync();

                        // Supprimer de la liste en mémoire
                        ListeInfirmiers.Remove(infirmier);
                        UpdateStatusBar(this, "PersonnelDeletedSuccessfully");
                    }
                }
                else
                {
                    UpdateStatusBar(this, "ErrorPersonnelNotFound");
                }
            }

            // Rafraîchir les données du DataGrid et des départements
            dgDepartement.Items.Refresh();
            RefreshPersonnelTabs();
        }

        private async void PersonnelTab_ModifierPersonnel(object sender, PersonnelTab.ModifierPersonnelEventArgs e)
        {
            if (e.AncienPersonnel == null || e.NouveauPersonnel == null)
            {
                UpdateStatusBar(this, "ErrorNoPersonnelSelectedForEdit");
                return;
            }

            using (var context = new ApplicationDbContext())
            {
                bool modificationReussie = false;

                if (e.AncienPersonnel is Medecin ancienMedecin && e.NouveauPersonnel is Medecin nouveauMedecin)
                {
                    // Rechercher le médecin dans la base de données
                    var medecinDb = await context.Medecins.FindAsync(ancienMedecin.Id);
                    if (medecinDb != null)
                    {
                        // Mise à jour des propriétés du médecin
                        medecinDb.Nom = nouveauMedecin.Nom;
                        medecinDb.Prenom = nouveauMedecin.Prenom;
                        medecinDb.Departement = nouveauMedecin.Departement; // ID du département
                        medecinDb.Disponibilite = nouveauMedecin.Disponibilite;
                        medecinDb.Specialite = nouveauMedecin.Specialite;

                        // Mettre à jour les consultations associées
                        var consultations = await context.Consultations
                            .Where(c => c.Medecin == $"{ancienMedecin.Prenom} {ancienMedecin.Nom}")
                            .ToListAsync();

                        foreach (var consultation in consultations)
                        {
                            consultation.Medecin = $"{nouveauMedecin.Prenom} {nouveauMedecin.Nom}";
                        }

                        // Gestion du changement de département
                        if (ancienMedecin.Departement != nouveauMedecin.Departement)
                        {
                            var ancienDepartement = await context.Departements.FirstOrDefaultAsync(d => d.Id == ancienMedecin.Departement);
                            var nouveauDepartement = await context.Departements.FirstOrDefaultAsync(d => d.Id == nouveauMedecin.Departement);

                            if (ancienDepartement != null) ancienDepartement.NombreMedecins--;
                            if (nouveauDepartement != null) nouveauDepartement.NombreMedecins++;
                        }

                        // Sauvegarder les modifications
                        await context.SaveChangesAsync();
                        modificationReussie = true;
                    }
                }
                else if (e.AncienPersonnel is Infirmier ancienInfirmier && e.NouveauPersonnel is Infirmier nouvelInfirmier)
                {
                    // Rechercher l'infirmier dans la base de données
                    var infirmierDb = await context.Infirmiers.FindAsync(ancienInfirmier.Id);
                    if (infirmierDb != null)
                    {
                        // Mise à jour des propriétés de l'infirmier
                        infirmierDb.Nom = nouvelInfirmier.Nom;
                        infirmierDb.Prenom = nouvelInfirmier.Prenom;
                        infirmierDb.Departement = nouvelInfirmier.Departement; // ID du département
                        infirmierDb.Disponibilite = nouvelInfirmier.Disponibilite;

                        // Gestion du changement de département
                        if (ancienInfirmier.Departement != nouvelInfirmier.Departement)
                        {
                            var ancienDepartement = await context.Departements.FirstOrDefaultAsync(d => d.Id == ancienInfirmier.Departement);
                            var nouveauDepartement = await context.Departements.FirstOrDefaultAsync(d => d.Id == nouvelInfirmier.Departement);

                            if (ancienDepartement != null) ancienDepartement.NombreInfirmiers--;
                            if (nouveauDepartement != null) nouveauDepartement.NombreInfirmiers++;
                        }

                        // Sauvegarder les modifications
                        await context.SaveChangesAsync();
                        modificationReussie = true;
                    }
                }

                // Mise à jour du statut
                UpdateStatusBar(this, modificationReussie ? "PersonnelModifiedSuccessfully" : "ErrorPersonnelNotFound");
            }

            // Recharger les données dans l'interface utilisateur
            await ChargerDonneesAsync();
        }

        private void PersonnelTab_ChangeLanguage(object sender, string cultureCode)
        {
            // Changer la culture courante pour les ressources
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureCode);
            Properties.Resources.Culture = new CultureInfo(cultureCode);

            // Recharger l'interface utilisateur
            var newWindow = new MainWindow();
            Application.Current.MainWindow = newWindow;
            newWindow.Show();

            // Fermer l'ancienne fenêtre
            this.Close();
        }

        // Gestion des Départements
        private async void btn_AjouterDep(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbNom.Text) && !string.IsNullOrEmpty(tbDescription.Text))
            {
                using (var context = new ApplicationDbContext())
                {
                    // Vérifie si un département avec le même nom existe déjà
                    bool departementExiste = await context.Departements.AnyAsync(d => d.Nom == tbNom.Text);
                    if (departementExiste)
                    {
                        UpdateStatusBar(this, "ErrorDepartmentAlreadyExists");
                        return;
                    }

                    var nouveauDepartement = new Departement
                    {
                        Id = tbNom.Text, // Utilise le Nom comme ID
                        Nom = tbNom.Text,
                        Description = tbDescription.Text
                    };

                    context.Departements.Add(nouveauDepartement);
                    await context.SaveChangesAsync();

                    // Rafraîchir les données
                    await ChargerDonneesAsync();
                }

                UpdateStatusBar(this, "StatusAddedSuccessfully");
            }
            else
            {
                UpdateStatusBar(this, "StatusFillAllFields");
            }
        }

        private async void btn_ModifierDep(object sender, RoutedEventArgs e)
        {
            if (dgDepartement.SelectedItem is not Departement selectedDepartement)
            {
                UpdateStatusBar(this, "ErrorNoDepartmentSelectedForEdit");
                return;
            }

            if (string.IsNullOrWhiteSpace(tbNom.Text) || string.IsNullOrWhiteSpace(tbDescription.Text))
            {
                UpdateStatusBar(this, "StatusFillAllFields");
                return;
            }

            using (var context = new ApplicationDbContext())
            {
                // Vérifiez si un autre département utilise déjà le nouveau nom
                var existingDepartement = await context.Departements.FindAsync(tbNom.Text);
                if (existingDepartement != null)
                {
                    UpdateStatusBar(this, "ErrorDepartmentNameAlreadyExists");
                    return;
                }

                // Récupérer les propriétés actuelles du département
                var nombreMedecins = selectedDepartement.NombreMedecins;
                var nombreInfirmiers = selectedDepartement.NombreInfirmiers;

                // Créez un nouveau département avec les propriétés mises à jour
                var newDepartement = new Departement
                {
                    Id = tbNom.Text,
                    Nom = tbNom.Text,
                    Description = tbDescription.Text,
                    NombreMedecins = nombreMedecins,
                    NombreInfirmiers = nombreInfirmiers
                };
                context.Departements.Add(newDepartement);

                // Mettre à jour les dépendances (médecins et infirmiers)
                var medecins = await context.Medecins.Where(m => m.Departement == selectedDepartement.Id).ToListAsync();
                foreach (var medecin in medecins)
                {
                    medecin.Departement = newDepartement.Id;
                }

                var infirmiers = await context.Infirmiers.Where(i => i.Departement == selectedDepartement.Id).ToListAsync();
                foreach (var infirmier in infirmiers)
                {
                    infirmier.Departement = newDepartement.Id;
                }

                // Supprimer l'ancien département
                context.Departements.Remove(selectedDepartement);

                // Enregistrez toutes les modifications
                await context.SaveChangesAsync();

                // Recharger les données dans l'interface utilisateur
                await ChargerDonneesAsync();
                dgDepartement.Items.Refresh();
                RefreshPersonnelTabs();

                UpdateStatusBar(this, "DepartmentModifiedSuccessfully");
            }
        }

        private async void btn_SupprimerDep(object sender, RoutedEventArgs e)
        {
            // Vérifiez que l'utilisateur a sélectionné un département
            Departement? departementSupprimer = dgDepartement.SelectedItem as Departement;

            if (departementSupprimer != null)
            {
                using (var context = new ApplicationDbContext())
                {
                    // Vérifiez les relations avec les médecins et infirmiers
                    bool hasMedecins = await context.Medecins.AnyAsync(m => m.Departement == departementSupprimer.Id);
                    bool hasInfirmiers = await context.Infirmiers.AnyAsync(i => i.Departement == departementSupprimer.Id);

                    if (!hasMedecins && !hasInfirmiers)
                    {
                        // Supprimez le département
                        var departementDb = await context.Departements.FindAsync(departementSupprimer.Id);
                        if (departementDb != null)
                        {
                            context.Departements.Remove(departementDb);
                            await context.SaveChangesAsync();

                            // Rafraîchir la liste des départements
                            await ChargerDonneesAsync();

                            UpdateStatusBar(this, "DepartmentDeletedSuccessfully");
                        }
                    }
                    else
                    {
                        UpdateStatusBar(this, "ErrorDepartmentContainsPersonnel");
                    }
                }
            }
            else
            {
                UpdateStatusBar(this, "SelectDepartmentPrompt");
            }
        }

        private void dgDepartement_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var departementSelectionne = dgDepartement.SelectedItem as Departement;

            if (departementSelectionne != null)
            {
                tbNom.Text = departementSelectionne.Nom;
                tbDescription.Text = departementSelectionne.Description;
            }
            else
            {
                tbNom.Clear();
                tbDescription.Clear();
            }
        }

        private void btn_RechDep(object sender, RoutedEventArgs e)
        {
            if (dgDepartement.ItemsSource == null)
            {
                UpdateStatusBar(this, "StatusNoDataToFilter");
                return;
            }
            var filteredDepartements = ListeDepartements.Where(d =>
                 (string.IsNullOrEmpty(cbFiltreNom.Text) || d.Nom.Contains(cbFiltreNom.Text, StringComparison.OrdinalIgnoreCase)) &&
                 (string.IsNullOrEmpty(cbFiltreNumMed.Text) || int.TryParse(cbFiltreNumMed.Text, out int numMed) && d.NombreMedecins == numMed) &&
                 (string.IsNullOrEmpty(cbFiltreNumInf.Text) || int.TryParse(cbFiltreNumInf.Text, out int numInf) && d.NombreInfirmiers == numInf)
             ).ToList();
            dgDepartement.ItemsSource = filteredDepartements;
            UpdateStatusBar(this, "StatusFilteredResults");
        }

        private void btn_ReinitDep(object sender, RoutedEventArgs e)
        {
            cbFiltreNom.Clear();
            cbFiltreNumMed.Clear();
            cbFiltreNumInf.Clear();

            dgDepartement.ItemsSource = ListeDepartements;

            UpdateStatusBar(this, "DepartmentsListReset");
        }

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

        // Gestion des Consultations
        private async void btn_AjoutConsultation_Click(object sender, RoutedEventArgs e)
        {
            // Vérifiez que tous les champs requis sont remplis
            if (string.IsNullOrEmpty(tbPatient.Text) || cbMedecin.SelectedItem == null || dpDate.SelectedDate == null || cbHeure.SelectedItem == null)
            {
                UpdateStatusBar(this, "StatusFillAllFields");
                return;
            }

            using (var context = new ApplicationDbContext())
            {
                try
                {
                    // Vérifiez que le médecin sélectionné existe dans la base de données
                    var medecinSelectionne = cbMedecin.SelectedItem as Medecin;
                    if (medecinSelectionne == null)
                    {
                        UpdateStatusBar(this, "ErrorMedecinNotFound");
                        return;
                    }

                    // Créez une nouvelle consultation
                    var nouvelleConsultation = new Consultation
                    {
                        Patient = tbPatient.Text,
                        Medecin = medecinSelectionne.Nom, // Utiliser l'ID du médecin
                        Date = dpDate.SelectedDate.Value,
                        Heure = (cbHeure.SelectedItem as ComboBoxItem)?.Content.ToString()
                    };

                    // Ajoutez la consultation à la base de données
                    context.Consultations.Add(nouvelleConsultation);
                    await context.SaveChangesAsync();

                    // Ajoutez la consultation à la liste en mémoire (si nécessaire pour l'affichage immédiat)
                    ListeConsultations.Add(nouvelleConsultation);

                    UpdateStatusBar(this, "StatusAddedSuccessfully");

                    // Réinitialisez les champs de saisie
                    tbPatient.Clear();
                    cbMedecin.SelectedItem = null;
                    dpDate.SelectedDate = null;
                    cbHeure.SelectedItem = null;
                }
                catch (Exception ex)
                {
                    // Gérez les exceptions et mettez à jour la barre de statut
                    UpdateStatusBar(this, "ErrorAddingConsultation");
                    Console.WriteLine($"Erreur lors de l'ajout de la consultation : {ex.Message}");
                }
            }
        }

        private async void btn_SupprConsultation_Click(object sender, RoutedEventArgs e)
        {
            // Récupérez la consultation sélectionnée dans le DataGrid
            var consultationSelectionnee = dgConsultation.SelectedItem as Consultation;
            if (consultationSelectionnee == null)
            {
                UpdateStatusBar(this, "ErrorSelectConsultationToDelete");
                return;
            }

            using (var context = new ApplicationDbContext())
            {
                try
                {
                    // Recherchez la consultation dans la base de données
                    var consultationDb = await context.Consultations
                        .FirstOrDefaultAsync(c => c.Patient == consultationSelectionnee.Patient &&
                                                  c.Medecin == consultationSelectionnee.Medecin &&
                                                  c.Date == consultationSelectionnee.Date &&
                                                  c.Heure == consultationSelectionnee.Heure);

                    // Vérifiez si la consultation existe
                    if (consultationDb == null)
                    {
                        UpdateStatusBar(this, "ErrorConsultationNotFound");
                        return;
                    }

                    // Supprimez la consultation de la base de données
                    context.Consultations.Remove(consultationDb);
                    await context.SaveChangesAsync();

                    // Supprimez la consultation de la liste en mémoire
                    ListeConsultations.Remove(consultationSelectionnee);

                    // Mettre à jour la barre de statut
                    UpdateStatusBar(this, "ConsultationDeletedSuccessfully");
                }
                catch (Exception ex)
                {
                    // Gestion des erreurs
                    UpdateStatusBar(this, "ErrorDeletingConsultation");
                    Console.WriteLine($"Erreur lors de la suppression de la consultation : {ex.Message}");
                }
            }

            // Rafraîchir l'affichage du DataGrid
            dgConsultation.Items.Refresh();
        }

        private async void btn_ModifConsultation_Click(object sender, RoutedEventArgs e)
        {
            // Vérifiez qu'une consultation est sélectionnée
            if (dgConsultation.SelectedItem is not Consultation selectedConsultation)
            {
                UpdateStatusBar(this, "ErrorNoConsultationSelectedForEdit");
                return;
            }

            // Vérifiez que tous les champs nécessaires sont remplis
            if (string.IsNullOrWhiteSpace(tbPatient.Text) || cbMedecin.SelectedItem == null || dpDate.SelectedDate == null || cbHeure.SelectedItem == null)
            {
                UpdateStatusBar(this, "StatusFillAllFields");
                return;
            }

            using (var context = new ApplicationDbContext())
            {
                // Recherchez la consultation à modifier dans la base de données
                var consultationDb = await context.Consultations
                    .FirstOrDefaultAsync(c => c.Patient == selectedConsultation.Patient &&
                                              c.Medecin == selectedConsultation.Medecin &&
                                              c.Date == selectedConsultation.Date &&
                                              c.Heure == selectedConsultation.Heure);

                if (consultationDb == null)
                {
                    UpdateStatusBar(this, "ErrorConsultationNotFound");
                    return;
                }

                // Supprimer l'ancienne consultation
                context.Consultations.Remove(consultationDb);

                // Créer une nouvelle consultation avec les valeurs mises à jour
                var nouvelleConsultation = new Consultation
                {
                    Patient = tbPatient.Text,
                    Medecin = cbMedecin.SelectedValue.ToString(),
                    Date = dpDate.SelectedDate.Value,
                    Heure = (cbHeure.SelectedItem as ComboBoxItem)?.Content.ToString()
                };

                // Ajouter la nouvelle consultation
                context.Consultations.Add(nouvelleConsultation);

                // Sauvegarder les modifications dans la base de données
                await context.SaveChangesAsync();

                // Recharger les données dans l'interface utilisateur
                await ChargerDonneesAsync();

                // Rafraîchir l'affichage du DataGrid
                dgConsultation.Items.Refresh();

                // Mettre à jour la barre de statut
                UpdateStatusBar(this, "ConsultationModifiedSuccessfully");
            }
        }

        private void dgConsultation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var consultationSelectionnee = dgConsultation.SelectedItem as Consultation;

            if (consultationSelectionnee != null)
            {
                tbPatient.Text = consultationSelectionnee.Patient;
                cbMedecin.SelectedValue = consultationSelectionnee.Medecin;
                dpDate.SelectedDate = consultationSelectionnee.Date;
                cbHeure.SelectedValue = consultationSelectionnee.Heure;
            }
            else
            {
                tbPatient.Clear();
                cbMedecin.SelectedItem = null;
                dpDate.SelectedDate = null;
                cbHeure.SelectedItem = null;
            }
        }

        // Gestion des Langues d'affichages
        private void btn_Francais(object sender, RoutedEventArgs e)
        {
            ChangeLanguage("fr");
        }

        private void btn_Anglais(object sender, RoutedEventArgs e)
        {
            ChangeLanguage("en");
        }

        private void ChangeLanguage(string cultureCode)
        {
            // Changer la culture courante pour les ressources
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureCode);
            Properties.Resources.Culture = new CultureInfo(cultureCode);

            // Recharger l'interface utilisateur
            var newWindow = new MainWindow();
            Application.Current.MainWindow = newWindow;
            newWindow.Show();

            // Fermer l'ancienne fenêtre
            this.Close();
        }
    }
}