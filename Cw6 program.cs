using Microsoft.Data.SqlClient;

public class Student
{
    public int StudentId { get; set; }
    public string Imie { get; set; } = string.Empty;
    public string Nazwisko { get; set; } = string.Empty;
    public List<Ocena> Oceny { get; set; } = new List<Ocena>();
}

public class Ocena
{
    public int OcenaId { get; set; }
    public double Wartosc { get; set; }
    public string Przedmiot { get; set; } = string.Empty;
    public int StudentId { get; set; }
}

public class Program
{
    public static void Main()
    {
        string dbConnStr = "Server=10.200.2.28;Database=studenci_72231;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";

        try
        {
            using (var polaczenie = new SqlConnection(dbConnStr))
            {
                polaczenie.Open();
                Console.WriteLine("Połączenie aktywne.\n");

                Console.WriteLine("--- ZADANIE 4: Przegląd tabeli Student ---");
                WyswietlWszystkichStudentow(polaczenie);

                Console.WriteLine("\n--- ZADANIE 5: Pobieranie rekordu ID=1 ---");
                WypiszStudentaPoId(polaczenie, 1);

                Console.WriteLine("\n--- ZADANIE 7: Rejestracja nowej osoby ---");
                var s = new Student { Imie = "Jan", Nazwisko = "Kowalski" };
                DodajStudenta(polaczenie, s);

                Console.WriteLine("\n--- ZADANIE 8: Wprowadzanie ocen ---");
                DodajOcene(polaczenie, new Ocena { Wartosc = 2.5, Przedmiot = "Matematyka", StudentId = 1 });
                DodajOcene(polaczenie, new Ocena { Wartosc = 4.5, Przedmiot = "Geografia", StudentId = 1 });

                Console.WriteLine("\n--- ZADANIE 10: Korekta oceny ---");
                ZaktualizujOcene(polaczenie, 1, 5.0);

                Console.WriteLine("\n--- ZADANIE 9: Czyszczenie ocen z Geografii ---");
                UsunOcenyZGeografii(polaczenie);

                Console.WriteLine("\n--- ZADANIE 6: Raport zbiorczy ---");
                var lista = PobierzStudentowZOcenami(polaczenie);
                lista.ForEach(stu => {
                    Console.WriteLine($"Osoba: {stu.Imie} {stu.Nazwisko} [ID: {stu.StudentId}]");
                    if (stu.Oceny.Any())
                        stu.Oceny.ForEach(o => Console.WriteLine($"   > {o.Przedmiot}: {o.Wartosc}"));
                    else
                        Console.WriteLine("   > Brak danych o ocenach");
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Błąd podczas pracy z bazą: " + ex.Message);
        }
    }

    public static void WyswietlWszystkichStudentow(SqlConnection pol)
    {
        string zapytanie = "SELECT * FROM Student";
        using SqlCommand cmd = new SqlCommand(zapytanie, pol);
        using SqlDataReader dr = cmd.ExecuteReader();

        while (dr.Read())
        {
            Console.WriteLine($"{dr["student_id"]} | {dr["imie"]} {dr["nazwisko"]}");
        }
    }

    public static void WypiszStudentaPoId(SqlConnection pol, int id)
    {
        string sql = "SELECT imie, nazwisko FROM Student WHERE student_id = @idKey";
        using var cmd = new SqlCommand(sql, pol);
        cmd.Parameters.AddWithValue("@idKey", id);

        using var dr = cmd.ExecuteReader();
        if (dr.Read()) Console.WriteLine($"Rezultat: {dr["imie"]} {dr["nazwisko"]}");
    }

    public static List<Student> PobierzStudentowZOcenami(SqlConnection pol)
    {
        var wykaz = new List<Student>();
        using (var cmd = new SqlCommand("SELECT * FROM Student", pol))
        using (var dr = cmd.ExecuteReader())
        {
            while (dr.Read())
            {
                wykaz.Add(new Student
                {
                    StudentId = (int)dr["student_id"],
                    Imie = dr["imie"].ToString(),
                    Nazwisko = dr["nazwisko"].ToString()
                });
            }
        }

        foreach (var student in wykaz)
        {
            using var cmdOceny = new SqlCommand("SELECT * FROM Ocena WHERE student_id = @id", pol);
            cmdOceny.Parameters.AddWithValue("@id", student.StudentId);
            using var drOceny = cmdOceny.ExecuteReader();
            while (drOceny.Read())
            {
                student.Oceny.Add(new Ocena
                {
                    OcenaId = (int)drOceny["ocena_id"],
                    Wartosc = Convert.ToDouble(drOceny["Wartosc"]),
                    Przedmiot = drOceny["Przedmiot"].ToString(),
                    StudentId = student.StudentId
                });
            }
        }
        return wykaz;
    }

    public static void DodajStudenta(SqlConnection pol, Student s)
    {
        string insert = "INSERT INTO Student (imie, nazwisko) VALUES (@i, @n)";
        using var cmd = new SqlCommand(insert, pol);
        cmd.Parameters.AddWithValue("@i", s.Imie);
        cmd.Parameters.AddWithValue("@n", s.Nazwisko);

        int wyn = cmd.ExecuteNonQuery();
        Console.WriteLine($"Dodano pomyślnie. Rekordy: {wyn}");
    }

    private static bool WalidujOcene(double stopien)
    {
        if (stopien < 2.0 || stopien > 5.0 || stopien == 2.5) return false;
        return (stopien * 10) % 5 == 0;
    }

    public static void DodajOcene(SqlConnection pol, Ocena o)
    {
        if (!WalidujOcene(o.Wartosc))
        {
            Console.WriteLine($"Nieprawidłowa wartość: {o.Wartosc}");
            return;
        }

        string sql = "INSERT INTO Ocena (Wartosc, Przedmiot, student_id) VALUES (@v, @p, @s)";
        using var cmd = new SqlCommand(sql, pol);
        cmd.Parameters.AddWithValue("@v", o.Wartosc);
        cmd.Parameters.AddWithValue("@p", o.Przedmiot);
        cmd.Parameters.AddWithValue("@s", o.StudentId);

        cmd.ExecuteNonQuery();
        Console.WriteLine($"Ocena {o.Wartosc} zapisana.");
    }

    public static void UsunOcenyZGeografii(SqlConnection pol)
    {
        using var cmd = new SqlCommand("DELETE FROM Ocena WHERE Przedmiot LIKE 'Geografia'", pol);
        int ile = cmd.ExecuteNonQuery();
        Console.WriteLine($"Usunięte wiersze: {ile}");
    }

    public static void ZaktualizujOcene(SqlConnection pol, int idOceny, double nowaSkala)
    {
        if (!WalidujOcene(nowaSkala)) return;

        string update = "UPDATE Ocena SET Wartosc = @val WHERE ocena_id = @id";
        using var cmd = new SqlCommand(update, pol);
        cmd.Parameters.AddWithValue("@val", nowaSkala);
        cmd.Parameters.AddWithValue("@id", idOceny);

        if (cmd.ExecuteNonQuery() > 0) Console.WriteLine("Aktualizacja zakończona.");
    }
}
