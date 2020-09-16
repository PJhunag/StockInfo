    using System.Collections.Generic;
    using System.Threading;
    using System;
    using HtmlAgilityPack;
    using MySql.Data.MySqlClient;

    public class getRSVAndKD {
        private static MySqlConnection conn;
        private static string srcTable;

        List<KD> KDList;
        public getRSVAndKD (MySqlConnection dbconn, string src) {

            conn = dbconn;
            srcTable = src;

            KDList = getList ();

            int idx = 1;
            foreach (KD newKD in KDList) {
                Console.WriteLine (newKD.no + ":" + newKD.dt + " 開始計算 " + idx + "/" + KDList.Count);
                idx += 1;

                //日KD(range=9,target=twse010,twse011,twse012)
                getKD (newKD, 9, "twse010", "twse011", "twse012");

                //週KD(range=63,target=twse013,twse014,twse015)
                //getKD (newKD, 63, "twse013", "twse014", "twse015");

                //月KD(range=270,target=twse016,twse017,twse018)
                //getKD (newKD, 270, "twse016", "twse017", "twse018");
                //return;
            }
        }

        public static List<KD> getList () {
            List<KD> KDList = new List<KD> ();

            // 先取出需要處理的清單
            try {

                string StrSQL = "select twse001,twse002 from twse_t where twse010 is null and twse001 <> '0000' order by twse001,twse002 limit 200000 ";
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader
                while (reader.Read ()) {
                    KD newKD = new KD ();
                    newKD.no = reader.GetString (0);
                    newKD.dt = DateTime.Parse (reader.GetString (1));
                    KDList.Add (newKD);
                }
                reader.Close ();

            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error1 " + ex.Number + " : " + ex.Message);
            }

            return KDList;
        }

        public static void getKD (KD newKD, int range, string RSV, string K, string D) {

            //截止日
            DateTime eTime = newKD.dt;

            //起始日
            DateTime sTime = eTime.AddDays (-range);

            //截止日收盤價
            double endPrice = 0;

            //N天內最高價
            double maxPrice = 0;

            //N天內最低價
            double minPrice = 0;

            //前一天的K(預設50)
            double preK = 50;

            //前一天的K(預設50)
            double preD = 50;

            //收盤價
            try {
                string StrSQL = "select twse007 from twse_t where twse001 = ?twse001 and twse002 = ?twse002 and twse007 is not null ";
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                /*                 Console.WriteLine ("StrSQL:" + StrSQL);
                                Console.WriteLine ("twse001:" + newKD.no);
                                Console.WriteLine ("twse002:" + newKD.dt.ToString ("yyyy/MM/dd")); */
                myCmd.Parameters.AddWithValue ("@twse001", newKD.no);
                myCmd.Parameters.AddWithValue ("@twse002", newKD.dt.ToString ("yyyy/MM/dd"));
                MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader

                while (reader.Read ()) {
                    endPrice = Double.Parse (reader.GetString (0));
                }
                reader.Close ();

            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error2 " + ex.Number + " : " + ex.Message);
                return;
            }

            //最高
            try {
                string StrSQL = "select max(p) from ( " +
                    " select twse005 p from twse_t where twse001 = ?twse001 and twse002 <= ?twse002 order by twse002 desc limit " + range + " ) tmp";
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                myCmd.Parameters.AddWithValue ("@twse001", newKD.no);
                myCmd.Parameters.AddWithValue ("@twse002", eTime);
                MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader

                while (reader.Read ()) {
                    maxPrice = Double.Parse (reader.GetString (0));
                }
                reader.Close ();

            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error3 " + ex.Number + " : " + ex.Message);
                return;
            }

            //最低
            try {
                string StrSQL = "select min(p) from ( " +
                    " select twse006 p from twse_t where twse001 = ?twse001 and twse002 <= ?twse002 order by twse002 desc limit " + range + " ) tmp";
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                myCmd.Parameters.AddWithValue ("@twse001", newKD.no);
                myCmd.Parameters.AddWithValue ("@twse002", eTime);
                MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader

                while (reader.Read ()) {
                    minPrice = Double.Parse (reader.GetString (0));
                }
                reader.Close ();

            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error4 " + ex.Number + " : " + ex.Message);
                return;
            }

            //前一天K,D
            try {
                string StrSQL = "select " + K + "," + D + " from twse_t where twse001 = ?twse001 and twse002 < ?twse002e order by twse002 desc limit 1";
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                myCmd.Parameters.AddWithValue ("@twse001", newKD.no);
                myCmd.Parameters.AddWithValue ("@twse002e", eTime);
                MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader

                if (reader.HasRows) {
                    //資料有存在
                    reader.Read ();
                    preK = Double.Parse (reader.GetString (0));
                    preD = Double.Parse (reader.GetString (1));

                } else {
                    //資料不存在
                    //不做處理, 依據預設值50處理
                }
                reader.Close ();

            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error5 " + ex.Number + " : " + ex.Message);
                return;
            }

            //取得RSV
            if (maxPrice != minPrice) {
                newKD.RSV = Math.Round ((endPrice - minPrice) / (maxPrice - minPrice) * 100, 2);
            } else {
                newKD.RSV = 50;
            }

            //取得K
            newKD.K = Math.Round (2 * preK / 3 + newKD.RSV / 3, 2);

            //取得D
            newKD.D = Math.Round (2 * preD / 3 + newKD.K / 3, 2);

            //資料回寫
            try {

                string StrSQL = "update twse_t set " + RSV + "=?RSV, " + K + "=?K, " + D + "=?D where twse001 = ?twse001 and twse002 = ?twse002 ";
/*                 Console.WriteLine (StrSQL);
                Console.WriteLine ("twse001:" + newKD.no);
                Console.WriteLine ("twse002:" + newKD.dt);
                Console.WriteLine ("newKD.RSV:" + newKD.RSV);
                Console.WriteLine ("newKD.K:" + newKD.K);
                Console.WriteLine ("newKD.D:" + newKD.D); */
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);

                myCmd.Parameters.AddWithValue ("@twse001", newKD.no);
                myCmd.Parameters.AddWithValue ("@twse002", newKD.dt);
                myCmd.Parameters.AddWithValue ("@RSV", newKD.RSV);
                myCmd.Parameters.AddWithValue ("@K", newKD.K);
                myCmd.Parameters.AddWithValue ("@D", newKD.D);

                if (myCmd.ExecuteNonQuery () > 0) {
                    //Console.WriteLine ("數據更新成功！");
                }

            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error6 " + ex.Number + " : " + ex.Message);
                return;
            }
        }
    }