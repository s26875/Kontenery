// See https://aka.ms/new-console-template for more information

/*
 * Przygotowujemy aplikacje do zarządzania załadunkiem kontenerów.
 * Kontenery mogą być później transportowane za pomocą różnego
 * rodzaju pojazdów - statków, pociągów, ciężarówek itp.
 */

/*
 * System:
 * 1. Zaladunek kontenerów na kontenerowiec
 * 2. Róne typy kontenerów zaley od ładunku
 * 3. Kontenery: chłodniczy - C, płyny - L, gaz - G.
 *
 * Charakterestyka:
 * 1. Masa ładunku - kg;
 * 2. Wysokość - cm;
 * 3. Waga własna - waga kont., w kg.;
 * 4. Głębokość - cm;
 * 5. Numer seryjny
 *   a) format numeru KON-C-1
 *   b) "C" - rodzaj kontenera;
 *   c) Liczba, unikalne, nie moze sie powtarzac;
 *   d) Max., ladownosc danego kontenera - kg.,
 *
 */

using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;
public class OverfillException : Exception
{
    public OverfillException(string message) : base(message)
    {
    }
}

public interface IHazardNotifier
{
    void NotifyHazard(string message); 
}

public abstract class Kontener
{
    public double MasaLadunku { get; protected set; }

    public int Wysokosc { get; protected set; }

    public double WagaWlasna { get; protected set; }

    public int Glebokosc { get; protected set; } 

    public string NumerSeryjny { get; protected set; }

    public double MaxLadownosc { get; protected set; }

    public Kontener(string numerSeryjny, double maxLadownosc, int glebokosc, double wagaWlasna, int wysokosc, double masaLadunku)
    {
        this.NumerSeryjny = numerSeryjny;
        this.MaxLadownosc = maxLadownosc;
        this.Glebokosc = glebokosc;
        this.WagaWlasna = wagaWlasna;
        this.Wysokosc = wysokosc;
        this.MasaLadunku = masaLadunku;
    }
    
    public abstract void Ladunek(double waga);
    public abstract void Rozladunek();
}

public class Plyny : Kontener, IHazardNotifier
{
    public Plyny(string numerSeryjny, double maxLadownosc, int glebokosc, double wagaWlasna, int wysokosc) : base(numerSeryjny, maxLadownosc, glebokosc, wagaWlasna, wysokosc, 10)
    {
    }

    public override void Ladunek(double mass)
    {
        if (mass > MaxLadownosc * 0.9)
        {
            throw new OverfillException("Nie można załadować więcej niż 90% pojemności kontenera.");
        }

        MasaLadunku = mass;
    }

    public override void Rozladunek()
    {
        MasaLadunku = 0; 
    }

    public void NotifyHazard(string message)
    {
        Console.WriteLine($"Powiadomienie o zagrożeniu dla {NumerSeryjny}: {message}"); 
    }
}

public class Gaz : Kontener, IHazardNotifier
{
    public double Cisnienie { get; protected set; } 

    public Gaz(string numerSeryjny, double maxLadownosc, int glebokosc, double wagaWlasna, int wysokosc, double cisnienie) 
        : base(numerSeryjny, maxLadownosc, glebokosc, wagaWlasna, wysokosc, 8) 
    {
        Cisnienie = cisnienie;
    }

    public override void Ladunek(double mass)
    {
        
        if (mass > MaxLadownosc)
        {
            throw new OverfillException("Nie można załadować więcej niż pojemność kontenera.");
        }
        MasaLadunku = mass;
    }

    public override void Rozladunek()
    {
        
        MasaLadunku *= 0.05;
    }

    public void NotifyHazard(string message)
    {
        Console.WriteLine($"Powiadomienie o zagrożeniu dla {NumerSeryjny}: {message}");
    }
}

public class Chlodniczy : Kontener, IHazardNotifier
{
    public string RodzajProduktu { get; private set; }
    public double Temperatura { get; private set; }
    private Dictionary<string, double> ladunek = new Dictionary<string, double>();

