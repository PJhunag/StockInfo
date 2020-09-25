    using System.Collections.Generic;
    using System.Threading;
    using System;
    using HtmlAgilityPack;
    using MySql.Data.MySqlClient;

    public class ReadProfit {
        private static MySqlConnection conn;
        private static string srcTable;
        public ReadProfit (MySqlConnection dbconn, string src) {

            conn = dbconn;
            srcTable = src;

            //定義好此次要抓取的股票編號清單, 規則是依據當下時間判斷, 若滿足截止後一個月的時間則抓取當期EPS
            //例如表定是8/14須釋出當月EPS->這邊延遲一個月, 若時間為9/14之後則檢查資料庫是否有Q2資料, 若無則抓取
            List<string> StockNoList = getprftist ();
            //StockNoList = new List<string> ();
            //StockNoList.Add ("1229");

            int idx = 0;

            foreach (string StockNo in StockNoList) {
                idx += 1;
                Console.WriteLine ("目前處理的股票編號為:" + StockNo);
                Console.WriteLine ("目前進度:" + idx + "/" + StockNoList.Count);

                //指定網頁URL, 讀取HTML內容(財報狗的EPS頁面)
                HtmlWeb webClient = new HtmlWeb ();
                HtmlDocument page = webClient.Load ("https://histock.tw/stock/" + StockNo + "/損益表");

                //先定義好承接的變數
                List<int> years = new List<int> ();
                List<prft> prftist = new List<prft> ();

                //指定要抓取的內容路徑(Xpath) /html/body/form/div[4]/div[4]/div/div[1]/div[3]/div/div[5]/div/div/table
                int i = 0;
                int j = 0;
                try {
                    foreach (HtmlNode table in page.DocumentNode.SelectNodes ("/html/body/form/div[4]/div[4]/div/div[1]/div[3]/div/div[5]/div/div/table")) {
                        //逐行,逐欄取出資料
                        //宣告暫存用prft
                        foreach (HtmlNode row in table.SelectNodes ("tr")) {
                            j = 0;
                            //Console.WriteLine (row.InnerText);
                            prft newprft = new prft ();
                            newprft.prft001 = StockNo; // 股票編號

                            foreach (HtmlNode cell in row.SelectNodes ("th|td")) {
                                if (i >= 1) {
                                    switch (j) {
                                        case 0: //年度+月份
                                            newprft.prft002 = Int32.Parse (cell.InnerText.Substring (0, 4)); //年度
                                            newprft.prft003 = cell.InnerText.Substring (4, 2); //季別
                                            break;
                                        case 1: //營收	
                                            newprft.prft004 = Int32.Parse (cell.InnerText.Replace (",", ""));
                                            break;
                                        case 2: //毛利
                                            newprft.prft005 = Int32.Parse (cell.InnerText.Replace (",", ""));
                                            break;
                                        case 3: //營業利益
                                            newprft.prft006 = Int32.Parse (cell.InnerText.Replace (",", ""));
                                            break;
                                        case 4: //稅前淨利
                                            newprft.prft007 = Int32.Parse (cell.InnerText.Replace (",", ""));
                                            break;
                                        case 5: //稅後淨利	
                                            newprft.prft008 = Int32.Parse (cell.InnerText.Replace (",", ""));
                                            break;
                                    }
                                }
                                j += 1;
                            }
                            if (i >= 1)
                                prftist.Add (newprft); //第一列為說明,不寫入
                            i += 1;
                        }
                    }
                    Console.WriteLine (StockNo + "共抓取:" + prftist.Count + "筆");

                    foreach (prft newEps in prftist) {
                        InsertToprft (newEps);
                    }
                } catch (Exception e) {
                    Console.WriteLine (StockNo + "發生錯誤:" + e.Message + " " + e.TargetSite + " " + e.InnerException);
                }

                Console.WriteLine ("休息一分鐘...");
                Thread.Sleep (60000);

            }

        }

        public static void InsertToprft (prft newprft) {

            string StrSQL;

            //事前檢查
            //1. 若年分為0代表異常資料不處理
            if (newprft.prft002 == 0) {
                return;
            }

            // 進行select (檢查該筆資料是否已經存在)
            try {

                StrSQL = "select count(1) from " + srcTable + ",prft_t where stckstus = 'Y' and stck001 = prft001 and prft001 = ?prft001 and prft002 = ?prft002 and prft003 = ?prft003";
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                myCmd.Parameters.AddWithValue ("@prft001", newprft.prft001);
                myCmd.Parameters.AddWithValue ("@prft002", newprft.prft002);
                myCmd.Parameters.AddWithValue ("@prft003", newprft.prft003);
                MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader
                reader.Read ();
                String s = reader.GetString (0);
                //Console.WriteLine ("存在否:" + s);
                reader.Close ();
                if (s == "1") {
                    //若返回值>0則代表已存在, 返回
                    return;
                }
                //若返回值=0代表無此資料, 往下繼續進行寫入

            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error2 " + ex.Number + " : " + ex.Message);
                return;
            }

            // 進行insert 
            try {
                StrSQL = "insert into stock.prft_t (prft001,prft002,prft003,prft004,prft005,prft006,prft007,prft008) " +
                    " values (?prft001,?prft002,?prft003,?prft004,?prft005,?prft006,?prft007,?prft008) ";
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                myCmd.Parameters.AddWithValue ("@prft001", newprft.prft001);
                myCmd.Parameters.AddWithValue ("@prft002", newprft.prft002);
                myCmd.Parameters.AddWithValue ("@prft003", newprft.prft003);
                myCmd.Parameters.AddWithValue ("@prft004", newprft.prft004);
                myCmd.Parameters.AddWithValue ("@prft005", newprft.prft005);
                myCmd.Parameters.AddWithValue ("@prft006", newprft.prft006);
                myCmd.Parameters.AddWithValue ("@prft007", newprft.prft007);
                myCmd.Parameters.AddWithValue ("@prft008", newprft.prft008);
                if (myCmd.ExecuteNonQuery () > 0) {
                    //Console.WriteLine ("數據新增成功！");
                }
            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error3 " + ex.Number + " : " + ex.Message);
                return;
            }

        }

        public static List<string> getprftist () {
            List<string> StockList = new List<string> ();

            //Q1 5/15
            //Q2 8/14
            //Q3 11/14
            //Q4 3/31

            //先抓出當天日期
            DateTime today = DateTime.Now;
            Console.WriteLine ("今天是" + today);

            //依據日期判斷最後一期的EPS為何時(因不會即時更新,往後推遲一個月判斷)
            DateTime Q1 = new DateTime (today.Year, 6, 1, 0, 0, 0); //5/15->6/1
            DateTime Q2 = new DateTime (today.Year, 9, 1, 0, 0, 0); //8/14->9/1
            DateTime Q3 = new DateTime (today.Year, 12, 1, 0, 0, 0); //11/14->12/1
            DateTime Q4 = new DateTime (today.Year, 4, 15, 0, 0, 0); //3/31->4/15

            prft targetEPS = new prft ();

            if (today >= Q3) {
                targetEPS.prft002 = today.Year;
                targetEPS.prft003 = "Q3";
            } else if (today >= Q2) {
                targetEPS.prft002 = today.Year;
                targetEPS.prft003 = "Q2";
            } else if (today >= Q1) {
                targetEPS.prft002 = today.Year;
                targetEPS.prft003 = "Q1";
            } else if (today >= Q4) {
                targetEPS.prft002 = today.Year - 1;
                targetEPS.prft003 = "Q4";
            }
            Console.WriteLine ("檢核目標為" + targetEPS.prft002 + "(" + targetEPS.prft003 + ")");

            //若不存在代表需要抓取更新

            // 進行select (取出有股利資料 但無EPS資料的清單, 區間為當年份)
            try {

                string StrSQL = "select stck001 from " + srcTable + " where stckstus='Y' and stck007='Y' " +
                    " and not exists ( select 1 from prft_t where prft001=stck001 and prft002=?prft002 and prft003=?prft003) ";
                //" and not exists ( select 1 from prft_t where prft001=stck001) ";
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                myCmd.Parameters.AddWithValue ("@prft002", targetEPS.prft002);
                myCmd.Parameters.AddWithValue ("@prft003", targetEPS.prft003);
                MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader
                while (reader.Read ()) {
                    String no = reader.GetString (0);
                    StockList.Add (no);
                    //Console.WriteLine(no);
                }
                reader.Close ();

            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error2 " + ex.Number + " : " + ex.Message);
            }

            return StockList;
        }
    }