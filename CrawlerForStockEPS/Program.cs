using System;
using MySql.Data.MySqlClient;
//股利爬蟲
namespace CrawlerForStockEPS {
    class Program {

        static MySqlConnection conn;
        static void Main (string[] args) {

            //進行資料庫連線
            database db = new database ();
            conn = db.dbConect ();

            //定義此次要運行的項目
            string type = "ALL";

            //抓取依據
            string src = "stck_t";

            if (args.Length > 0) {
                //此次依據來源為
                src = "stck_high_priority";
            } else {
                src = "stck_t";
            }
            Console.WriteLine ("此次依據來源為:" + src);

             //抓取上市價格(固定抓取)
            Console.WriteLine ("開始抓每日(上市)價格......");
            ReadTwseStockPrice getTWSEprice = new ReadTwseStockPrice (conn, src);

            //抓取上櫃價格(固定抓取)
            Console.WriteLine ("------------------------------------------------------------------------------");
            Console.WriteLine ("開始抓每日(上櫃)價格......");
            ReadTpexStockPrice getTPEXprice = new ReadTpexStockPrice (conn, src);

            //計算KD
            if (type == "ALL" || type == "KD") {
                Console.WriteLine ("------------------------------------------------------------------------------");
                Console.WriteLine ("開始計算KD......");
                getRSVAndKD getKD = new getRSVAndKD (conn, src);
            }

            //抓取EPS
            if (type == "ALL" || type == "EPS") {
                Console.WriteLine ("------------------------------------------------------------------------------");
                Console.WriteLine ("開始抓取EPS......");
                ReadEPS getEPSs = new ReadEPS (conn, src);
            }

            //抓取營收
            if (type == "ALL" || type == "OperatingIncomes") {
                Console.WriteLine ("------------------------------------------------------------------------------");
                Console.WriteLine ("開始抓月營收......");
                ReadOperatingIncomes getOperatingIncomes = new ReadOperatingIncomes (conn, src);
            }

            //抓取損益表
            if (type == "ALL" || type == "Profit") {
                Console.WriteLine ("------------------------------------------------------------------------------");
                Console.WriteLine ("開始抓損益......");
                ReadProfit getProfit = new ReadProfit (conn, src);
            }

            //抓取歷史股利
            if (type == "ALL" || type == "Dividend") {
                Console.WriteLine ("------------------------------------------------------------------------------");
                Console.WriteLine ("開始抓歷史股利......");
                ReadDividend getDividend = new ReadDividend (conn, src);
            }   
            
            //抓平均年線
            if (type == "ALL" || type == "YearLine") {
                Console.WriteLine ("------------------------------------------------------------------------------");
                Console.WriteLine ("開始抓平均年線......");
                getYearLine getYL = new getYearLine (conn, src);
            }  
            
        }
    }
}