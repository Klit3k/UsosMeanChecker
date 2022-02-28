using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using System.Text.RegularExpressions;

namespace UsosMeanChecker
{
    public class UsosChecker
    {
        FileHandler plik = new(@"./" + $"{DateTime.Now.Day}-{DateTime.Now.Month}-{DateTime.Now.Year}--{DateTime.Now.Hour}{DateTime.Now.Minute}{DateTime.Now.Second}.usosRaport");
        private string url;
        private string login = "";
        private string password = "";
        private bool zalogowano = false;
        EdgeDriver driver;
        WebDriverWait wait;
        public UsosChecker(string url)
        {
            this.url = url;
            EdgeOptions options = new EdgeOptions();
            options.AddArgument("--headless");
            driver = new EdgeDriver(options);
            wait = new WebDriverWait(driver, TimeSpan.FromMilliseconds(2000));
            wprowadzDane();
        }
        private void wprowadzDane()
        {
            Console.Clear();
            Console.Write("e-mail: ");
            this.login = Console.ReadLine();
            Console.Write("haslo: ");
            this.password = string.Empty;
            ConsoleKey key;
            do
            {
                var keyInfo = Console.ReadKey(intercept: true);
                key = keyInfo.Key;

                if (key == ConsoleKey.Backspace && password.Length > 0)
                {
                    Console.Write("\b \b");
                    password = password[0..^1];
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    Console.Write("*");
                    password += keyInfo.KeyChar;
                }
            } while (key != ConsoleKey.Enter);
            Console.WriteLine();
        }
        public async Task zaloguj()
        {
            string submitXpath = @"//html/body/table/tbody/tr/td/div[2]/form/div[2]/div[4]/div[2]/input";
            string usernameXpath = @"//*[@id='username']";
            string passwordXpath = @"//*[@id='password']";
            Console.Clear();
            Console.WriteLine("Loguje sie...");
            driver.Navigate().GoToUrl(url);
            driver.FindElement(OpenQA.Selenium.By.XPath(usernameXpath)).SendKeys(login);
            driver.FindElement(OpenQA.Selenium.By.XPath(passwordXpath)).SendKeys(password);
            driver.FindElement(OpenQA.Selenium.By.XPath(submitXpath)).Click();
            //Sprawdzenie
            if (sprawdzZalogowanie())
            {
                zalogowano = true;
                Console.WriteLine("Zalogowano!");
            }
            else
            {
                Console.WriteLine("Nie udalo sie zalogowac. Czy chcesz sprobowac ponownie ?\n[1] Tak\n[2] Nie");
                var decyzja = Console.ReadLine();
                if (decyzja.Equals("1"))
                {
                    wprowadzDane();
                    zaloguj().Wait();
                }
                else
                {
                    driver.Quit();
                }
            }
        }
        public bool czyZalogowany()
        {
            return zalogowano ? true : false;
        }
        private bool sprawdzZalogowanie()
        {
            string checkXpath = @"/html/body/div[2]/div/table/tbody/tr/td[2]";
            try
            {
                IWebElement SearchResult = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.XPath(checkXpath)));
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task wyloguj()
        {
            string wylogujXpath = "/html/body/div[2]/div/table/tbody/tr/td[2]/a[2]";
            if (zalogowano == false) Console.WriteLine("Nie jestes zalogowany !");
            else
            {
                driver.FindElement(OpenQA.Selenium.By.XPath(wylogujXpath)).Click();
                zalogowano = false;
                Console.WriteLine("Wylogowano");
            }
            driver.Quit();
        }
        public async Task idzOceny()
        {
            if (zalogowano == true)
            {
                string usernameXpath = @"/html/body/div[2]/div/table/tbody/tr/td[2]/b";
                Console.WriteLine("Przechodze do ocen...");
                driver.Navigate().GoToUrl("https://usos.wat.edu.pl/kontroler.php?_action=dla_stud/studia/oceny/index");
                Console.WriteLine("Rozwijam przedmioty...");
                //Rozwijanie semestrow
                IList<IWebElement> przedmioty = driver.FindElement(By.ClassName("grey")).FindElements(By.TagName("tbody"));
                IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                foreach (IWebElement przedmiot in przedmioty)
                {
                    js.ExecuteScript("arguments[0].style='display: block;'", przedmiot);
                }
                int i = 0;
                var rgx = new Regex(@"(\d,\d)|\d");
                var rgx2 = new Regex(@"(WYK: .*)|(PP: .*)");
                MatchCollection x;
                double suma = 0;
                int ilosc = 0;
                double srednia = 0;
                plik.wpisz($"Wygenerowano dla: {driver.FindElement(By.XPath(usernameXpath)).Text}");
                Console.WriteLine($"Wygenerowano dla: {driver.FindElement(By.XPath(usernameXpath)).Text}");
                int maxVal = 10;
                bool maxValCheck = false;
                for (int semestr = 10; semestr > 0; semestr--)
                {
                    double sumaSem = 0;
                    int iloscSem = 0;
                    double sredniaSem = 0;
                    if (driver.PageSource.Contains($"tab{semestr}") == false) continue;
                    if (maxValCheck == false)
                    {
                        maxVal = semestr;
                        maxValCheck = true;
                    }
                    foreach (IWebElement element in driver.FindElement(By.ClassName("grey")).FindElement(By.Id($"tab{semestr}")).FindElements(By.TagName("td")))
                    {
                        /*
                        if (i == 0)
                        {
                            plik.wpisz($"{element.FindElement(By.TagName("a")).Text}");
                        }
                        */
                        if (i == 2)
                        {
                            if (element.FindElements(By.TagName("div")).Count == 1)
                                foreach (Match m in rgx.Matches(element.Text))
                                {
                                    ilosc++;
                                    iloscSem++;
                                    suma += Double.Parse(m.Value);
                                    sumaSem += Double.Parse(m.Value);
                                    //plik.wpisz($"> {m.Value}");

                                }
                            else
                            {
                                foreach (Match o in rgx2.Matches(element.Text))
                                {
                                    foreach (Match o2 in rgx.Matches(o.ToString()))
                                    {
                                        suma += Double.Parse(o2.Value);
                                        sumaSem += Double.Parse(o2.Value);
                                        ilosc++;
                                        iloscSem++;
                                        //plik.wpisz($"> {o2.Value}");
                                    }
                                }
                            }
                        }
                        i++;
                        if (i == 4) i = 0;

                    }
                    sredniaSem = sumaSem / iloscSem;
                    plik.wpisz($"Semestr {Math.Abs(semestr - maxVal - 1)}: {sredniaSem}");
                    Console.WriteLine($"Semestr {Math.Abs(semestr - maxVal - 1)}: {sredniaSem}");
                }
                srednia = suma / ilosc;
                plik.wpisz($"Srednia ogolna: {srednia}");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Srednia ogolna: {srednia}");
                Console.ForegroundColor = ConsoleColor.White;
                Console.ReadKey();
            }

            else Console.WriteLine("Nie jestes zalogowany !");
        }
    }
}