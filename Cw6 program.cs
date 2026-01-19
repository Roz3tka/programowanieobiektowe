using Microsoft.Data.SqlClient;

class Osoba
{
    public int Id { get; set; }
    public string Imie { get; set; } = "";
    public string Nazwisko { get; set; } = "";
    public List<WpisOceny> ListaOcen { get; set; } = new();
}

class WpisOceny
{
    public int Id { get; set; }
    public double Ocena { get; set; }
    public string Przedmiot { get; set; } = "";
}

class App
{
    static string connStr =
        "Server=10.200.2.28;Database=studenci_72231;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";

    static void Main()
    {
        using SqlConnection con = new(connStr);
        con.Open();

        PokazStudentow(con);
        PokazStudenta(con, 1);

        DodajOsobe(con, "Adam", "Nowak");
        DodajWpis(con, 1, "Informatyka", 4.0);

        ZmienOcene(con, 1, 5.0);
        SkasujGeografie(con);

        Raport(con);
    }

    static void PokazStudentow(SqlConnection con)
    {
        string sql = "SELECT student_id, imie, nazwisko FROM Student";
        using SqlCommand cmd = new(sql, con);
        using SqlDataReader r = cmd.ExecuteReader();

        while (r.Read())
            Console.WriteLine($"{r[0]} -> {r[1]} {r[2]}");
    }

    static void PokazStudenta(SqlConnection con, int id)
    {
        string sql = "SELECT imie, nazwisko FROM Student WHERE student_id=@id";
        using SqlCommand cmd = new(sql, con);
        cmd.Parameters.AddWithValue("@id", id);

        using var r = cmd.ExecuteReader();
        if (r.Read())
            Console.WriteLine($"Student: {r[0]} {r[1]}");
    }

    static void DodajOsobe(SqlConnection con, string imie, string nazwisko)
    {
        string sql = "INSERT INTO Student(imie, nazwisko) VALUES(@i,@n)";
        using SqlCommand cmd = new(sql, con);
        cmd.Parameters.AddWithValue("@i", imie);
        cmd.Parameters.AddWithValue("@n", nazwisko);
        cmd.ExecuteNonQuery();
    }

    static bool PoprawnaOcena(double o)
        => o >= 2.0 && o <= 5.0 && o % 0.5 == 0;

    static void DodajWpis(SqlConnection con, int sid, string p, double o)
    {
        if (!PoprawnaOcena(o)) return;

        string sql = "INSERT INTO Ocena(Wartosc,Przedmiot,student_id) VALUES(@w,@p,@s)";
        using SqlCommand cmd = new(sql, con);
        cmd.Parameters.AddWithValue("@w", o);
        cmd.Parameters.AddWithValue("@p", p);
        cmd.Parameters.AddWithValue("@s", sid);
        cmd.ExecuteNonQuery();
    }

    static void ZmienOcene(SqlConnection con, int oid, double nowa)
    {
        if (!PoprawnaOcena(nowa)) return;

        string sql = "UPDATE Ocena SET Wartosc=@w WHERE ocena_id=@id";
        using SqlCommand cmd = new(sql, con);
        cmd.Parameters.AddWithValue("@w", nowa);
        cmd.Parameters.AddWithValue("@id", oid);
        cmd.ExecuteNonQuery();
    }

    static void SkasujGeografie(SqlConnection con)
    {
        using SqlCommand cmd =
            new("DELETE FROM Ocena WHERE Przedmiot='Geografia'", con);
        cmd.ExecuteNonQuery();
    }

    static void Raport(SqlConnection con)
    {
        string sql = """
        SELECT s.student_id, s.imie, s.nazwisko, o.Przedmiot, o.Wartosc
        FROM Student s
        LEFT JOIN Ocena o ON s.student_id = o.student_id
        ORDER BY s.student_id
        """;

        using SqlCommand cmd = new(sql, con);
        using SqlDataReader r = cmd.ExecuteReader();

        int lastId = -1;
        while (r.Read())
        {
            int id = (int)r["student_id"];
            if (id != lastId)
            {
                Console.WriteLine($"\n{id}: {r["imie"]} {r["nazwisko"]}");
                lastId = id;
            }
            if (r["Przedmiot"] != DBNull.Value)
                Console.WriteLine($"  {r["Przedmiot"]}: {r["Wartosc"]}");
        }
    }
}
