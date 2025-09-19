using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using Your_Story.Models;

namespace Your_Story.Services
{
    public static class Database
    {
        // 1) Taban klasörü: Kullanıcının "Belgelerim" (Documents)
        private static readonly string BaseDir =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                         "Your_Story");   // Klasör adı (yoksa Ensure() içinde oluşturacağız)

        // 2) Veritabanı yolu
        private static readonly string DbPath  = Path.Combine(BaseDir, "journal.db");
        private static readonly string ConnStr = $"Data Source={DbPath}";

        /// <summary>
        /// Uygulama açılışında 1 kez çağır: klasörü oluşturur, tabloyu yoksa kurar.
        /// </summary>
        public static void Ensure()
        {
            // Klasörü garanti et
            if (!Directory.Exists(BaseDir))
                Directory.CreateDirectory(BaseDir);


            using var conn = new SqliteConnection(ConnStr);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS entries(
                    id            INTEGER PRIMARY KEY AUTOINCREMENT,
                    day           TEXT    NOT NULL,
                    time_note     TEXT    NOT NULL,
                    long_note     TEXT    NOT NULL,
                    final_comment TEXT    NOT NULL,
                    created_at    TEXT    NOT NULL
                );";
            cmd.ExecuteNonQuery();
        }

        // ---------------- CRUD (aynı kalsın) ----------------
        public static void Insert(Entry e)
        {
            using var conn = new SqliteConnection(ConnStr);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO entries(day, time_note, long_note, final_comment, created_at)
                VALUES ($d, $t, $l, $f, $c);";
            cmd.Parameters.AddWithValue("$d", e.Day.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("$t", e.TimeNote);
            cmd.Parameters.AddWithValue("$l", e.LongNote);
            cmd.Parameters.AddWithValue("$f", e.FinalComment);
            cmd.Parameters.AddWithValue("$c", e.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.ExecuteNonQuery();
        }

        public static List<Entry> GetByDate(DateTime day)
        {
            var list = new List<Entry>();
            using var conn = new SqliteConnection(ConnStr);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT id, day, time_note, long_note, final_comment, created_at
                FROM entries
                WHERE day = $d
                ORDER BY datetime(created_at) ASC;";
            cmd.Parameters.AddWithValue("$d", day.ToString("yyyy-MM-dd"));
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new Entry
                {
                    Id           = r.GetInt64(0),
                    Day          = DateTime.Parse(r.GetString(1)),
                    TimeNote     = r.GetString(2),
                    LongNote     = r.GetString(3),
                    FinalComment = r.GetString(4),
                    CreatedAt    = DateTime.Parse(r.GetString(5))
                });
            }
            return list;
        }

        public static HashSet<DateTime> GetDaysWithEntriesInMonth(DateTime month)
        {
            var start = new DateTime(month.Year, month.Month, 1);
            var end   = start.AddMonths(1);
            var set   = new HashSet<DateTime>();

            using var conn = new SqliteConnection(ConnStr);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT DISTINCT day
                FROM entries
                WHERE day >= $s AND day < $e;";
            cmd.Parameters.AddWithValue("$s", start.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("$e", end.ToString("yyyy-MM-dd"));
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                set.Add(DateTime.Parse(r.GetString(0)).Date);
            }
            return set;
        }

       
    }
}
