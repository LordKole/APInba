using System.Data.SQLite;

namespace NBAapi
{
    public class DBSetup
    {
        public static void DBSetupMethod(SQLiteConnection conn)
        {

            string kom = @"
                CREATE TABLE Player (
                Name TEXT,
                Position TEXT,
                FTM INTEGER,
                FTA INTEGER,
                PM2 INTEGER,
                PA2 INTEGER,
                PM3 INTEGER,
                PA3 INTEGER,
                REB INTEGER,
                BLK INTEGER,
                AST INTEGER,
                STL INTEGER,
                TOV INTEGER
                );
            ";

            SQLiteCommand cmd = new SQLiteCommand(kom, conn);

            cmd.ExecuteNonQuery();
        }
    }
}
