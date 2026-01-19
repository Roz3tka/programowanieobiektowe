using System;
using System.Collections.Generic;

// Główna klasa programu
class Program
{
    // Lista zadań w pamięci programu
    static List<Zadanie> zadania = new List<Zadanie>();

    // Zmienna do nadawania ID
    static int nextId = 1;

    // Start programu
    static void Main()
    {
        while (true)
        {
            Console.WriteLine("\n1-Dodaj zadanie  2-Pokaz zadania  3-Usun zadanie  0-Wyjdz");
            string wybor = Console.ReadLine();

            if (wybor == "1") Dodaj();  // CREATE
            if (wybor == "2") Pokaz();  // READ
            if (wybor == "3") Usun();   // DELETE
            if (wybor == "0") break;
        }
    }

    // CREATE – dodawanie zadania
    static void Dodaj()
    {
        Zadanie z = new Zadanie();

        z.Id = nextId++;

        Console.Write("Wpisz zadanie: ");
        z.Opis = Console.ReadLine();

        zadania.Add(z);
    }

    // READ – wyświetlanie zadań (bez numeracji)
    static void Pokaz()
    {
        foreach (Zadanie z in zadania)
        {
            Console.WriteLine("- " + z.Opis);
        }
    }

    // DELETE – usuwanie zadania po treści
    static void Usun()
    {
        Console.Write("Wpisz dokladnie tresc zadania do usuniecia: ");
        string tekst = Console.ReadLine();

        for (int i = 0; i < zadania.Count; i++)
        {
            if (zadania[i].Opis == tekst)
            {
                zadania.RemoveAt(i);
                return;
            }
        }

        Console.WriteLine("Nie znaleziono zadania.");
    }
}
