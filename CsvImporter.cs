using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P_FUN
{
    internal class CsvImporter
    {
        static string[] Separer(string line)
        {
            return line.Split(',');
        }
        static Match CreateObj(string[] champs)
        {
            return new Match
            {
                Date = DateTime.Parse(champs[0]),
                MinutesPlayed = int.Parse(champs[10]),
                Goals = int.Parse(champs[11]),
                Penalties = int.Parse(champs[13]),
                YellowCards = int.Parse(champs[17]),
                RedCards = int.Parse(champs[18]),
            };
        }
        static List<Match> Import(string cheminFichier)
        {
            return File.ReadAllLines(cheminFichier)
                .Where(ligne => DateTime.TryParse(ligne.Split(',')[0], out DateTime d))
                .Select(ligne => CreateObj(Separer(ligne)))
                .ToList();
        }
    }
}