    private Dictionary<string, double> wymaganeTemperatury = new Dictionary<string, double>
    {
        {"Banany", 13.3},
        {"Czekolada", 18},
        {"Ryby", 2},
        {"Mięso", -15},
        {"Lody", -18},
        {"Mrozona Pizza", -30},
        {"Ser", 7.2}
    };

    public Chlodniczy(string numerSeryjny, double maxLadownosc, int glebokosc, double wagaWlasna, int wysokosc, string rodzajProduktu, double temperatura) 
        : base(numerSeryjny, maxLadownosc, glebokosc, wagaWlasna, wysokosc, 2)
    {
        RodzajProduktu = rodzajProduktu;
        Temperatura = temperatura;
    }

    public override void Ladunek(double waga)
    {
        if (waga + MasaLadunku > MaxLadownosc)
        {
            throw new OverfillException("Próba załadunku przekraczającego maksymalną ładowność kontenera.");
        }

        if (!wymaganeTemperatury.TryGetValue(RodzajProduktu, out double wymaganaTemperatura))
        {
            throw new ArgumentException("Nieznany rodzaj produktu.");
        }

        if (Temperatura < wymaganaTemperatura)
        {
            NotifyHazard($"Temperatura {Temperatura}°C jest za niska dla produktu {RodzajProduktu}, wymagana temperatura to {wymaganaTemperatura}°C.");
            return; 
        }

        if (!ladunek.ContainsKey(RodzajProduktu))
        {
            ladunek[RodzajProduktu] = waga;
        }
        else
        {
            ladunek[RodzajProduktu] += waga;
        }

        MasaLadunku += waga;
        Console.WriteLine($"Załadowano {waga}kg produktu {RodzajProduktu}. Aktualna masa ładunku: {MasaLadunku}kg.");
    }
    
    public override void Rozladunek()
    {
        MasaLadunku = 0;
    }

    public void ZmienProdukt(string nowyRodzajProduktu, double nowaTemperatura)
    {
        if (MasaLadunku > 0)
        {
            throw new InvalidOperationException("Nie można zmienić produktu, gdy kontener jest załadowany.");
        }
        RodzajProduktu = nowyRodzajProduktu;
        Temperatura = nowaTemperatura;
    }

    public bool CzyTemperaturaJestOdpowiednia(double wymaganaTemperatura)
    {
        return Temperatura >= wymaganaTemperatura;
    }

    public void NotifyHazard(string message)
    {
        Console.WriteLine($"Powiadomienie o zagrożeniu dla {NumerSeryjny}: {message}");
    }
}

public class Kontenerowiec
{
    public List<Kontener> Kontenery { get; private set; } = new List<Kontener>();
    public int MaksymalnaPredkosc { get; private set; }
    public int MaksymalnaLiczbaKontenerow { get; private set; }
    public double MaksymalnaWaga { get; private set; }

    public Kontenerowiec(int maksymalnaPredkosc, int maksymalnaLiczbaKontenerow, double maksymalnaWaga)
    {
        MaksymalnaPredkosc = maksymalnaPredkosc;
        MaksymalnaLiczbaKontenerow = maksymalnaLiczbaKontenerow;
        MaksymalnaWaga = maksymalnaWaga;
    }

    public void DodajKontener(Kontener kontener)
    {
        if (Kontenery.Count >= MaksymalnaLiczbaKontenerow)
        {
            throw new InvalidOperationException("Nie można dodać więcej kontenerów: osiągnięto maksymalną liczbę kontenerów.");
        }

        if (Kontenery.Sum(k => k.MasaLadunku + k.WagaWlasna) + kontener.MasaLadunku + kontener.WagaWlasna > MaksymalnaWaga * 1000)
        {
            throw new InvalidOperationException("Nie można dodać kontenera: przekroczono maksymalną wagę.");
        }

        Kontenery.Add(kontener);
    }

