using System;
using System.Collections.Generic;
using System.IO;
using MySql.Data.MySqlClient;

namespace ReportOfStock {
    class Program {
        static MySqlConnection conn;
        static void Main (string[] args) {

            //進行資料庫連線
            database db = new database ();
            conn = db.dbConect ();
            //準備好檔案
            string filePath = System.Environment.CurrentDirectory;

            using (StreamWriter outputFile = new StreamWriter (Path.Combine (filePath, "RecommendedList.csv"))) {

                Console.WriteLine ("檔案產出路徑:" + Path.Combine (filePath, "RecommendedList.csv"));

                //產生時間
                //outputFile.WriteLine ("產生時間:" + DateTime.Now);

                string title = "編號,名稱,類別,去年EPS,五年平均EPS,近四季EPS,預測EPS,預測本益比,去年股利,去年股息,預測股利,五年平均殖利率,預測殖利率,合理價位,目前價格,五年均價,營收更新月,推(股利),推(EPS)";
                outputFile.WriteLine (title);

                //抓取資料並產出報表
                string src;
                if (args.Length == 0) {
                    src = "stck_t";
                } else {
                    src = "stck_high_priority";
                }

                List<string> stockList = getList (src);
                //stockList = new List<string> ();
                //stockList.Add ("2820");

                foreach (string stockNo in stockList) {
                    Console.WriteLine ("開始運算..." + stockNo);
                    parameters para = new parameters (stockNo, conn);
                    //(當年淨利 / 當年累營收) / (去年淨利 / 去年營收) * 去年EPS * 累積年增率/ 股數增加率 = 當年度[預測]EPS
                    double preEPS = Math.Round ((para.ThisYearAccumulationProfit / para.ThisYearAccumulationOperatingIncome) / (para.LastYearAccumulationProfit / para.LastYearAccumulationOperatingIncome) *
                        para.LastEPS * para.RevenueIncreaseRatio / para.StockIncreaseRatio, 2);
                    /*                                          Console.WriteLine ("ThisYearAccumulationProfit:" + para.ThisYearAccumulationProfit);
                                                            Console.WriteLine ("ThisYearAccumulationOperatingIncome:" + para.ThisYearAccumulationOperatingIncome);
                                                            Console.WriteLine ("LastYearAccumulationProfit:" + para.LastYearAccumulationProfit);
                                                            Console.WriteLine ("LastYearAccumulationOperatingIncome:" + para.LastYearAccumulationOperatingIncome);
                                                            Console.WriteLine ("LastEPS:" + para.LastEPS);
                                                            Console.WriteLine ("RevenueIncreaseRatio:" + para.RevenueIncreaseRatio);
                                                            Console.WriteLine ("StockIncreaseRatio:" + para.StockIncreaseRatio);
                                                            Console.WriteLine ("preEPS:" + preEPS);
                                          */
                    //當年度[預測]EPS*股利發放率/7% = 合理價位

                    double preDividend = Math.Round (preEPS * para.EPSUseRatio, 2);

                    double goodPrice = Math.Round (preDividend / 0.07, 2);

                    //預測本益比
                    double preEPSRate = Math.Round (para.Price / preEPS, 2);

                    //預測殖利率
                    double preDividendRadio = Math.Round (preDividend / para.Price * 100, 2);

                    //是否關注(by股利)
                    string attention = "";
                    //現價*1.2<目標價
                    //預測EPS不超過五年平均*5
                    //預測EPS>0
                    if ((goodPrice / para.Price) > 1 && (preEPS / para.FiveYearsAvgerageEPS) < 5 && preEPS > 0) {
                        attention = "Y";
                    } else {
                        attention = "N";
                    }

                    //是否關注(byEPS)
                    string attention2 = "";
                    //preESP大於現價10%
                    //預測EPS不超過五年平均*5
                    //預測EPS>0
                    //近五年平均殖利率>3.5%
                    //累積成長率每月平均達1%
                    //發放率不大於100%
                    double RevenueIncreaseRatioEveryMonth = (para.RevenueIncreaseRatio - 1) * 100 / DateTime.Now.Month;
                    //Console.WriteLine(RevenueIncreaseRatioEveryMonth);
                    //Console.WriteLine(para.RevenueIncreaseRatio);
                    if ((para.Price / preEPS) < 15 &&
                        (preEPS / para.FiveYearsAvgerageEPS) < 5 &&
                        preEPS > 0 &&
                        para.FiveYearsAvgerageDividend > 3.5 &&
                        RevenueIncreaseRatioEveryMonth > 1 &&
                        para.EPSUseRatio < 1
                    ) {
                        attention2 = "Y";
                    } else {
                        attention2 = "N";
                    }

                    //股價預測(股票編號,股票名稱,去年EPS,五年平均EPS,預測EPS,去年股利,預測股利,合理價位,目前價格)
                    if (para.StockIncreaseRatio == 0) {
                        //資料不完整, 沒有EPS, 該筆不處理
                        Console.WriteLine ("此股資料不齊全:" + stockNo);
                    } else {
                        string content = stockNo + "," + //股票編號
                            para.Name + "," + //股票名稱
                            para.Type + "," + //股票類別
                            para.LastEPS + "," + //去年EPS
                            para.FiveYearsAvgerageEPS + "," + //五年平均EPS
                            para.LastFourEPS + "," + //近四季EPS
                            preEPS + "," + //預測EPS
                            preEPSRate + "," + //預測本益比
                            para.LastMoneyDividend + "," + //去年股利
                            para.LastStockDividend + "," + //去年股息
                            //para.RevenueIncreaseRatio + "," + //發放率
                            preDividend + "," + //預測股利
                            para.FiveYearsAvgerageDividend + "," + //五年平均殖利率
                            preDividendRadio + "," + //預測殖利率
                            goodPrice + "," + //合理價位
                            para.Price + "," + //目前價格
                            para.FiveYearsAvgeragePrice + "," + //五年均價
                            para.OperatingUpdateTime + "," + //營收更新月
                            attention + "," + //關注否(by股利)
                            attention2; //關注否(by股利)
                        //Console.WriteLine (content);
                        outputFile.WriteLine (content);
                    }
                }
            }
        }

        public static List<string> getList (string src) {
            List<string> StockList = new List<string> ();

            // 進行select (取出有股利資料 但無EPS資料的清單, 區間為當年份)
            try {

                //決定是否只找高優先的

                string StrSQL = "select distinct stck001 from " + src + ",divd_t,opme_t,prft_t where stckstus='Y' and stck003='Y' " +
                    " and stck001 = divd001 and divd002 = ?year " +
                    " and stck001 = opme001 and opme002 = ?year " +
                    " and stck001 = prft001 and opme002 = ?year " +
                    " order by stck004,stck001";
                //" and not exists ( select 1 from epsl_t where epsl001=stck001) ";
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                myCmd.Parameters.AddWithValue ("@year", DateTime.Now.Year);
                MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader
                while (reader.Read ()) {
                    String stockNo = reader.GetString (0);
                    StockList.Add (stockNo);
                    //Console.WriteLine(no);
                }
                reader.Close ();

            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error2 " + ex.Number + " : " + ex.Message);
            }

            return StockList;
        }
    }
}