namespace P_FUN
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var matches CsvImporter.Import("D:/ETMLFID2/I323/Projet/P_FUN/P_FUN/data/2002-2003.csv");
        }
    }
}