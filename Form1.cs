namespace P_FUN
{
    public partial class Form1 : Form
    {
        // Fenêtre principale de PTL : affiche les séries de matchs sur un graphique.
        // Tous les matchs chargés depuis le dossier data.
        private List<Match> _matches = new();

        // Dossier de stockage local (copié à côté de l'exécutable).
        private readonly string _dossierData = Path.Combine(AppContext.BaseDirectory, "data");

        // Les 5 séries : nom affiché, et comment lire la valeur d'un match.
        private readonly Dictionary<string, Func<Match, double>> _series = new()
        {
            { "Minutes jouées", m => m.MinutesPlayed },
            { "Buts", m => m.Goals },
            { "Penalties", m => m.Penalties },
            { "Cartons jaunes", m => m.YellowCards },
            { "Cartons rouges", m => m.RedCards },
        };

        private readonly Dictionary<string, CheckBox> _cases = new();
        private readonly CheckBox _lisser = new() { Text = "Lisser (10 matchs)", AutoSize = true, Checked = true };
        private readonly CheckBox _normaliser = new() { Text = "Normaliser (0 à 1)", AutoSize = true };
        private readonly DateTimePicker _debut = new() { Format = DateTimePickerFormat.Short, Width = 100 };
        private readonly DateTimePicker _fin = new() { Format = DateTimePickerFormat.Short, Width = 100 };
        private readonly Button _importer = new() { Text = "Importer un CSV", AutoSize = true };

        public Form1()
        {
            InitializeComponent();
            ConstruireBarreOutils();
            Load += (s, e) => ChargerDonnees();
        }

        // Méthode vide : le Designer en a besoin (il y branche l'événement Load du graphique).
        private void formsPlot1_Load(object sender, EventArgs e)
        {
            // Le chargement des données se fait dans le Load du formulaire.
        }
        //Crée la barre du haut (séries, options, dates, import) sans passer par le Designer.
        private void ConstruireBarreOutils()
        {
            var barre = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(5) };

            // Une case à cocher par série (LINQ, pas de boucle for)
            var cases = _series.Keys.Select(CreerCase).ToArray();
            barre.Controls.AddRange(cases);

            barre.Controls.AddRange(new Control[]
            {
                _lisser,
                _normaliser,
                new Label { Text = "Du", AutoSize = true, Margin = new Padding(15, 6, 0, 0) },
                _debut,
                new Label { Text = "au", AutoSize = true, Margin = new Padding(5, 6, 0, 0) },
                _fin,
                _importer,
            });

            _lisser.CheckedChanged += (s, e) => Actualiser();
            _normaliser.CheckedChanged += (s, e) => Actualiser();
            _debut.ValueChanged += (s, e) => Actualiser();
            _fin.ValueChanged += (s, e) => Actualiser();
            _importer.Click += Importer_Click;

            Controls.Add(barre);
            formsPlot1.BringToFront(); // le graphique remplit l'espace restant sous la barre
        }

        // Crée la case à cocher d'une série et la mémorise. "Buts" est cochée au départ.
        private CheckBox CreerCase(string nomSerie)
        {
            var caseSerie = new CheckBox { Text = nomSerie, AutoSize = true, Checked = nomSerie == "Buts" };
            caseSerie.CheckedChanged += (s, e) => Actualiser();
            _cases[nomSerie] = caseSerie;
            return caseSerie;
        }

        // Lit tous les CSV du dossier data et règle les dates sur toute la carrière.
        private void ChargerDonnees()
        {
            Directory.CreateDirectory(_dossierData);
            _matches = CsvImporter.ImportDossier(_dossierData);

            if (_matches.Count > 0)
            {
                _debut.Value = _matches.First().Date;
                _fin.Value = _matches.Last().Date;
            }

            Actualiser();
        }

        // Redessine le graphique selon les cases cochées, les options et les dates.
        private void Actualiser()
        {
            var plot = formsPlot1.Plot;
            plot.Clear();

            var visibles = _matches
                .Where(m => m.Date.Date >= _debut.Value.Date && m.Date.Date <= _fin.Value.Date)
                .ToList();

            if (visibles.Count > 0)
            {
                double[] dates = visibles.Select(m => m.Date.ToOADate()).ToArray();

                _cases.Where(c => c.Value.Checked)
                      .ToList()
                      .ForEach(c => AjouterSerie(c.Key, dates, visibles));
            }

            plot.Axes.DateTimeTicksBottom();
            plot.ShowLegend();
            plot.Axes.AutoScale();
            formsPlot1.Refresh();
        }

        // Ajoute une courbe au graphique (avec lissage / normalisation si demandé).
        private void AjouterSerie(string nom, double[] dates, List<Match> matches)
        {
            IEnumerable<double> valeurs = matches.Select(_series[nom]);

            if (_lisser.Checked)
            {
                valeurs = valeurs.MoyenneGlissante(10);
            }

            if (_normaliser.Checked)
            {
                valeurs = valeurs.Normaliser();
            }

            var courbe = formsPlot1.Plot.Add.Scatter(dates, valeurs.ToArray());
            courbe.LegendText = nom;
            courbe.MarkerSize = 3;
        }

        // Importe un ou plusieurs CSV : vérifie qu'ils contiennent des matchs,
        // les copie dans le dossier data (stockage local) puis recharge tout.
        private void Importer_Click(object? sender, EventArgs e)
        {
            using var dialogue = new OpenFileDialog
            {
                Filter = "Fichiers CSV (*.csv)|*.csv",
                Multiselect = true
            };

            if (dialogue.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            try
            {
                var illisibles = dialogue.FileNames
                    .Where(f => !CsvImporter.Import(f).Any())
                    .Select(Path.GetFileName)
                    .ToList();

                if (illisibles.Count > 0)
                {
                    MessageBox.Show("Fichier illisible ou sans match : " + string.Join(", ", illisibles),
                        "Import impossible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                //crée le dossier s’il manque
                Directory.CreateDirectory(_dossierData);
                dialogue.FileNames.ToList()
                    .ForEach(f => File.Copy(f, Path.Combine(_dossierData, Path.GetFileName(f)), true));

                ChargerDonnees();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur pendant l'import : " + ex.Message,
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

