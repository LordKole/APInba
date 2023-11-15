
using System;
using System.IO;
using Microsoft.VisualBasic.FileIO;
using System.Data.SQLite;
using System.Text.Json;
using System.Data.SqlClient;
using System.Text;
using System.Text.Json.Nodes;
using static System.Net.WebRequestMethods;
using System.Runtime.Intrinsics.X86;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace NBAapi 
{
    public class Player
    {

        public string m_Name, m_Position;
        public int m_FTM, m_FTA, m_2PM, m_2PA, m_3PM, m_3PA, m_REB, m_BLK, m_AST, m_STL, m_TOV;
        public Player(string Name, string Position, int FTM, int FTA, int PM2, int PA2, int PM3, int PA3, int REB, int BLK, int AST, int STL, int TOV)
        {

            m_Name = Name;
            m_Position = Position;
            m_FTM = FTM;
            m_FTA = FTA;
            m_2PM = PM2;
            m_2PA = PA2;
            m_3PM = PM3;
            m_3PA = PA3;
            m_REB = REB;
            m_BLK = BLK;
            m_AST = AST;
            m_STL = STL;
            m_TOV = TOV;
        }



    }





    public class Program
    {
        

        public static List<Player> m_Players = new List<Player>();

        public static string writeToDb = @"

        INSERT INTO Player (Name, Position, FTM, FTA, PM2, PA2, PM3, PA3, REB, BLK, AST, STL, TOV)
        VALUES (@Name, @Position, @FTM, @FTA, @PM2, @PA2, @PM3, @PA3, @REB, @BLK, @AST, @STL, @TOV);
    
        ";

        public static string ConnPath = @"Data Source=:memory:";

        public static SQLiteConnection conn = new SQLiteConnection(ConnPath);

        static void ProcitajCSV(string p)
        {
            var path = p;
            bool jeUspesno = true;
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;

              
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                   
                    string[] fields = csvParser.ReadFields();

                    SQLiteCommand cm = new SQLiteCommand(writeToDb, conn);
                    if (fields != null)
                    {

                        cm.Parameters.AddWithValue("@Name", fields[0]);
                        cm.Parameters.AddWithValue("@Position", fields[1]);
                        cm.Parameters.AddWithValue("@FTM", fields[2]);
                        cm.Parameters.AddWithValue("@FTA", fields[3]);
                        cm.Parameters.AddWithValue("@PM2", fields[4]);
                        cm.Parameters.AddWithValue("@PA2", fields[5]);
                        cm.Parameters.AddWithValue("@PM3", fields[6]);
                        cm.Parameters.AddWithValue("@PA3", fields[7]);
                        cm.Parameters.AddWithValue("@REB", fields[8]);
                        cm.Parameters.AddWithValue("@BLK", fields[9]);
                        cm.Parameters.AddWithValue("@AST", fields[10]);
                        cm.Parameters.AddWithValue("@STL", fields[11]);
                        cm.Parameters.AddWithValue("@TOV", fields[12]);

                        int us = cm.ExecuteNonQuery();
                        if (us ==  0)
                        {
                            jeUspesno = false;
                        }
                      
                    }
                }
            }
            if (jeUspesno)
            {
                Console.WriteLine("Uspesno su upisani podaci u bazu podataka");
            }
            else
            {
                Console.WriteLine("Doslo je do greske pri ucitavanju podataka u bazu podataka");
            }
        }


        public static string json(string Name)
        {

            SQLiteCommand cmd = new SQLiteCommand($@"SELECT * FROM Player WHERE Name = '{Name}';", conn);

            SQLiteDataReader reader = cmd.ExecuteReader();

            if (m_Players.Count > 0)
            {
                m_Players.Clear();
            }

            while (reader.Read())
            {

                m_Players.Add(new Player(reader[0].ToString(),
                                         reader[1].ToString(),
                                         int.Parse(reader[2].ToString()),
                                         int.Parse(reader[3].ToString()),
                                         int.Parse(reader[4].ToString()),
                                         int.Parse(reader[5].ToString()),
                                         int.Parse(reader[6].ToString()),
                                         int.Parse(reader[7].ToString()),
                                         int.Parse(reader[8].ToString()),
                                         int.Parse(reader[9].ToString()),
                                         int.Parse(reader[10].ToString()),
                                         int.Parse(reader[11].ToString()),
                                         int.Parse(reader[12].ToString())
                                        ));

            }
            if (m_Players.Count == 0)
            {
                return "";
            }
            float[] freeThrows = FreeThrows();
            float[] twoP = TwoPoints();
            float[] threeP = ThreePoints();
            float[] others = OtherUsual();

            string resp = @"
                    {
                       ""playerName"":""" + m_Players[0].m_Name + @""",
                       ""gamesPlayed"":" + m_Players.Count + @",
                       ""traditional"":{
                          ""freeThrows"":{
                             ""attempts"":" + freeThrows[0].ToString(".0") + @",
                             ""made"":" + freeThrows[1].ToString(".0") + @",
                             ""shootingPercentage"":" + freeThrows[2].ToString(".0") + @"
                          },
                          ""twoPoints"":{
                             ""attempts"":" + twoP[0].ToString(".0") + @",
                             ""made"":" + twoP[1].ToString(".0") + @",
                             ""shootingPercentage"":" + twoP[2].ToString(".0") + @"
                          },
                          ""threePoints"":{
                             ""attempts"":" + threeP[0].ToString("0.0") + @",
                             ""made"":" + threeP[1].ToString(".0") + @",
                             ""shootingPercentage"":" + threeP[2].ToString(".0") + @"
                          },
                          ""points"":" + PTS().ToString(".0") + @",
                          ""rebounds"":" + others[0].ToString(".0") + @",
                          ""blocks"":" + others[1].ToString(".0") + @",
                          ""assists"":" + others[2].ToString(".0") + @",
                          ""steals"":" + others[3].ToString(".0") + @",
                          ""turnovers"":" + others[4].ToString(".0") + @"
                       },
                       ""advanced"":{
                          ""valorization"":" + Valorization().ToString(".0") + @",
                          ""effectiveFieldGoalPercentage"":" + EffectiveFieldGoalPercentage().ToString(".0") + @",
                          ""trueShootingPercentage"":" + TrueShootingPercentage().ToString(".0") + @",
                          ""hollingerAssistRatio"":" + HollingerAssistRatio().ToString(".0") + @"
                       }
                    }
                    ";


            return resp;
        }
        static float[] FreeThrows()
        {
            float[] resp = new float[3];
            int att = 0, made = 0;
            foreach (Player player in m_Players)
            {
                att += player.m_FTA;
                made += player.m_FTM;

            }
            resp[0] = (float)att / m_Players.Count;
            resp[1] = (float)made / m_Players.Count;
            resp[2] = (float)made / att * 100.0F;
            return resp;
        }

        static float[] TwoPoints()
        {
            float[] resp = new float[3];
            int att = 0, made = 0;
            foreach (Player player in m_Players)
            {
                att += player.m_2PA;
                made += player.m_2PM;

            }
            resp[0] = (float)att / m_Players.Count;
            resp[1] = (float)made / m_Players.Count;
            resp[2] = (float)made / att * 100.0F;
            return resp;
        }

        static float[] ThreePoints()
        {
            float[] resp = new float[3];
            int att = 0, made = 0;
            foreach (Player player in m_Players)
            {
                att += player.m_3PA;
                made += player.m_3PM;

            }
            resp[0] = (float)att / m_Players.Count;
            resp[1] = (float)made / m_Players.Count;
            resp[2] = (float)made / att * 100.0F;
            return resp;
        }

        static float PTS()
        {
            float resp = 0.0F;

            foreach (Player player in m_Players)
            {
                //FTM + 2*2PM + 3*3PM
                resp += 3 * player.m_3PM + 2 * player.m_2PM + player.m_FTM;
                //18 12 2 //6 6 2
                //9 6 6   //3 3 6

            }
            resp /= m_Players.Count();
            return resp;

        }

        static float[] OtherUsual()
        {
            float[] resp = new float[5];

            foreach (Player player in m_Players)
            {
                resp[0] += player.m_REB;
                resp[1] += player.m_BLK;
                resp[2] += player.m_AST;
                resp[3] += player.m_STL;
                resp[4] += player.m_TOV;
            }
            resp[0] /= m_Players.Count();

            resp[1] /= m_Players.Count();
            resp[2] /= m_Players.Count();
            resp[3] /= m_Players.Count();
            resp[4] /= m_Players.Count();
            return resp;

        }

        static float Valorization()
        {

            float resp = 0.0F;
            //(FTM + 2x2PM + 3x3PM + REB + BLK + AST + STL) - (FTA-FTM + 2PA-2PM + 3PA-3PM + TOV)
            foreach (var p in m_Players)
            {
                resp += p.m_FTM + 2 * p.m_2PM + 3 * p.m_3PM + p.m_REB + p.m_BLK + p.m_AST + p.m_STL - (p.m_FTA - p.m_FTM + p.m_2PA - p.m_2PM + p.m_3PA - p.m_3PM + p.m_TOV);
            }
            resp /= m_Players.Count();
            return resp;
        }

        static float EffectiveFieldGoalPercentage()
        {

            decimal resp = 0.0M;
            //(2PM + 3PM + 0,5 * 3PM) / (2PA + 3PA) * 100
            foreach (var p in m_Players)
            {
                resp += (p.m_2PM + p.m_3PM + 0.5M * p.m_3PM) / (p.m_2PA + p.m_3PA) * 100.0M;
            }
            resp /= m_Players.Count();
            
            return (float)resp;
        }

        static float TrueShootingPercentage()
        {

            float resp = 0.0F;
            //
            foreach (var p in m_Players)
            {
                resp += PTS() / (2 * (p.m_2PA + p.m_3PA + 0.475F * p.m_FTA)) * 100.0F;
            }
            resp /= m_Players.Count();
            Math.Round(resp, 1);
            return resp;
        }

        static float HollingerAssistRatio()
        {

            float resp = 0.0F;
            //
            foreach (var p in m_Players)
            {
                resp += p.m_AST / (p.m_2PA + p.m_3PA + 0.475F * p.m_FTA + p.m_AST + p.m_TOV) * 100.0F;
            }
            resp /= m_Players.Count();
            Math.Round(resp, 1);
            return resp;
        }

        public  Program()
        {
            conn.Open();
            DBSetup.DBSetupMethod(conn);

            Console.WriteLine("Unesite potpunu putanju do lokalne CSV datoteke");

            string path = Console.ReadLine();


            if(path.Length == 0 || path.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {
                Console.WriteLine("Uneta putanja do CSV datoteke nije validna.\nMolim vas da ponovo pokrenete program i pokusate ponovo.");
                return;
            }
            //@"C:\Users\david\Desktop\L9HomeworkChallengePlayersInput.csv";
            ProcitajCSV(path);
           



        }
    }
}

