    using System.Collections.Generic;
    using System.Threading;
    using System;
    using HtmlAgilityPack;
    using MySql.Data.MySqlClient;

    public class ReadDividend {
        private static MySqlConnection conn;
        private static string srcTable;
        public ReadDividend (MySqlConnection dbconn, string src) {

            conn = dbconn;
            srcTable = src;

            //定義好此次要抓取的股票編號清單, 規則是依據當下時間判斷, 若滿足截止後一個月的時間則抓取當期EPS
            //例如表定是8/14須釋出當月EPS->這邊延遲一個月, 若時間為9/14之後則檢查資料庫是否有Q2資料, 若無則抓取
            List<string> StockNoList = getdivdist ();
            //StockNoList = new List<string> ();
            //StockNoList.Add ("1213");

            int idx = 0;

            foreach (string StockNo in StockNoList) {
                idx += 1;
                Console.WriteLine ("目前處理的股票編號為:" + StockNo);
                Console.WriteLine ("目前進度:" + idx + "/" + StockNoList.Count);

                //指定網頁URL, 讀取HTML內容(財報狗的EPS頁面)
                HtmlWeb webClient = new HtmlWeb ();
                HtmlDocument page = webClient.Load ("https://histock.tw/stock/" + StockNo + "/除權除息");

                //先定義好承接的變數
                List<int> years = new List<int> ();
                List<divd> divdist = new List<divd> ();

                //指定要抓取的內容路徑(Xpath) /html/body/form/div[4]/div[4]/div/div[1]/div[3]/div/div[5]/div/div/table
                int i = 0;
                int j = 0;
                try {
                    foreach (HtmlNode table in page.DocumentNode.SelectNodes ("/html/body/form/div[4]/div[4]/div/div[1]/div[3]/div/div[5]/div/table")) {
                        //逐行,逐欄取出資料
                        //宣告暫存用divd

                        foreach (HtmlNode row in table.SelectNodes ("tr")) {
                            j = 0;
                            divd newdivd = new divd ();
                            newdivd.divd001 = StockNo; // 股票編號

                            if (string.IsNullOrEmpty (row.InnerText)) {
                                continue;
                            }

                            foreach (HtmlNode cell in row.SelectNodes ("th|td")) {
                                if (i >= 1) {
                                    switch (j) {
                                        case 1: //發放年度
                                            if (string.IsNullOrEmpty (cell.InnerText) || cell.InnerText == "-") {
                                                newdivd.divd002 = null;
                                            } else {
                                                newdivd.divd002 = Int32.Parse (cell.InnerText.Substring (0, 4)); //年度
                                            }
                                            break;
                                        case 2: //除權日
                                            if (string.IsNullOrEmpty (cell.InnerText) || cell.InnerText == "-") {
                                                newdivd.divd003 = null;
                                            } else {
                                                newdivd.divd003 = cell.InnerText;
                                            }

                                            break;
                                        case 3: //除息日
                                            if (string.IsNullOrEmpty (cell.InnerText) || cell.InnerText == "-") {
                                                newdivd.divd004 = null;
                                            } else {
                                                newdivd.divd004 = cell.InnerText;
                                            }
                                            break;
                                        case 4: //除權息前股價
                                            if (string.IsNullOrEmpty (cell.InnerText) || cell.InnerText == "-") {
                                                newdivd.divd005 = null;
                                            } else {
                                                newdivd.divd005 = Double.Parse (cell.InnerText.Replace (",", ""));
                                            }
                                            break;
                                        case 5: //股票股利
                                            if (string.IsNullOrEmpty (cell.InnerText) || cell.InnerText == "-") {
                                                newdivd.divd006 = null;
                                            } else {
                                                newdivd.divd006 = Double.Parse (cell.InnerText.Replace (",", ""));
                                            }
                                            break;
                                        case 6: //現金股利
                                            if (string.IsNullOrEmpty (cell.InnerText) || cell.InnerText == "-") {
                                                newdivd.divd007 = null;
                                            } else {
                                                newdivd.divd007 = Double.Parse (cell.InnerText.Replace (",", ""));
                                            }
                                            break;
                                        case 7: //EPS
                                            if (string.IsNullOrEmpty (cell.InnerText) || cell.InnerText == "-") {
                                                newdivd.divd008 = null;
                                            } else {
                                                newdivd.divd008 = Double.Parse (cell.InnerText.Replace (",", ""));
                                            }
                                            break;
                                        case 8: //配息率
                                            if (string.IsNullOrEmpty (cell.InnerText) || cell.InnerText == "-") {
                                                newdivd.divd009 = null;
                                            } else {
                                                newdivd.divd009 = Double.Parse (cell.InnerText.Replace ("%", ""));
                                            }
                                            break;
                                        case 9: //現金殖利率
                                            if (string.IsNullOrEmpty (cell.InnerText) || cell.InnerText == "-") {
                                                newdivd.divd010 = null;
                                            } else {
                                                newdivd.divd010 = Double.Parse (cell.InnerText.Replace ("%", ""));
                                            }
                                            break;
                                    }
                                }
                                j += 1;
                            }
                            if (i >= 1)
                                divdist.Add (newdivd); //第一列為說明,不寫入
                            i += 1;
                        }
                    }
                    Console.WriteLine (StockNo + "共抓取:" + divdist.Count + "筆");

                    foreach (divd newEps in divdist) {
                        InsertTodivd (newEps);
                    }
                } catch (Exception e) {
                    Console.WriteLine (StockNo + "發生錯誤:" + e.Message + " " + e.TargetSite + " " + e.InnerException);
                }

                Console.WriteLine ("休息一分鐘...");
                Thread.Sleep (60000);

            }

        }

        public static void InsertTodivd (divd newdivd) {

            string StrSQL;

            //事前檢查
            //1. 若年分為0代表異常資料不處理
            if (newdivd.divd002 == 0) {
                return;
            }

            // 進行select (檢查該筆資料是否已經存在)
            try {

                StrSQL = "select count(1) from stock.divd_t where divd001 = ?divd001 and divd002 = ?divd002";
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                myCmd.Parameters.AddWithValue ("@divd001", newdivd.divd001);
                myCmd.Parameters.AddWithValue ("@divd002", newdivd.divd002);
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
                StrSQL = "insert into stock.divd_t (divd001,divd002,divd003,divd004,divd005,divd006,divd007,divd008,divd009,divd010) " +
                    " values (?divd001,?divd002,?divd003,?divd004,?divd005,?divd006,?divd007,?divd008,?divd009,?divd010) ";
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                myCmd.Parameters.AddWithValue ("@divd001", newdivd.divd001);
                myCmd.Parameters.AddWithValue ("@divd002", newdivd.divd002);
                myCmd.Parameters.AddWithValue ("@divd003", newdivd.divd003);
                myCmd.Parameters.AddWithValue ("@divd004", newdivd.divd004);
                myCmd.Parameters.AddWithValue ("@divd005", newdivd.divd005);
                myCmd.Parameters.AddWithValue ("@divd006", newdivd.divd006);
                myCmd.Parameters.AddWithValue ("@divd007", newdivd.divd007);
                myCmd.Parameters.AddWithValue ("@divd008", newdivd.divd008);
                myCmd.Parameters.AddWithValue ("@divd009", newdivd.divd009);
                myCmd.Parameters.AddWithValue ("@divd010", newdivd.divd010);
                if (myCmd.ExecuteNonQuery () > 0) {
                    //Console.WriteLine ("數據新增成功！");
                }
            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error3 " + ex.Number + " : " + ex.Message);
                return;
            }

        }

        public static List<string> getdivdist () {
            List<string> StockList = new List<string> ();

            //若不存在代表需要抓取更新
            int targetYear = DateTime.Now.Year;

            // 進行select (取出有股利資料 但無EPS資料的清單, 區間為當年份)
            try {

                string StrSQL = "select stck001 from " + srcTable + " where stckstus='Y' and stck003='Y' " +
                    " and not exists ( select 1 from divd_t where divd001=stck001 and divd002=?divd002 ) ";
                //" and not exists ( select 1 from divd_t where divd001=stck001) ";
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                myCmd.Parameters.AddWithValue ("@divd002", targetYear);
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