    public void UsunKontener(string numerSeryjny)
    {
        var kontener = Kontenery.FirstOrDefault(k => k.NumerSeryjny == numerSeryjny);
        if (kontener == null)
        {
            throw new InvalidOperationException("Kontener o podanym numerze seryjnym nie istnieje.");
        }

        Kontenery.Remove(kontener);
    }
  
    public void ZastapKontener(string numerSeryjny, Kontener nowyKontener)
    {
        UsunKontener(numerSeryjny);
        DodajKontener(nowyKontener);
    }

    public static void PrzeniesKontener(Kontenerowiec zStatku, Kontenerowiec naStatek, string numerSeryjny)
    {
        Kontener kontener = zStatku.Kontenery.FirstOrDefault(k => k.NumerSeryjny == numerSeryjny);
        if (kontener != null)
        {
            zStatku.UsunKontener(numerSeryjny);
            naStatek.DodajKontener(kontener);
        }
        else
        {
            throw new InvalidOperationException("Kontener o podanym numerze seryjnym nie został znaleziony.");
        }
    }
    
    public void WypiszInformacjeOKontenerze(string numerSeryjny)
    {
        var kontener = Kontenery.FirstOrDefault(k => k.NumerSeryjny == numerSeryjny);
        if (kontener != null)
        {
            
            Console.WriteLine(kontener);
        }
        else
        {
            Console.WriteLine("Kontener o podanym numerze seryjnym nie został znaleziony.");
        }
    }

    public void WypiszInformacje()
    {
        Console.WriteLine($"Kontenerowiec może rozwijać maksymalnie {MaksymalnaPredkosc} węzłów, przewozić maksymalnie {MaksymalnaLiczbaKontenerow} kontenerów o łącznej maksymalnej wadze {MaksymalnaWaga} ton.");
        Console.WriteLine("Aktualnie na pokładzie znajdują się kontenery:");
        foreach (var kontener in Kontenery)
        {
            Console.WriteLine($"Kontener {kontener.NumerSeryjny}: Masa ładunku - {kontener.MasaLadunku}, Wysokość - {kontener.Wysokosc}, Waga własna - {kontener.WagaWlasna}, Głębokość - {kontener.Glebokosc}, Maksymalna ładowność - {kontener.MaxLadownosc}");
        }
    }
}
class Program
{
    static void Main(string[] args)
    {
        // Tworzenie statków kontenerowych
        Kontenerowiec kontenerowiec1 = new Kontenerowiec(20, 5, 100);
        Kontenerowiec kontenerowiec2 = new Kontenerowiec(15, 10, 150);

        // Tworzenie kontenerów
        Chlodniczy chlodniczy = new Chlodniczy("KON-C-001", 10, 2, 2, 2, "Ryby", 0);
        Plyny plyny = new Plyny("KON-L-002", 15, 2, 2, 2);
        Gaz gaz = new Gaz("KON-G-003", 20, 2, 2, 2, 100);

        // Załadunek kontenerów do statku
        kontenerowiec1.DodajKontener(chlodniczy);
        kontenerowiec1.DodajKontener(plyny);
        kontenerowiec1.DodajKontener(gaz);

        // Wyświetlenie informacji o statku i jego ładunku
        kontenerowiec1.WypiszInformacje();

        // Załadunek ładunku do kontenera chłodniczego
        chlodniczy.Ladunek(5);
        // Wyświetlenie informacji o załadowanym kontenerze
        kontenerowiec1.WypiszInformacjeOKontenerze("KON-C-001");

        // Przeniesienie kontenera gazowego z jednego statku na drugi
        Kontenerowiec.PrzeniesKontener(kontenerowiec1, kontenerowiec2, "KON-G-003");

        // Usunięcie kontenera płynów ze statku
        kontenerowiec1.UsunKontener("KON-L-002");

        // Zastąpienie kontenera na statku innym kontenerem
        Chlodniczy nowyChlodniczy = new Chlodniczy("KON-C-004", 12, 2, 2, 2, "Czekolada", -18);
        kontenerowiec1.ZastapKontener("KON-C-001", nowyChlodniczy);

        // Wyświetlenie informacji o statkach po zmianach
        Console.WriteLine("\nPo zmianach:");
        Console.WriteLine("Kontenerowiec 1:");
        kontenerowiec1.WypiszInformacje();
        Console.WriteLine("\nKontenerowiec 2:");
        kontenerowiec2.WypiszInformacje();
    }
}

