    using System.Collections.Generic;
    using System.Threading;
    using System;
    using HtmlAgilityPack;
    using MySql.Data.MySqlClient;

    public class ReadOperatingIncomes {
        private static MySqlConnection conn;
        private static string srcTable;
        public ReadOperatingIncomes (MySqlConnection dbconn, string src) {

            conn = dbconn;
            srcTable = src;

            //定義好此次要抓取的股票編號清單, 規則是依據當下時間判斷, 若滿足截止後一個月的時間則抓取當期OperatingIncome
            //例如表定是8/14須釋出當月OperatingIncome->這邊延遲一個月, 若時間為9/14之後則檢查資料庫是否有Q2資料, 若無則抓取
            List<string> StockNoList = getOPMEList ();
            //StockNoList = new List<string> ();
            //StockNoList.Add ("1229");
            int idx = 0;

            foreach (string StockNo in StockNoList) {
                idx += 1;
                Console.WriteLine ("目前處理的股票編號為:" + StockNo);
                Console.WriteLine ("目前進度:" + idx + "/" + StockNoList.Count);

                //指定網頁URL, 讀取HTML內容(財報狗的OperatingIncome頁面)
                HtmlWeb webClient = new HtmlWeb ();
                HtmlDocument page = webClient.Load ("https://histock.tw/stock/" + StockNo + "/%E6%AF%8F%E6%9C%88%E7%87%9F%E6%94%B6");

                //先定義好承接的變數
                List<int> years = new List<int> ();
                List<opme> opmeist = new List<opme> ();

                //指定要抓取的內容路徑(Xpath) /html/body/form/div[4]/div[4]/div/div[1]/div[3]/div/div[4]/div[2]/div/table
                int i = 0;
                int j = 0;
                List<opme> opmeLiest = new List<opme> ();
                try {
                    foreach (HtmlNode table in page.DocumentNode.SelectNodes ("/html/body/form/div[4]/div[4]/div/div[1]/div[3]/div/div[5]/div/table")) {
                        //逐行,逐欄取出資料
                        //宣告暫存用opme
                        foreach (HtmlNode row in table.SelectNodes ("tr")) {
                            j = 0;
                            //Console.WriteLine (row.InnerText);
                            opme newOpme = new opme ();
                            newOpme.opme001 = StockNo; // 股票編號
                            foreach (HtmlNode cell in row.SelectNodes ("th|td")) {
                                if (i >= 2) {
                                    switch (j) {
                                        case 0: //年度+月份
                                            newOpme.opme002 = Int32.Parse (cell.InnerText.Substring (0, 4)); //年度
                                            newOpme.opme003 = Int32.Parse (cell.InnerText.Substring (5, 2)); //月份
                                            break;
                                        case 1: //單月營收	
                                            newOpme.opme004 = Double.Parse (cell.InnerText.Replace (",", ""));
                                            break;
                                        case 2: //去年同月營收
                                            newOpme.opme005 = Double.Parse (cell.InnerText.Replace (",", ""));
                                            break;
                                        case 3: //單月月增率
                                            newOpme.opme006 = Double.Parse (cell.InnerText.Replace ("%", ""));
                                            break;
                                        case 4: //單月年增率
                                            newOpme.opme007 = Double.Parse (cell.InnerText.Replace ("%", ""));
                                            break;
                                        case 5: //累計營收	
                                            newOpme.opme008 = Double.Parse (cell.InnerText.Replace (",", ""));
                                            break;
                                        case 6: //去年累計營收	
                                            newOpme.opme009 = Double.Parse (cell.InnerText.Replace (",", ""));
                                            break;
                                        case 7: //累積年增率
                                            newOpme.opme010 = Double.Parse (cell.InnerText.Replace ("%", ""));
                                            break;
                                    }
                                }
                                j += 1;
                            }
                            opmeLiest.Add (newOpme);
                            i += 1;
                        }
                    }

                    //將取出的資料逐筆回寫資料庫
                    foreach (opme newOpme in opmeLiest) {
                        /*                         Console.WriteLine ("--" + newOpme.opme001);
                                                Console.WriteLine ("--" + newOpme.opme002);
                                                Console.WriteLine ("--" + newOpme.opme003);
                                                Console.WriteLine ("--" + newOpme.opme004);
                                                Console.WriteLine ("--" + newOpme.opme005);
                                                Console.WriteLine ("--" + newOpme.opme006);
                                                Console.WriteLine ("--" + newOpme.opme007);
                                                Console.WriteLine ("--" + newOpme.opme008);
                                                Console.WriteLine ("--" + newOpme.opme009);
                                                Console.WriteLine ("--" + newOpme.opme010); */
                        InsertToOpme (newOpme);
                    }

                } catch (Exception e) {
                    /*                     Console.WriteLine (StockNo + "發生錯誤:" + e.Message);

                                        //代表該號碼沒有OperatingIncome資料, 回寫資料庫並修改該筆資料的OperatingIncome紀錄(stck003=N)
                                        string StrSQL = "update stock."+srcTable+" set stck003='N' where stck001 = ?stck001 ";
                                        try {
                                            MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                                            myCmd.Parameters.AddWithValue ("@stck001", StockNo);

                                            if (myCmd.ExecuteNonQuery () > 0) {
                                                Console.WriteLine (StockNo + "已修改為無OperatingIncome紀錄!");
                                            }

                                        } catch (MySql.Data.MySqlClient.MySqlException ex) {
                                            Console.WriteLine ("Error1 " + ex.Number + " : " + ex.Message);
                                            return;
                                        } */
                }

                Console.WriteLine ("休息一分鐘...");
                Thread.Sleep (60000);

            }
        }

        public static void InsertToOpme (opme newOpme) {

            string StrSQL;

            //事前檢查
            //1. 若年分為0代表異常資料不處理
            if (newOpme.opme002 == 0) {
                return;
            }

            // 進行select (檢查該筆資料是否已經存在且EPS為空)
            try {

                StrSQL = "select count(1) from " + srcTable + ",opme_t where stckstus = 'Y' and stck001 = opme001 AND opme001 = ?opme001 and opme002 = ?opme002 and opme003 = ?opme003 ";
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                myCmd.Parameters.AddWithValue ("@opme001", newOpme.opme001);
                myCmd.Parameters.AddWithValue ("@opme002", newOpme.opme002);
                myCmd.Parameters.AddWithValue ("@opme003", newOpme.opme003);
                MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader
                reader.Read ();
                String s = reader.GetString (0);
                //Console.WriteLine ("存在否:" + s);
                reader.Close ();
                if (s == "1") {
                    //已存在掠過
                    return;

                }
                //若返回值=0代表無此資料, 往下繼續進行寫入

            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error1 " + ex.Number + " : " + ex.Message);
                return;
            }

            // 進行insert 
            try {
                StrSQL = "insert into stock.opme_t (opme001,opme002,opme003,opme004,opme005,opme006,opme007,opme008,opme009,opme010) " +
                    " values (?opme001,?opme002,?opme003,?opme004,?opme005,?opme006,?opme007,?opme008,?opme009,?opme010) ";
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                myCmd.Parameters.AddWithValue ("@opme001", newOpme.opme001);
                myCmd.Parameters.AddWithValue ("@opme002", newOpme.opme002);
                myCmd.Parameters.AddWithValue ("@opme003", newOpme.opme003);
                myCmd.Parameters.AddWithValue ("@opme004", newOpme.opme004);
                myCmd.Parameters.AddWithValue ("@opme005", newOpme.opme005);
                myCmd.Parameters.AddWithValue ("@opme006", newOpme.opme006);
                myCmd.Parameters.AddWithValue ("@opme007", newOpme.opme007);
                myCmd.Parameters.AddWithValue ("@opme008", newOpme.opme008);
                myCmd.Parameters.AddWithValue ("@opme009", newOpme.opme009);
                myCmd.Parameters.AddWithValue ("@opme010", newOpme.opme010);

                if (myCmd.ExecuteNonQuery () > 0) {
                    //Console.WriteLine ("數據新增成功！");
                }
            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error2 " + ex.Number + " : " + ex.Message);
                return;
            }

        }

        public static List<string> getOPMEList () {
            List<string> StockList = new List<string> ();

            //抓取兩個月前的營收資料
            //例如今天為2020/8/18 -> 抓取2020/6營收資料

            //先抓出當天日期
            DateTime today = DateTime.Now;
            Console.WriteLine ("今天是" + today);

            //依據日期判斷最後一期的EPS為何時(因不會即時更新,往後推遲一個月判斷)
            int year = today.Year;
            int month = today.Month - 1;
            if (month <= 0) {
                month += 12;
            }

            opme targetOPME = new opme ();

            targetOPME.opme002 = year;
            targetOPME.opme003 = month;
            Console.WriteLine ("檢核目標為" + targetOPME.opme002 + "/" + targetOPME.opme003);

            //若不存在代表需要抓取更新

            // 進行select (取出有股利資料 但無EPS資料的清單, 區間為當年份)
            try {
                //避免每次都從前頭抓, 改為隨機
                string StrSQL = "select stck001 from " + srcTable + " where stckstus='Y' and stck006='Y' " +
                    " and not exists ( select 1 from opme_t where opme001=stck001 and opme002=?opme002 and opme003=?opme003) order by rand()";
                //" and not exists ( select 1 from epsl_t where epsl001=stck001) ";
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                myCmd.Parameters.AddWithValue ("@opme002", targetOPME.opme002);
                myCmd.Parameters.AddWithValue ("@opme003", targetOPME.opme003);
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