namespace UsosMeanChecker
{
    static class Program
    {
        static void Main(string[] args)
        {
            UsosChecker y = new(@"https://logowanie.wat.edu.pl/cas/login?service=https%3A%2F%2Fusos.wat.edu.pl%2Fkontroler.php%3F_action%3Dlogowaniecas%2Findex&locale=pl");
            y.zaloguj().Wait();
            if (y.czyZalogowany())
            {
                y.idzOceny().Wait();
                y.wyloguj().Wait();
            }
        }
    }
}