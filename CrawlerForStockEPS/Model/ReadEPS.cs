    using System.Collections.Generic;
    using System.Threading;
    using System;
    using HtmlAgilityPack;
    using MySql.Data.MySqlClient;

    public class ReadEPS {
        private static MySqlConnection conn;
        private static string srcTable;
        public ReadEPS (MySqlConnection dbconn, string src) {

            conn = dbconn;
            srcTable = src;

            //定義好此次要抓取的股票編號清單, 規則是依據當下時間判斷, 若滿足截止後一個月的時間則抓取當期EPS
            //例如表定是8/14須釋出當月EPS->這邊延遲一個月, 若時間為9/14之後則檢查資料庫是否有Q2資料, 若無則抓取
            List<string> StockNoList = getEPSList ();
            int idx = 0;

            foreach (string StockNo in StockNoList) {
                idx += 1;
                Console.WriteLine ("目前處理的股票編號為:" + StockNo);
                Console.WriteLine ("目前進度:" + idx + "/" + StockNoList.Count);

                //指定網頁URL, 讀取HTML內容(財報狗的EPS頁面)
                HtmlWeb webClient = new HtmlWeb ();
                HtmlDocument page = webClient.Load ("https://histock.tw/stock/financial.aspx?no=" + StockNo + "&st=2");

                //先定義好承接的變數
                List<int> years = new List<int> ();
                List<epsl> epslist = new List<epsl> ();

                //指定要抓取的內容路徑(Xpath) /html/body/form/div[4]/div[4]/div/div[1]/div[3]/div/div[4]/div[2]/div/table
                int i = 0;
                int j = 0;
                try {
                    foreach (HtmlNode table in page.DocumentNode.SelectNodes ("/html/body/form/div[4]/div[4]/div/div[1]/div[3]/div/div[4]/div[2]/div/table")) {
                        i = 0;
                        j = 0;

                        //逐行,逐欄取出資料
                        foreach (HtmlNode row in table.SelectNodes ("tr")) {
                            foreach (HtmlNode cell in row.SelectNodes ("th|td")) {
                                if (j > 0) { //第一行為標題及年份,也可能為 desh(-) 符號,
                                    double? eps;
                                    try {
                                        //如果該格無法轉成數字代表無資料(可能為desh)
                                        //有資料就存下來(EPS資料)
                                        eps = Double.Parse (cell.InnerText);
                                    } catch (Exception e) {
                                        //給予空值(代表當下無資料)
                                        eps = null;
                                    }

                                    //宣告暫存用epsl
                                    epsl newEpsl = new epsl ();

                                    switch (i) {
                                        case 0: //年度
                                            try {
                                                int y = Int32.Parse (cell.InnerText);
                                                years.Add (y);
                                            } catch (Exception e) {
                                                years.Add (0);
                                            }
                                            break;
                                        case 1: //Q1 EPS
                                            newEpsl.epsl001 = StockNo; //股票編號
                                            newEpsl.epsl002 = years[j - 1]; //年度
                                            newEpsl.epsl003 = "Q1"; //區間
                                            newEpsl.epsl004 = eps; //EPS
                                            epslist.Add (newEpsl);
                                            break;
                                        case 2: //Q2 EPS
                                            newEpsl.epsl001 = StockNo; //股票編號
                                            newEpsl.epsl002 = years[j - 1]; //年度
                                            newEpsl.epsl003 = "Q2"; //區間
                                            newEpsl.epsl004 = eps; //EPS
                                            epslist.Add (newEpsl);
                                            break;
                                        case 3: //Q3 EPS
                                            newEpsl.epsl001 = StockNo; //股票編號
                                            newEpsl.epsl002 = years[j - 1]; //年度
                                            newEpsl.epsl003 = "Q3"; //區間
                                            newEpsl.epsl004 = eps; //EPS
                                            epslist.Add (newEpsl);
                                            break;
                                        case 4: //Q4 EPS
                                            newEpsl.epsl001 = StockNo; //股票編號
                                            newEpsl.epsl002 = years[j - 1]; //年度
                                            newEpsl.epsl003 = "Q4"; //區間
                                            newEpsl.epsl004 = eps; //EPS
                                            epslist.Add (newEpsl);
                                            break;
                                        case 5: //ALL EPS
                                            newEpsl.epsl001 = StockNo; //股票編號
                                            newEpsl.epsl002 = years[j - 1]; //年度
                                            newEpsl.epsl003 = "ALL"; //區間
                                            newEpsl.epsl004 = eps; //EPS
                                            epslist.Add (newEpsl);
                                            break;
                                    }
                                }

                                j += 1;
                            }
                            j = 0;
                            i += 1;
                        }
                    }
                    Console.WriteLine (StockNo + "共抓取:" + epslist.Count + "筆");

                    foreach (epsl newEps in epslist) {
                        InsertToEpsl (newEps);
                    }
                } catch (Exception e) {
                    Console.WriteLine (StockNo + "發生錯誤:" + e.Message);

                    //代表該號碼沒有EPS資料, 回寫資料庫並修改該筆資料的EPS紀錄(stck003=N)
                    string StrSQL = "update " + srcTable + " set stck003='N' where stck001 = ?stck001 ";
                    try {
                        MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                        myCmd.Parameters.AddWithValue ("@stck001", StockNo);

                        if (myCmd.ExecuteNonQuery () > 0) {
                            Console.WriteLine (StockNo + "已修改為無EPS紀錄!");
                        }

                    } catch (MySql.Data.MySqlClient.MySqlException ex) {
                        Console.WriteLine ("Error1 " + ex.Number + " : " + ex.Message);
                        return;
                    }
                }

                Console.WriteLine ("休息一分鐘...");
                Thread.Sleep (60000);

            }

        }

        public static void InsertToEpsl (epsl newEpsl) {

            string StrSQL;

            //事前檢查
            //1. 若年分為0代表異常資料不處理
            if (newEpsl.epsl002 == 0) {
                return;
            }

            //如果該筆為"年度總合"可能會有異動的狀態, 先清除舊有資料再進行寫入
            if (newEpsl.epsl003 == "ALL") {
                StrSQL = "delete from stock.epsl_t where epsl001 = ?epsl001 and epsl002 = ?epsl002 and epsl003 = ?epsl003 ";
                try {
                    MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                    myCmd.Parameters.AddWithValue ("@epsl001", newEpsl.epsl001);
                    myCmd.Parameters.AddWithValue ("@epsl002", newEpsl.epsl002);
                    myCmd.Parameters.AddWithValue ("@epsl003", newEpsl.epsl003);

                    if (myCmd.ExecuteNonQuery () > 0) {
                        //Console.WriteLine ("數據刪除成功！");
                    }

                } catch (MySql.Data.MySqlClient.MySqlException ex) {
                    Console.WriteLine ("Error1 " + ex.Number + " : " + ex.Message);
                    return;
                }
            }

            // 進行select (檢查該筆資料是否已經存在)
            try {

                StrSQL = "select count(1) from stock.epsl_t where epsl001 = ?epsl001 and epsl002 = ?epsl002 and epsl003 = ?epsl003 and epsl004 is not null";
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                myCmd.Parameters.AddWithValue ("@epsl001", newEpsl.epsl001);
                myCmd.Parameters.AddWithValue ("@epsl002", newEpsl.epsl002);
                myCmd.Parameters.AddWithValue ("@epsl003", newEpsl.epsl003);
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

            // 進行select (檢查該筆資料是否已經存在且EPS為空)
            try {

                StrSQL = "select count(1) from stock.epsl_t where epsl001 = ?epsl001 and epsl002 = ?epsl002 and epsl003 = ?epsl003 and epsl004 is null";
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                myCmd.Parameters.AddWithValue ("@epsl001", newEpsl.epsl001);
                myCmd.Parameters.AddWithValue ("@epsl002", newEpsl.epsl002);
                myCmd.Parameters.AddWithValue ("@epsl003", newEpsl.epsl003);
                MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader
                reader.Read ();
                String s = reader.GetString (0);
                //Console.WriteLine ("存在否:" + s);
                reader.Close ();
                if (s == "1") {
                    //若出現為空的值, 先砍掉再重建
                    StrSQL = "delete from stock.epsl_t where epsl001 = ?epsl001 and epsl002 = ?epsl002 and epsl003 = ?epsl003 ";
                    try {
                        MySqlCommand myCmd2 = new MySqlCommand (StrSQL, conn);
                        myCmd2.Parameters.AddWithValue ("@epsl001", newEpsl.epsl001);
                        myCmd2.Parameters.AddWithValue ("@epsl002", newEpsl.epsl002);
                        myCmd2.Parameters.AddWithValue ("@epsl003", newEpsl.epsl003);

                        if (myCmd2.ExecuteNonQuery () > 0) {
                            //Console.WriteLine ("數據刪除成功！");
                        }

                    } catch (MySql.Data.MySqlClient.MySqlException ex) {
                        Console.WriteLine ("Error1 " + ex.Number + " : " + ex.Message);
                        return;
                    }
                }
                //若返回值=0代表無此資料, 往下繼續進行寫入

            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error2 " + ex.Number + " : " + ex.Message);
                return;
            }

            // 進行insert 
            try {
                StrSQL = "insert into stock.epsl_t (epsl001,epsl002,epsl003,epsl004) values (?epsl001,?epsl002,?epsl003,?epsl004) ";
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                myCmd.Parameters.AddWithValue ("@epsl001", newEpsl.epsl001);
                myCmd.Parameters.AddWithValue ("@epsl002", newEpsl.epsl002);
                myCmd.Parameters.AddWithValue ("@epsl003", newEpsl.epsl003);
                myCmd.Parameters.AddWithValue ("@epsl004", newEpsl.epsl004);

                if (myCmd.ExecuteNonQuery () > 0) {
                    //Console.WriteLine ("數據新增成功！");
                }
            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error3 " + ex.Number + " : " + ex.Message);
                return;
            }

        }

        public static List<string> getEPSList () {
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

            epsl targetEPS = new epsl ();

            if (today >= Q3) {
                targetEPS.epsl002 = today.Year;
                targetEPS.epsl003 = "Q3";
            } else if (today >= Q2) {
                targetEPS.epsl002 = today.Year;
                targetEPS.epsl003 = "Q2";
            } else if (today >= Q1) {
                targetEPS.epsl002 = today.Year;
                targetEPS.epsl003 = "Q1";
            } else if (today >= Q4) {
                targetEPS.epsl002 = today.Year - 1;
                targetEPS.epsl003 = "Q4";
            }
            Console.WriteLine ("檢核目標為" + targetEPS.epsl002 + "(" + targetEPS.epsl003 + ")");

            //若不存在代表需要抓取更新

            // 進行select (取出有股利資料 但無EPS資料的清單, 區間為當年份)
            try {

                string StrSQL = "select stck001 from " + srcTable + " where stckstus='Y' and stck003='Y' " +
                    " and not exists ( select 1 from epsl_t where epsl001=stck001 and epsl002=?epsl002 and epsl003=?epsl003 and epsl004 is not null) ";
                //" and not exists ( select 1 from epsl_t where epsl001=stck001) ";
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                myCmd.Parameters.AddWithValue ("@epsl002", targetEPS.epsl002);
                myCmd.Parameters.AddWithValue ("@epsl003", targetEPS.epsl003);
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