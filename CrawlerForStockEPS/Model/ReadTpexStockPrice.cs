    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Threading;
    using System;
    using Dapper;
    using MySql.Data.MySqlClient;

    public class ReadTpexStockPrice {
        private static MySqlConnection conn;
        private static string srcTable;
        public ReadTpexStockPrice (MySqlConnection dbconn, string src) {

            conn = dbconn;
            srcTable = src;

            //定義好此次要抓取的日期清單, 取出Tpex表中最新的資料到當下日期-1為止
            //例如Tpex資料室到2020/01/05, 今日為2020/08/21, 取出的資料範圍就是2020/01/06 - 2020/08/20
            List<string> DateList = getTpexList ();
            //DateList = new List<string> ();
            //DateList.Add ("109/01/02");

            int idx = 0;

            foreach (string Date in DateList) {
                idx += 1;
                Console.WriteLine ("目前處理的日期為:" + Date);
                Console.WriteLine ("目前進度:" + idx + "/" + DateList.Count);

                //指定網頁URL, 讀取HTML內容(財報狗的OperatingIncome頁面)
                //HtmlWeb webClient = new HtmlWeb ();
                //HtmlDocument page = webClient.Load ("https://www.tpex.com.tw/exchangeReport/MI_INDEX?response=json&date=" + Date + "&type=ALL");

                //定義URL
                string URL = "http://www.tpex.org.tw/web/stock/aftertrading/daily_close_quotes/stk_quote_download.php?l=zh-tw&d=" + Date + "&s=0";
                Console.WriteLine ("URL:" + URL);
                Console.WriteLine ("Now:" + DateTime.Now);

                //呼叫並抓取資料
                WebRequest request = WebRequest.Create (URL);
                WebResponse response = request.GetResponse ();

                //將JSON讀出來
                Stream dataStream = response.GetResponseStream ();
                StreamReader reader = new StreamReader (dataStream);
                string responseFromServer = reader.ReadToEnd ();

                string[] stockList = responseFromServer.Split ("\n");

                string[] stockInfo;

                //先定義好承接的變數
                List<tpex> Tpexist = new List<tpex> ();

                int memoIdx = 0; //前三行為備註資料不需處理

                MySqlTransaction transaction = conn.BeginTransaction ();
                try {
                    //將取出的資料逐筆回寫資料庫
                    foreach (string stock in stockList) {

                        stockInfo = stock.Split (",");
                        tpex newTpex = new tpex ();

                        //前三行不須處理或是已無資料
                        if (memoIdx <= 2 || stockInfo.Length < 8) {
                            memoIdx += 1;
                            continue;
                        }

                        //tpex001//股票編號 [0]
                        //tpex002//日期 - date
                        //tpex003//成交股數 [8]
                        //tpex004//開盤價 [4]
                        //tpex005//最高價 [5]
                        //tpex006//最低價 [6]
                        //tpex007//收盤價 [2]

                        newTpex.tpex001 = stockInfo[0].Replace ("\"", "");
                        int year = Int32.Parse (Date.Substring (0, 3)) + 1911;
                        string Date2 = year.ToString () + Date.Substring (3, 6);
                        newTpex.tpex002 = DateTime.ParseExact (Date2, "yyyy/MM/dd", System.Globalization.CultureInfo.CurrentCulture);
                        newTpex.tpex003 = stockInfo[8].Replace ("\"", "");
                        newTpex.tpex004 = stockInfo[4].Replace ("\"", "");
                        newTpex.tpex005 = stockInfo[5].Replace ("\"", "");
                        newTpex.tpex006 = stockInfo[6].Replace ("\"", "");
                        newTpex.tpex007 = stockInfo[2].Replace ("\"", "");

                        /*                                             Console.WriteLine ("Tpex001:" + newTpex.tpex001);
                                                                    Console.WriteLine ("Tpex002:" + newTpex.tpex002);
                                                                    Console.WriteLine ("Tpex003:" + newTpex.tpex003);
                                                                    Console.WriteLine ("Tpex004:" + newTpex.tpex004);
                                                                    Console.WriteLine ("Tpex005:" + newTpex.tpex005);
                                                                    Console.WriteLine ("Tpex006:" + newTpex.tpex006);
                                                                    Console.WriteLine ("Tpex007:" + newTpex.tpex007);
                                                                    Console.WriteLine ("Tpex008:" + newTpex.tpex008);
                                                                    Console.WriteLine ("Tpex009:" + newTpex.tpex009);  */

                        InsertToTpex (newTpex);
                    }

                    transaction.Commit ();
                } catch (Exception e) {
                    Console.WriteLine ("ERROR:" + e.Message);
                    transaction.Rollback ();
                }

                Console.WriteLine ("休息20秒鐘...");
                Thread.Sleep (20000);

            }

            //清除舊有資料
            //1. 超過一年以上
            //2. 僅保留每周末價格(周五)
            //DeleteFromTpex ();
        }

        public static void InsertToTpex (tpex newTpex) {

            string StrSQL;

            //檢查是否為所需資料(存在於主表"+srcTable+"內)
            try {
                //定義確認SQL, 確認是否存在於股票主表內(stck_t)
                StrSQL = "select count(1) from stock." + srcTable + " where stck001 = @stck001";

                //指定目標股票編號
                int cnt = conn.ExecuteScalar<int> (StrSQL, new { stck001 = newTpex.tpex001 });

                if (cnt == 0) {
                    //不存在, 該筆不處理
                    return;

                }
            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error1 " + ex.Number + " : " + ex.Message);
                return;
            }

            // 進行select (檢查該筆資料是否已經存在)
            try {

                //定義確認SQL, 確認資料是否已存在
                StrSQL = "select count(1) from stock.tpex_t where Tpex001 = @Tpex001 and Tpex002 = @Tpex002 ";

                //指定目標股票編號
                int cnt = conn.ExecuteScalar<int> (StrSQL, new { Tpex001 = newTpex.tpex001, Tpex002 = newTpex.tpex002 });

                if (cnt == 1) {
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

                //先針對資料準備
                newTpex.tpex003 = numTrans (newTpex.tpex003);
                newTpex.tpex004 = numTrans (newTpex.tpex004);
                newTpex.tpex005 = numTrans (newTpex.tpex005);
                newTpex.tpex006 = numTrans (newTpex.tpex006);
                newTpex.tpex007 = numTrans (newTpex.tpex007);
                //沒有收盤價代表資料異常, 返回不處理
                if(newTpex.tpex007 is null)
                {
                    return;
                }
                newTpex.tpex009 = numTrans (newTpex.tpex009);

                //計算差價
                if (!string.IsNullOrEmpty (newTpex.tpex007) && !string.IsNullOrEmpty (newTpex.tpex004)) {
                    newTpex.tpex008 = (Double.Parse (newTpex.tpex007) - Double.Parse (newTpex.tpex004)).ToString ();
                }

                //準備寫入資料SQL
                StrSQL = "insert into stock.tpex_t (Tpex001,Tpex002,Tpex003,Tpex004,Tpex005,Tpex006,Tpex007,Tpex008,Tpex009) " +
                    " values (@Tpex001,@Tpex002,@Tpex003,@Tpex004,@Tpex005,@Tpex006,@Tpex007,@Tpex008,@Tpex009) ";
                conn.Execute (StrSQL, newTpex);
            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("@tpex001:" + newTpex.tpex001);
                Console.WriteLine ("@tpex002:" + newTpex.tpex002);
                Console.WriteLine ("@tpex003:" + newTpex.tpex003);
                Console.WriteLine ("@tpex004:" + newTpex.tpex004);
                Console.WriteLine ("@tpex005:" + newTpex.tpex005);
                Console.WriteLine ("@tpex006:" + newTpex.tpex006);
                Console.WriteLine ("@tpex007:" + newTpex.tpex007);
                Console.WriteLine ("@tpex008:" + newTpex.tpex008);
                Console.WriteLine ("@tpex009:" + newTpex.tpex009);
                Console.WriteLine ("Error2 " + ex.Number + " : " + ex.Message);
                return;
            }

        }

        public static void DeleteFromTpex () {

            string StrSQL;
            DateTime targetDt = DateTime.Now.AddYears (-1);
            Console.WriteLine ("開始清除 " + targetDt + " 以前的舊有資料...");
            //清除超過一年且非周末的紀錄資料
            try {
                StrSQL = "delete from stock.tpex_t where Tpex002 <= ?Tpex002 and DAYOFWEEK(Tpex002) <= 5";
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                myCmd.Parameters.AddWithValue ("@Tpex002", targetDt);
                if (myCmd.ExecuteNonQuery () > 0) {
                    Console.WriteLine ("資料清除完成！");
                }

            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error1 " + ex.Number + " : " + ex.Message);
                return;
            }

        }

        public static List<string> getTpexList () {
            List<string> DateList = new List<string> ();

            //抓取Tpex_t最新資料到到當天-1的日期清單
            DateTime sTime = DateTime.Now.AddDays (-1);
            DateTime eTime;

            if (DateTime.Now.Hour >= 15) {
                Console.WriteLine ("抓今天");
                eTime = DateTime.Now; //如果在下午三點前, 預設資料未出, 只抓到前一天
            } else {
                Console.WriteLine ("抓昨天");
                eTime = DateTime.Now.AddDays (-1); //如果在下午三點後, 預設資料已出, 抓到當天為止
            }

            // 進行select (取出有股利資料 但無EPS資料的清單, 區間為當年份)
            try {
                //定義確認SQL, 取出最後更新日
                string StrSQL = "select max(Tpex002) from Tpex_t where tpex001 = '1240'";

                //從最後更新日+1開始抓取
                string dt = conn.ExecuteScalar<string> (StrSQL);
                sTime = DateTime.Parse (dt).AddDays (1);

            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error2 " + ex.Number + " : " + ex.Message);
            }

            Console.WriteLine ("資料抓取範圍為" + sTime + " 至 " + eTime);

            string day;
            string holiday;
            while (eTime >= sTime) {
                //日期格式 107(民國)/01/01
                day = sTime.Year - 1911 + "/" + sTime.Month.ToString ("00") + "/" + sTime.Day.ToString ("00");
                holiday = "N"; //預設為非假日
                switch (sTime.DayOfWeek.ToString ()) {
                    case "Saturday":
                        holiday = "Y"; //週六
                        break;
                    case "Sunday":
                        holiday = "Y"; //週日
                        break;
                }

                //判定為非假日才抓取
                if (holiday == "N") {
                    DateList.Add (day);
                    //Console.WriteLine(day);
                }

                //抓取下一個日期
                sTime = sTime.AddDays (1);
            }

            return DateList;
        }

        public static string numTrans (string str) {

            //若為空則返回Null
            if (string.IsNullOrEmpty (str)) {
                str = null;
            } else {
                try {
                    //若數字中出現,則剔除
                    str = str.Replace (",", "");

                    //若數字中出現--則替換為null
                    if (str.IndexOf ("-", 0) >= 0) {
                        str = null;
                    }
                } catch (Exception e) {
                    Console.WriteLine ("str:(" + str + ")");
                    Console.WriteLine ("Message(" + e.StackTrace + ")");
                }
            }
            return str;
        }

    }