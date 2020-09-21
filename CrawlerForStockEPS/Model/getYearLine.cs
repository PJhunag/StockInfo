    using System.Collections.Generic;
    using System.Threading;
    using System;
    using HtmlAgilityPack;
    using MySql.Data.MySqlClient;

    public class getYearLine {
        private static MySqlConnection conn;
        private static string srcTable;

        List<YearLine> YearLineList;
        public getYearLine (MySqlConnection dbconn, string src) {

            conn = dbconn;
            srcTable = src;

            MySqlTransaction transaction = conn.BeginTransaction ();

            //先處理twse的資料
            YearLineList = getTwseList ();
            int idx = 1;
            foreach (YearLine newYL in YearLineList) {
                Console.WriteLine (newYL.no + ":" + newYL.dt + " 開始計算 " + idx + "/" + YearLineList.Count);
                idx += 1;

                getTwseYearLine (newYL, 0, "twse019"); //月前
                getTwseYearLine (newYL, 1, "twse020"); //一年線
                getTwseYearLine (newYL, 3, "twse021"); //三年線
                getTwseYearLine (newYL, 5, "twse022"); //五年線
            }

            //再處理tpex的資料
            YearLineList = getTpexList ();
            idx = 1;
            foreach (YearLine newYL in YearLineList) {
                Console.WriteLine (newYL.no + ":" + newYL.dt + " 開始計算 " + idx + "/" + YearLineList.Count);
                idx += 1;
                getTpexYearLine (newYL, 0, "tpex019"); //月前
                getTpexYearLine (newYL, 1, "tpex020"); //一年線
                getTpexYearLine (newYL, 3, "tpex021"); //三年線
                getTpexYearLine (newYL, 5, "tpex022"); //五年線
            }

            transaction.Commit ();

        }

        public static List<YearLine> getTwseList () {
            List<YearLine> YLList = new List<YearLine> ();

            // 先取出需要處理的清單
            try {

                string StrSQL = "select twse001,twse002 from twse_t where twse007 is not null and twse019 is null order by twse001,twse002 limit 10000";
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader
                while (reader.Read ()) {
                    YearLine newYL = new YearLine ();
                    newYL.no = reader.GetString (0);
                    newYL.dt = DateTime.Parse (reader.GetString (1));
                    YLList.Add (newYL);
                }
                reader.Close ();

            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error1 " + ex.Number + " : " + ex.Message);
            }

            Console.WriteLine ("CNT:" + YLList.Count);

            return YLList;
        }

        public static void getTwseYearLine (YearLine newYL, int yesrs, string target) {

            string avg = "";

            //先取出平均
            try {
                DateTime eTime = newYL.dt; //結束時間
                DateTime sTime = eTime;
                if (yesrs == 0) {
                    sTime = newYL.dt.AddMonths (-1); //起始時間
                } else {
                    sTime = newYL.dt.AddYears (-yesrs); //起始時間
                }

                string StrSQL = "select ROUND(avg(twse007),2) from twse_t where twse001 = ?twse001 and twse002 between ?twse002s and ?twse002e ";
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                /*                 Console.WriteLine (StrSQL);
                                Console.WriteLine (newKD.no);
                                Console.WriteLine (eTime); */
                myCmd.Parameters.AddWithValue ("@twse001", newYL.no);
                myCmd.Parameters.AddWithValue ("@twse002s", sTime);
                myCmd.Parameters.AddWithValue ("@twse002e", eTime);
                MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader

                while (reader.Read ()) {
                    avg = reader.GetString (0);
                }
                reader.Close ();

                //Console.WriteLine (sTime + " --- " + eTime + " --- " + avg);

            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error3 " + ex.Number + " : " + ex.Message);
                return;
            }

            //資料回寫
            try {
                string StrSQL = "update twse_t set " + target + "=?AVG where twse001 = ?twse001 and twse002 = ?twse002 ";
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);

                myCmd.Parameters.AddWithValue ("@AVG", avg);
                myCmd.Parameters.AddWithValue ("@twse001", newYL.no);
                myCmd.Parameters.AddWithValue ("@twse002", newYL.dt);

                if (myCmd.ExecuteNonQuery () > 0) {
                    //Console.WriteLine ("數據更新成功！");
                }

            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error6 " + ex.Number + " : " + ex.Message);
                return;
            }
        }

        public static List<YearLine> getTpexList () {
            List<YearLine> YLList = new List<YearLine> ();

            // 先取出需要處理的清單
            try {

                string StrSQL = "select tpex001,tpex002 from tpex_t where tpex007 is not null and tpex019 is null order by tpex001,tpex002 limit 10000";
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader
                while (reader.Read ()) {
                    YearLine newYL = new YearLine ();
                    newYL.no = reader.GetString (0);
                    newYL.dt = DateTime.Parse (reader.GetString (1));
                    YLList.Add (newYL);
                }
                reader.Close ();

            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error1 " + ex.Number + " : " + ex.Message);
            }

            Console.WriteLine ("CNT:" + YLList.Count);

            return YLList;
        }

        public static void getTpexYearLine (YearLine newYL, int yesrs, string target) {

            string avg = "";

            //先取出平均
            try {
                DateTime eTime = newYL.dt; //結束時間
                DateTime sTime = eTime; //起始時間
                if (yesrs == 0) {
                    sTime = newYL.dt.AddMonths (-1); //起始時間
                } else {
                    sTime = newYL.dt.AddYears (-yesrs); //起始時間
                }

                string StrSQL = "select ROUND(avg(tpex007),2) from tpex_t where tpex001 = ?tpex001 and tpex002 between ?tpex002s and ?tpex002e ";
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                /*                 Console.WriteLine (StrSQL);
                                Console.WriteLine (newKD.no);
                                Console.WriteLine (eTime); */
                myCmd.Parameters.AddWithValue ("@tpex001", newYL.no);
                myCmd.Parameters.AddWithValue ("@tpex002s", sTime);
                myCmd.Parameters.AddWithValue ("@tpex002e", eTime);
                MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader

                while (reader.Read ()) {
                    avg = reader.GetString (0);
                }
                reader.Close ();

            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error3 " + ex.Number + " : " + ex.Message);
                return;
            }

            //資料回寫
            try {
                string StrSQL = "update tpex_t set " + target + "=?AVG where tpex001 = ?tpex001 and tpex002 = ?tpex002 ";
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);

                myCmd.Parameters.AddWithValue ("@tpex001", newYL.no);
                myCmd.Parameters.AddWithValue ("@tpex002", newYL.dt);
                myCmd.Parameters.AddWithValue ("@AVG", avg);

                if (myCmd.ExecuteNonQuery () > 0) {
                    //Console.WriteLine ("數據更新成功！");
                }

            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error6 " + ex.Number + " : " + ex.Message);
                return;
            }
        }
    }