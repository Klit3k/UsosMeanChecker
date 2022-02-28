namespace UsosMeanChecker
{
    public class FileHandler
    {
        private string sciezka;
        public FileHandler(string sciezka)
        {
            this.sciezka = sciezka;
        }
        public string odczytaj()
        {
            if (File.Exists(sciezka))
            {
                return File.ReadAllText(sciezka);
            }
            else
            {
                Console.WriteLine("Plik nie istnieje !");
                return null;
            }
        }

        public void wpisz(string tresc)
        {
            if (!File.Exists(sciezka))
                Console.WriteLine("Tworze plik...");
            using (StreamWriter sw = File.AppendText(this.sciezka))
            {
                sw.WriteLine(tresc);
            }
        }
    }
}