using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
public class parameters {

    //先定義好相關變數
    public string Name = ""; //名稱
    public string? Type = ""; //類別
    public double FiveYearsAvgerageEPS = new double (); //五年平均EPS

    public string LastFourEPS = ""; //近四季平均EPS
    public double LastMoneyDividend = new double (); //去年股利(現金)
    public double LastStockDividend = new double (); //去年股利(股票)
    public double FiveYearsAvgerageDividend = new double (); //五年平均殖利率
    public string FiveYearsAvgeragePrice = ""; //五年均價
    public double Price = new double (); //目前價格

    public string K = ""; //K
    public string D = ""; //D
    public string UpdateDate = ""; //價格更新日
    public double ThisYearAccumulationProfit = new double (); //當年淨利
    public double ThisYearAccumulationOperatingIncome = new double (); //當年營收
    public double LastYearAccumulationProfit = new double (); //去年淨利
    public double LastYearAccumulationOperatingIncome = new double (); //去年營收
    public double LastEPS = new double (); //去年EPS
    public double RevenueIncreaseRatio = new double (); //累積年增率(營收增加比率)
    public double StockIncreaseRatio = new double (); //股票(股數)增加比率

    public double EPSUseRatio = new double (); //EPS發放率

    public string OperatingUpdateTime = ""; //營收更新年月
    public string no;
    MySqlConnection conn;
    string StrSQL;
    public parameters (string stockNo, MySqlConnection dbconn) {
        no = stockNo;
        conn = dbconn;

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
    }

    public void getYearName () {
        try {

            StrSQL = "select name003 from name_t where name001 = ?name001 and name002 = ?name002 ";
            MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
            myCmd.Parameters.AddWithValue ("@name001", no);
            myCmd.Parameters.AddWithValue ("@name002", "zh_TW");
            MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader
            while (reader.Read ()) {

                Name = reader.GetString (0); //名稱-name003

            }
            reader.Close ();

        } catch (MySql.Data.MySqlClient.MySqlException ex) {
            Console.WriteLine ("Error2 " + ex.Number + " : " + ex.Message);
        }
    }

