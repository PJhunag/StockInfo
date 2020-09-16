    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Threading;
    using System;
    using HtmlAgilityPack;
    using MySql.Data.MySqlClient;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json;

    public class ReadTwseStockPrice {
        private static MySqlConnection conn;
        private static string srcTable;
        public ReadTwseStockPrice (MySqlConnection dbconn, string src) {

            conn = dbconn;
            srcTable = src;

            //定義好此次要抓取的日期清單, 取出TWSE表中最新的資料到當下日期-1為止
            //例如TWSE資料室到2020/01/05, 今日為2020/08/21, 取出的資料範圍就是2020/01/06 - 2020/08/20
            List<string> DateList = getTWSEList ();
            //DateList = new List<string> ();
            //DateList.Add ("20150105");
            //DateList.Add ("20150106");

            int idx = 0;

            foreach (string Date in DateList) {
                idx += 1;
                Console.WriteLine ("目前處理的日期為:" + Date);
                Console.WriteLine ("目前進度:" + idx + "/" + DateList.Count);

                //指定網頁URL, 讀取HTML內容(財報狗的OperatingIncome頁面)
                //HtmlWeb webClient = new HtmlWeb ();
                //HtmlDocument page = webClient.Load ("https://www.twse.com.tw/exchangeReport/MI_INDEX?response=json&date=" + Date + "&type=ALL");

                //定義URL
                string URL = "https://www.twse.com.tw/exchangeReport/MI_INDEX?response=json&date=" + Date + "&type=ALL";
                Console.WriteLine ("URL:" + URL);
                Console.WriteLine ("Now:" + DateTime.Now);

                //呼叫並抓取資料
                WebRequest request = WebRequest.Create (URL);
                WebResponse response = request.GetResponse ();

                //將JSON讀出來
                Stream dataStream = response.GetResponseStream ();
                StreamReader reader = new StreamReader (dataStream);
                string responseFromServer = reader.ReadToEnd ();

                //先定義好承接的變數
                List<twse> TWSEist = new List<twse> ();

                //將JSON字串轉回物件
                dynamic json = JValue.Parse (responseFromServer);
                //Console.WriteLine (json["data9"].Count);
                //Console.WriteLine ((string) json["data9"][0][9]);

                MySqlTransaction transaction = conn.BeginTransaction ();
                try {

                    //將取出的資料逐筆回寫資料庫
                    for (int i = 0; i < json["data9"].Count; i++) {
                        twse newTWSE = new twse ();
                        // 台灣證券交易所
                        // twse001 代碼(0)
                        // twse002 日期
                        // twse003 成交股數(2)
                        // twse004 開盤價(5)
                        // twse005 最高價(6)
                        // twse006 最低價(7)
                        // twse007 收盤價(8)
                        // twse008 價差(10)
                        // twse009 本益比(15)
                        newTWSE.twse001 = (string) json["data9"][i][0];
                        newTWSE.twse002 = DateTime.ParseExact (Date, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                        newTWSE.twse003 = (string) json["data9"][i][2];
                        newTWSE.twse003 = newTWSE.twse003.Replace (",", "");
                        newTWSE.twse004 = (string) json["data9"][i][5];
                        newTWSE.twse005 = (string) json["data9"][i][6];
                        newTWSE.twse006 = (string) json["data9"][i][7];
                        newTWSE.twse007 = (string) json["data9"][i][8];

                        if ((string) json["data9"][i][9] == "<p style= color:red>+</p>") {
                            //正的
                            newTWSE.twse008 = (string) json["data9"][i][10];
                        } else {
                            //負的
                            newTWSE.twse008 = "-" + (string) json["data9"][i][10];
                        }
                        newTWSE.twse009 = (string) json["data9"][i][15];
                        //Console.WriteLine ("twse001:" + newTWSE.twse001);
                        //Console.WriteLine ("twse002:" + newTWSE.twse002);
                        //Console.WriteLine ("twse003:" + newTWSE.twse003);
                        //Console.WriteLine ("twse004:" + newTWSE.twse004);
                        //Console.WriteLine ("twse005:" + newTWSE.twse005);
                        //Console.WriteLine ("twse006:" + newTWSE.twse006);
                        //Console.WriteLine ("twse007:" + newTWSE.twse007);
                        //Console.WriteLine ("twse008:" + newTWSE.twse008);
                        //Console.WriteLine ("twse009:" + newTWSE.twse009);

                        InsertToTWSE (newTWSE);
                    }

                    transaction.Commit ();

                } catch (Exception e) {
                    Console.WriteLine ("ERROR:" + e.Message);
                    transaction.Rollback ();
                }

                Console.WriteLine ("休息30秒鐘...");
                Thread.Sleep (30000);

            }

            //清除舊有資料
            //1. 超過一年以上
            //2. 僅保留每周末價格(周五)
            //DeleteFromTWSE ();
        }

        public static void InsertToTWSE (twse newTWSE) {

            string StrSQL;

            //檢查是否為所需資料(存在於主表"+srcTable+"內)
            try {
                StrSQL = "select count(1) from stock." + srcTable + " where stck001 = ?stck001";
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                myCmd.Parameters.AddWithValue ("@stck001", newTWSE.twse001);
                MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader
                reader.Read ();
                String s = reader.GetString (0);
                //Console.WriteLine ("存在否:" + s);
                reader.Close ();
                if (s == "0") {
                    //不存在, 該筆不處理
                    return;

                }
            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error1 " + ex.Number + " : " + ex.Message);
                return;
            }

            // 進行select (檢查該筆資料是否已經存在)
            try {
                StrSQL = "select count(1) from stock.TWSE_t where twse001 = ?twse001 and twse002 = ?twse002 ";
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                myCmd.Parameters.AddWithValue ("@twse001", newTWSE.twse001);
                myCmd.Parameters.AddWithValue ("@twse002", newTWSE.twse002);
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
                StrSQL = "insert into stock.TWSE_t (twse001,twse002,twse003,twse004,twse005,twse006,twse007,twse008,twse009) " +
                    " values (?twse001,?twse002,?twse003,?twse004,?twse005,?twse006,?twse007,?twse008,?twse009) ";
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);

                myCmd.Parameters.AddWithValue ("@twse001", newTWSE.twse001);
                myCmd.Parameters.AddWithValue ("@twse002", newTWSE.twse002);
                myCmd.Parameters.AddWithValue ("@twse003", numTrans (newTWSE.twse003));
                myCmd.Parameters.AddWithValue ("@twse004", numTrans (newTWSE.twse004));
                myCmd.Parameters.AddWithValue ("@twse005", numTrans (newTWSE.twse005));
                myCmd.Parameters.AddWithValue ("@twse006", numTrans (newTWSE.twse006));
                myCmd.Parameters.AddWithValue ("@twse007", numTrans (newTWSE.twse007));
                myCmd.Parameters.AddWithValue ("@twse008", numTrans (newTWSE.twse008));
                myCmd.Parameters.AddWithValue ("@twse009", numTrans (newTWSE.twse009));

                if (myCmd.ExecuteNonQuery () > 0) {
                    //Console.WriteLine ("數據新增成功！");
                }
            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error2 " + ex.Number + " : " + ex.Message);
                Console.WriteLine ("@twse001:" + newTWSE.twse001);
                Console.WriteLine ("@twse002:" + newTWSE.twse002);
                Console.WriteLine ("@twse003:" + newTWSE.twse003);
                Console.WriteLine ("@twse004:" + newTWSE.twse004);
                Console.WriteLine ("@twse005:" + newTWSE.twse005);
                Console.WriteLine ("@twse006:" + newTWSE.twse006);
                Console.WriteLine ("@twse007:" + newTWSE.twse007);
                Console.WriteLine ("@twse008:" + newTWSE.twse008);
                Console.WriteLine ("@twse009:" + newTWSE.twse009);
                return;
            }

        }

        public static void DeleteFromTWSE () {

            string StrSQL;
            DateTime targetDt = DateTime.Now.AddYears (-1);
            Console.WriteLine ("開始清除 " + targetDt + " 以前的舊有資料...");
            //清除超過一年且非周末的紀錄資料
            try {
                StrSQL = "delete from stock.twse_t where twse002 <= ?twse002 and DAYOFWEEK(twse002) <= 5";
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                myCmd.Parameters.AddWithValue ("@twse002", targetDt);
                if (myCmd.ExecuteNonQuery () > 0) {
                    Console.WriteLine ("資料清除完成！");
                }

            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error1 " + ex.Number + " : " + ex.Message);
                return;
            }

        }

        public static List<string> getTWSEList () {
            List<string> DateList = new List<string> ();

            //抓取Tpex_t最新資料到到當天-1的日期清單
            DateTime sTime = DateTime.Now.AddDays (-1);
            DateTime eTime;

            if (DateTime.Now.Hour > 15) {
                eTime = DateTime.Now; //如果在下午三點前, 預設資料未出, 只抓到前一天
            } else {
                eTime = DateTime.Now.AddDays(-1); //如果在下午三點後, 預設資料已出, 抓到當天為止
            }

            // 進行select (取出有股利資料 但無EPS資料的清單, 區間為當年份)
            try {

                string StrSQL = "select max(twse002) from twse_t where twse001 = '0050'";
                //" and not exists ( select 1 from epsl_t where epsl001=stck001) ";
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader
                while (reader.Read ()) {
                    sTime = DateTime.Parse (reader.GetString (0)).AddDays(1);
                    //Console.WriteLine(no);
                }
                reader.Close ();

            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error2 " + ex.Number + " : " + ex.Message);
            }

            Console.WriteLine ("資料抓取範圍為" + sTime + " 至 " + eTime);

            string day;
            string holiday;
            while (eTime >= sTime) {
                day = sTime.Year.ToString ("0000") + sTime.Month.ToString ("00") + sTime.AddDays(-1).Day.ToString ("00");
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
                if ((str.Contains ("-") || str.Contains (","))) {
                    try {
                        //若數字中出現,則剔除
                        str = str.Replace (",", "");

                        //若數字中出現--則替換為null
                        if (str == "--") {
                            str = null;
                        }
                    } catch (Exception e) {
                        Console.WriteLine ("str:(" + str + ")");
                        Console.WriteLine ("Message(" + e.StackTrace + ")");
                    }
                }
            }
            return str;
        }

    }