/*
class Program
{
    static List<Kontenerowiec> kontenerowce = new List<Kontenerowiec>();
    static List<Kontener> kontenery = new List<Kontener>();

    static void Main(string[] args)
    {
        bool running = true;
        while (running)
        {
            Console.Clear();
            WyswietlStanAplikacji();
            WyswietlMenuGlowne();

            switch (Console.ReadLine())
            {
                case "1":
                    DodajKontenerowiec();
                    break;
                case "2":
                    UsunKontenerowiec();
                    break;
                case "3":
                    DodajKontener();
                    break;
                case "4":
                    ZaladujKontenerNaStatek();
                    break;
                case "5":
                    UsunKontenerZeStatku();
                    break;
                case "q":
                case "Q":
                    running = false;
                    break;
                default:
                    Console.WriteLine("Nieznane polecenie.");
                    break;
            }
            Console.WriteLine("Naciśnij dowolny klawisz, aby kontynuować...");
            Console.ReadKey();
        }
    }

    static void WyswietlStanAplikacji()
    {
        Console.WriteLine("Lista kontenerowców:");
        if (kontenerowce.Count == 0)
        {
            Console.WriteLine("Brak");
        }
        else
        {
            foreach (var statek in kontenerowce)
            {
                Console.WriteLine($"Statek {statek} (speed={statek.MaksymalnaPredkosc}, maxContainerNum={statek.MaksymalnaLiczbaKontenerow}, maxWeight={statek.MaksymalnaWaga})");
            }
        }

        Console.WriteLine("Lista kontenerów:");
        if (kontenery.Count == 0)
        {
            Console.WriteLine("Brak");
        }
        else
        {
            foreach (var kontener in kontenery)
            {
                Console.WriteLine($"Kontener {kontener.NumerSeryjny}: Typ={kontener}, Masa ładunku={kontener.MasaLadunku}");
            }
        }
    }

    static void WyswietlMenuGlowne()
    {
        Console.WriteLine("Możliwe akcje:");
        Console.WriteLine("1. Dodaj kontenerowiec");
        Console.WriteLine("2. Usun kontenerowiec");
        Console.WriteLine("3. Dodaj kontener");
        Console.WriteLine("4. Zaladuj kontener na statek");
        Console.WriteLine("5. Usun kontener ze statku");
        Console.WriteLine("Q. Wyjście");
    }

    static void DodajKontenerowiec()
    {
        // Implementacja dodawania kontenerowca (pytanie o parametry, tworzenie instancji, dodanie do listy)
    }

    static void UsunKontenerowiec()
    {
        // Implementacja usuwania kontenerowca (wybór i usunięcie z listy)
    }

    static void DodajKontener()
    {
        // Implementacja dodawania kontenera (pytanie o parametry, tworzenie instancji odpowiedniego typu, dodanie do listy)
    }

    static void ZaladujKontenerNaStatek()
    {
        // Implementacja załadunku kontenera na statek (wybór kontenera i statku, wykonanie operacji)
    }

    static void UsunKontenerZeStatku()
    {
        // Implementacja usuwania kontenera ze statku (wybór kontenera i wykonanie operacji)
    }

    // Tutaj należy dodać pozostałe metody potrzebne do obsługi logiki biznesowej aplikacji
}
*/