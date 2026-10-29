using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P_FUN
{
    static class Extension
    {
        public static int Conversion(this string champs)
        {
            /// Convertit un texte en entier. Retourne 0 si le texte est vide ou invalide
            bool succes = int.TryParse(champs, out int result);
            if (succes == true )
            {
                return result;
            }
            else
            {
                return 0;
            }
        }
        public static IEnumerable<double> MoyenneGlissante(this IEnumerable<double> valeurs, int fenetre)
        {
            /// Moyenne glissante : chaque valeur devient la moyenne des "fenetre" dernières valeurs
            var liste = valeurs.ToList();

            return Enumerable.Range(0, liste.Count)
                .Select(i => liste
                    .Skip(Math.Max(0, i - fenetre + 1))
                    .Take(Math.Min(fenetre, i + 1))
                    .Average());
        }
        public static IEnumerable<double> Normaliser(this IEnumerable<double> valeurs)
        {
            /// Ramène les valeurs entre 0 et 1 pour pouvoir comparer des séries d'échelles différentes
            var liste = valeurs.ToList();
            if (liste.Count == 0)
            {
                return liste;
            }

            double min = liste.Min();
            double max = liste.Max();

            return max == min
                ? liste.Select(_ => 0.0)
                : liste.Select(v => (v - min) / (max - min));
        }
    }
}
