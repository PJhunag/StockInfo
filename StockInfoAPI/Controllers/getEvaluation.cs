using System;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace StockInfoAPI.Controllers {
    [ApiController]
    [Route ("[controller]/{id}")]
    public class getEvaluation : ControllerBase {

        //先定義好相關變數
        Evaluation ev = new Evaluation ();
        MySqlConnection conn;
        string StrSQL;

        [HttpGet]
        public Evaluation Get (string id) {

            ControllerContext.HttpContext.Response.Headers.Add ("Access-Control-Allow-Origin", "*");
            ControllerContext.HttpContext.Response.Headers.Add ("Access-Control-Allow-Headers", "Content-Type");
            ControllerContext.HttpContext.Response.Headers.Add ("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");

            ev.stockNo = id;
            database db = new database ();
            conn = db.dbConect ();

            int year = DateTime.Now.Year; //當年年度

            //取得股票名稱
            getYearName ();

            //取得當年營收AND淨利
            getYearAccumulationOperatingIncomeAndYearAccumulationProfit (year, "this");

            //取得去年營收AND淨利
            getYearAccumulationOperatingIncomeAndYearAccumulationProfit (year - 1, "last");

            //取得去年EPS(發放年度為今年)
            getLastYearEPSAndEPSUseRatioAndStockIncreaseRatio (year);

            //取得營收成長率(本年最新)
            getRevenueIncreaseRatio (year);

            //抓取平均EPS(五年) & 平均殖利率(五年)
            getFiveYearsAvgerageEPSandDividend (year);

            //抓取時價/更新日
            getPriceAndUpdateDate ();

            //抓取近四季EPS
            getLastFourEPS ();

            //抓取營收最後更新時間
            getOperatingUpdateTime ();

            //抓取五年股票均價
            getFiveYearsAvgeragePrice ();

            //抓取股票類別
            getType ();

            //(當年淨利 / 當年累營收) / (去年淨利 / 去年營收) * 去年EPS * 累積年增率/ 股數增加率 = 當年度[預測]EPS
            if (!string.IsNullOrEmpty (ev.LastEPS)) {
                double ThisYearAccumulationProfit = Double.Parse (ev.ThisYearAccumulationProfit);
                double ThisYearAccumulationOperatingIncome = Double.Parse (ev.ThisYearAccumulationOperatingIncome);
                double LastYearAccumulationProfit = Double.Parse (ev.LastYearAccumulationProfit);
                double LastYearAccumulationOperatingIncome = Double.Parse (ev.LastYearAccumulationOperatingIncome);
                double LastEPS = Double.Parse (ev.LastEPS);
                double RevenueIncreaseRatio = Double.Parse (ev.RevenueIncreaseRatio);
                double StockIncreaseRatio = Double.Parse (ev.StockIncreaseRatio);

                double preEPS = Math.Round ((ThisYearAccumulationProfit / ThisYearAccumulationOperatingIncome) / (LastYearAccumulationProfit / LastYearAccumulationOperatingIncome) *
                    LastEPS * RevenueIncreaseRatio / StockIncreaseRatio, 2);
                ev.preEPS = preEPS.ToString ();
                //當年度[預測]EPS*股利發放率/7% = 合理價位

                ev.preDividend = Math.Round (Double.Parse (ev.preEPS) * Double.Parse (ev.EPSUseRatio), 2).ToString ();

                ev.goodPrice = Math.Round (Double.Parse (ev.preDividend) / 0.07, 2).ToString ();

                //預測本益比
                ev.preEPSRate = Math.Round (Double.Parse (ev.Price) / Double.Parse (ev.preEPS), 2).ToString ();

                //預測殖利率
                ev.preDividendRadio = Math.Round (Double.Parse (ev.preDividend) / Double.Parse (ev.Price) * 100, 2).ToString ();
            } else {
                ev.preEPS = "";
            }

            return ev;
        }

        public void getYearName () {
            try {

                StrSQL = "select name003 from name_t where name001 = ?name001 and name002 = ?name002 ";
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                myCmd.Parameters.AddWithValue ("@name001", ev.stockNo);
                myCmd.Parameters.AddWithValue ("@name002", "zh_TW");
                MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader
                while (reader.Read ()) {

                    ev.stockName = reader.GetString (0); //名稱-name003

                }
                reader.Close ();

            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error2 " + ex.Number + " : " + ex.Message);
            }
        }

        //取得指定年份營收AND淨利
        public void getYearAccumulationOperatingIncomeAndYearAccumulationProfit (int year, string type) {
            try {

                StrSQL = "select IFNULL(sum(prft004),0),IFNULL(sum(prft008),0) from prft_t where prft001 = ?prft001 and prft002 = ?prft002 ";
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                myCmd.Parameters.AddWithValue ("@prft001", ev.stockNo);
                myCmd.Parameters.AddWithValue ("@prft002", year);
                MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader
                if (reader.HasRows) {
                    while (reader.Read ()) {
                        if (type == "this") {
                            ev.ThisYearAccumulationOperatingIncome = reader.GetString (0); //營收-prft004
                            ev.ThisYearAccumulationProfit = reader.GetString (1); //純利-prft008
                        } else {
                            ev.LastYearAccumulationOperatingIncome = reader.GetString (0); //營收-prft004
                            ev.LastYearAccumulationProfit = reader.GetString (1); //純利-prft008
                        }
                    }
                }
                reader.Close ();

            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error2 " + ex.Number + " : " + ex.Message);
            }
        }

        public void getLastYearEPSAndEPSUseRatioAndStockIncreaseRatio (int year) {
            try {
                StrSQL = "select divd008,divd009/100,(divd006/10)+1,divd006,divd007 from divd_t where divd001 = ?divd001 and divd002 = ?divd002 ";
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                myCmd.Parameters.AddWithValue ("@divd001", ev.stockNo);
                myCmd.Parameters.AddWithValue ("@divd002", year);
                MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader
                while (reader.Read ()) {

                    ev.LastEPS = reader.GetString (0); //EPS-divd008
                    //EPSUseRatio = reader.GetString (1)); //發放率-divd009
                    ev.StockIncreaseRatio = reader.GetString (2); //股息-divd006
                    ev.LastStockDividend = reader.GetString (3); //股息(股票)-divd006
                    ev.LastMoneyDividend = reader.GetString (4); //股息(現金)-divd007
                }
                reader.Close ();

            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error2 " + ex.Number + " : " + ex.Message);
            }

            try {
                StrSQL = "select IFNULL(ROUND(AVG(divd009)/100,2),0) from divd_t where divd001 = ?divd001 and divd002 >= ?divd002 ";
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                myCmd.Parameters.AddWithValue ("@divd001", ev.stockNo);
                myCmd.Parameters.AddWithValue ("@divd002", year - 5);
                MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader
                while (reader.Read ()) {
                    ev.EPSUseRatio = reader.GetString (0); //五年內發放率-divd009
                }
                reader.Close ();

            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error2 " + ex.Number + " : " + ex.Message);
            }
        }

        public void getRevenueIncreaseRatio (int year) {
            try {

                StrSQL = "select (opme010/100)+1 from opme_t where opme001 = ?opme001 and opme002 = ?opme002  order by opme003 desc limit 1 ";
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                myCmd.Parameters.AddWithValue ("@opme001", ev.stockNo);
                myCmd.Parameters.AddWithValue ("@opme002", year);
                MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader
                while (reader.Read ()) {

                    ev.RevenueIncreaseRatio = reader.GetString (0); //EPS-opme010
                }
                reader.Close ();

            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error2 " + ex.Number + " : " + ex.Message);
            }
        }

        //五年平均EPS &　五年平均殖利率
        public void getFiveYearsAvgerageEPSandDividend (int year) {
            try {

                StrSQL = "select IFNULL(ROUND(AVG(divd008),2),0),IFNULL(ROUND(AVG(divd010),2),0) from divd_t where divd001 = ?divd001 and divd002 between ?divd002s and ?divd002e ";
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                myCmd.Parameters.AddWithValue ("@divd001", ev.stockNo);
                myCmd.Parameters.AddWithValue ("@divd002s", year - 4); //起始年
                myCmd.Parameters.AddWithValue ("@divd002e", year); //今年
                MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader
                while (reader.Read ()) {
                    ev.FiveYearsAvgerageEPS = reader.GetString (0); //EPS-divd008
                    ev.FiveYearsAvgerageDividend = reader.GetString (1); //EPS-divd010
                }
                reader.Close ();

            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error2 " + ex.Number + " : " + ex.Message);
            }
        }

        //近4季EPS
        public void getLastFourEPS () {
            try {

                StrSQL = "select epsl004 from epsl_t,stck_t where epsl001 = stck001 and stck003 = 'Y' and epsl001 = ?epsl001 and epsl004 is not null and epsl003 <> 'ALL' order by epsl002 desc,epsl003 desc limit 4 ";
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                myCmd.Parameters.AddWithValue ("@epsl001", ev.stockNo);
                MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader
                while (reader.Read ()) {
                    ev.LastFourEPS = reader.GetString (0) + "|" + ev.LastFourEPS; //EPS-epsl004
                }
                reader.Close ();

            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error2 " + ex.Number + " : " + ex.Message);
            }
        }

        //最後營收更新年月
        public void getOperatingUpdateTime () {
            try {

                StrSQL = "select opme002,opme003 from opme_t where opme001=?opme001 order by opme002 desc,opme003 desc limit 1 ";
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                myCmd.Parameters.AddWithValue ("@opme001", ev.stockNo);
                MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader
                while (reader.Read ()) {
                    ev.OperatingUpdateTime = reader.GetString (0) + "/" + reader.GetString (1); //opme002(年)+opme003(月)
                }
                reader.Close ();

            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error2 " + ex.Number + " : " + ex.Message);
            }
        }
        public void getPriceAndUpdateDate () {

            string type = "";

            //先確定是上市(TWSE)或上櫃(TPEX)
            try {

                StrSQL = "select stck002 from stck_t where stck001 = ?stck001 ";
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                myCmd.Parameters.AddWithValue ("@stck001", ev.stockNo);
                MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader
                while (reader.Read ()) {
                    type = reader.GetString (0); //類型-stck002
                }
                reader.Close ();

            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error2 " + ex.Number + " : " + ex.Message);
            }

            if (type == "TWSE") {
                //上市股票
                try {
                    StrSQL = "select twse007,twse002 from twse_t where twse001 = ?twse001 and twse007 not like '%-%' order by twse002 desc limit 1 ";
                    MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                    myCmd.Parameters.AddWithValue ("@twse001", ev.stockNo);
                    MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader
                    while (reader.Read ()) {
                        ev.Price = reader.GetString (0); //收盤價-twse007
                        ev.UpdateDate = reader.GetString (1); //更新日-twse002
                    }
                    reader.Close ();

                } catch (MySql.Data.MySqlClient.MySqlException ex) {
                    Console.WriteLine ("Error2 " + ex.Number + " : " + ex.Message);
                }
            } else {
                //上櫃股票
                try {
                    StrSQL = "select tpex007,tpex002 from tpex_t where tpex001 = ?tpex001 and tpex007 not like '%-%' order by tpex002 desc limit 1 ";
                    MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                    myCmd.Parameters.AddWithValue ("@tpex001", ev.stockNo);
                    MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader
                    while (reader.Read ()) {
                        ev.Price = reader.GetString (0); //收盤價-twse007
                        ev.UpdateDate = reader.GetString (1); //更新日-twse002
                    }
                    reader.Close ();

                } catch (MySql.Data.MySqlClient.MySqlException ex) {
                    Console.WriteLine ("Error2 " + ex.Number + " : " + ex.Message);
                }
            }
        }

        public void getFiveYearsAvgeragePrice () {

            string type = "";

            //先確定是上市(TWSE)或上櫃(TPEX)
            try {

                StrSQL = "select stck002 from stck_t where stck001 = ?stck001 ";
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                myCmd.Parameters.AddWithValue ("@stck001", ev.stockNo);
                MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader
                while (reader.Read ()) {
                    type = reader.GetString (0); //類型-stck002
                }
                reader.Close ();

            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error2 " + ex.Number + " : " + ex.Message);
            }

            string prices = "";
            if (type == "TWSE") {
                //上市股票
                try {
                    StrSQL = "select EXTRACT(YEAR FROM twse002),ROUND(avg(twse007),2) from twse_t where twse001 = ?twse001 group by EXTRACT(YEAR FROM twse002) order by EXTRACT(YEAR FROM twse002) desc limit 5";
                    MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                    myCmd.Parameters.AddWithValue ("@twse001", ev.stockNo);
                    MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader
                    while (reader.Read ()) {
                        prices = reader.GetString (1) + "|" + prices; //均價
                    }
                    reader.Close ();

                } catch (MySql.Data.MySqlClient.MySqlException ex) {
                    Console.WriteLine ("Error2 " + ex.Number + " : " + ex.Message);
                }
            } else {
                //上櫃股票
                try {
                    StrSQL = "select EXTRACT(YEAR FROM tpex002),ROUND(avg(tpex007),2) from tpex_t where tpex001 = ?tpex001 group by EXTRACT(YEAR FROM tpex002) order by EXTRACT(YEAR FROM tpex002) desc limit 5 ";
                    MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                    myCmd.Parameters.AddWithValue ("@tpex001", ev.stockNo);
                    MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader
                    while (reader.Read ()) {
                        prices = reader.GetString (1) + "|" + prices; //均價
                    }
                    reader.Close ();

                } catch (MySql.Data.MySqlClient.MySqlException ex) {
                    Console.WriteLine ("Error2 " + ex.Number + " : " + ex.Message);
                }
            }

            ev.FiveYearsAvgeragePrice = prices;
        }

        public void getType () {

            //先確定是上市(TWSE)或上櫃(TPEX)
            try {

                StrSQL = "select IFNULL(stck005,'未知') from stck_t where stck001 = ?stck001 ";
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                myCmd.Parameters.AddWithValue ("@stck001", ev.stockNo);
                MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader
                while (reader.Read ()) {
                    ev.Type = reader.GetString (0); //類型-stck005
                }
                reader.Close ();

            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine(ex.Code);
            }

        }

    }

}