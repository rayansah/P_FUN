using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.CompilerServices;

namespace P_FUN
{
    internal class CsvImporter
    {
       
        // Découpe une ligne CSV en champs.
        static string[] Separer(string line)
        {
            return line.Split(',');
        }
        // Construit un Match à partir des champs d'une ligne.
        static Match CreateObj(string[] champs)
        {
            return new Match
            {
                Date = DateTime.Parse(champs[0]),
                MinutesPlayed = champs[10].Conversion(),
                Goals = champs[11].Conversion(),
                Penalties = champs[13].Conversion(),
                YellowCards = champs[17].Conversion(),
                RedCards = champs[18].Conversion(),
            };
        }
        // Importe un fichier CSV. Les lignes sans date valide (en-tête, lignes vides, totaux)
        public static List<Match> Import(string cheminFichier)
        {
            return File.ReadAllLines(cheminFichier)
                .Select(Separer)
                .Where(champs => champs.Length > 18 && DateTime.TryParse(champs[0], out _))
                .Select(CreateObj)
                .ToList();
        }
        // Importe tous les CSV d'un dossier, triés par date.
        public static List<Match> ImportDossier(string cheminDossier)
        {
            return Directory.GetFiles(cheminDossier, "*.csv")
           .SelectMany(chemin => Import(chemin))
           .OrderBy(match => match.Date)
           .ToList();
        }
    }
}