    //取得指定年份營收AND淨利
    public void getYearAccumulationOperatingIncomeAndYearAccumulationProfit (int year, string type) {
        try {

            StrSQL = "select sum(prft004),sum(prft008) from prft_t where prft001 = ?prft001 and prft002 = ?prft002 ";
            MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
            myCmd.Parameters.AddWithValue ("@prft001", no);
            myCmd.Parameters.AddWithValue ("@prft002", year);
            MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader
            while (reader.Read ()) {
                if (type == "this") {
                    ThisYearAccumulationOperatingIncome = Double.Parse (reader.GetString (0)); //營收-prft004
                    ThisYearAccumulationProfit = Double.Parse (reader.GetString (1)); //純利-prft008
                } else {
                    LastYearAccumulationOperatingIncome = Double.Parse (reader.GetString (0)); //營收-prft004
                    LastYearAccumulationProfit = Double.Parse (reader.GetString (1)); //純利-prft008
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
            myCmd.Parameters.AddWithValue ("@divd001", no);
            myCmd.Parameters.AddWithValue ("@divd002", year);
            MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader
            while (reader.Read ()) {

                LastEPS = Double.Parse (reader.GetString (0)); //EPS-divd008
                //EPSUseRatio = Double.Parse (reader.GetString (1)); //發放率-divd009
                StockIncreaseRatio = Double.Parse (reader.GetString (2)); //股息-divd006
                LastStockDividend = Double.Parse (reader.GetString (3)); //股息(股票)-divd006
                LastMoneyDividend = Double.Parse (reader.GetString (4)); //股息(現金)-divd007
            }
            reader.Close ();

        } catch (MySql.Data.MySqlClient.MySqlException ex) {
            Console.WriteLine ("Error2 " + ex.Number + " : " + ex.Message);
        }

        try {
            StrSQL = "select AVG(divd009)/100 from divd_t where divd001 = ?divd001 and divd002 >= ?divd002 ";
            MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
            myCmd.Parameters.AddWithValue ("@divd001", no);
            myCmd.Parameters.AddWithValue ("@divd002", year - 5);
            MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader
            while (reader.Read ()) {
                EPSUseRatio = Double.Parse (reader.GetString (0)); //五年內發放率-divd009
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
            myCmd.Parameters.AddWithValue ("@opme001", no);
            myCmd.Parameters.AddWithValue ("@opme002", year);
            MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader
            while (reader.Read ()) {

                RevenueIncreaseRatio = Double.Parse (reader.GetString (0)); //EPS-opme010
            }
            reader.Close ();

        } catch (MySql.Data.MySqlClient.MySqlException ex) {
            Console.WriteLine ("Error2 " + ex.Number + " : " + ex.Message);
        }
    }

    //五年平均EPS &　五年平均殖利率
    public void getFiveYearsAvgerageEPSandDividend (int year) {
        try {

            StrSQL = "select ROUND(AVG(divd008),2),ROUND(AVG(divd010),2) from divd_t where divd001 = ?divd001 and divd002 between ?divd002s and ?divd002e ";
            MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
            myCmd.Parameters.AddWithValue ("@divd001", no);
            myCmd.Parameters.AddWithValue ("@divd002s", year - 4); //起始年
            myCmd.Parameters.AddWithValue ("@divd002e", year); //今年
            MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader
            while (reader.Read ()) {
                FiveYearsAvgerageEPS = Double.Parse (reader.GetString (0)); //EPS-divd008
                FiveYearsAvgerageDividend = Double.Parse (reader.GetString (1)); //EPS-divd010
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
            myCmd.Parameters.AddWithValue ("@epsl001", no);
            MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader
            while (reader.Read ()) {
                LastFourEPS = reader.GetString (0) + "|" + LastFourEPS; //EPS-epsl004
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
            myCmd.Parameters.AddWithValue ("@opme001", no);
            MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader
            while (reader.Read ()) {
                OperatingUpdateTime = reader.GetString (0) + "/" + reader.GetString (1); //opme002(年)+opme003(月)
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
            myCmd.Parameters.AddWithValue ("@stck001", no);
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
                StrSQL = "select twse007,twse002,twse011,twse012 from twse_t where twse001 = ?twse001 and twse007 not like '%-%' order by twse002 desc limit 1 ";
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                myCmd.Parameters.AddWithValue ("@twse001", no);
                MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader
                while (reader.Read ()) {
                    Price = Double.Parse (reader.GetString (0)); //收盤價-twse007
                    UpdateDate = reader.GetString (1); //更新日-twse002
                    K = reader.GetString (2); //K-twse011
                    D = reader.GetString (3); //D-twse012
                }
                reader.Close ();

            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error2 " + ex.Number + " : " + ex.Message);
            }
        } else {
            //上櫃股票
            try {
                StrSQL = "select tpex007,tpex002,tpex011,tpex012 from tpex_t where tpex001 = ?tpex001 and tpex007 not like '%-%' order by tpex002 desc limit 1 ";
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                myCmd.Parameters.AddWithValue ("@tpex001", no);
                MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader
                while (reader.Read ()) {
                    Price = Double.Parse (reader.GetString (0)); //收盤價-twse007
                    UpdateDate = reader.GetString (1); //更新日-twse002
                    K = reader.GetString (2); //K-twse011
                    D = reader.GetString (3); //D-twse012
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
            myCmd.Parameters.AddWithValue ("@stck001", no);
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
                myCmd.Parameters.AddWithValue ("@twse001", no);
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
                myCmd.Parameters.AddWithValue ("@tpex001", no);
                MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader
                while (reader.Read ()) {
                    prices = reader.GetString (1) + "|" + prices; //均價
                }
                reader.Close ();

            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error2 " + ex.Number + " : " + ex.Message);
            }
        }

        FiveYearsAvgeragePrice = prices;
    }

    public void getType () {

        //先確定是上市(TWSE)或上櫃(TPEX)
        try {

            StrSQL = "select IFNULL(stck005,'未知') from stck_t where stck001 = ?stck001 ";
            MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
            myCmd.Parameters.AddWithValue ("@stck001", no);
            MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader
            while (reader.Read ()) {
                Type = reader.GetString (0); //類型-stck005
            }
            reader.Close ();

        } catch (MySql.Data.MySqlClient.MySqlException ex) {

        }

    }

}