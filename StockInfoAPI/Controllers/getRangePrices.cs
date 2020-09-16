using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace StockInfoAPI.Controllers {
    [ApiController]
    [Route ("[controller]/{id}/{sTime}/{eTime}/{range}")]
    public class getRangePrices : ControllerBase {
        [HttpGet]
        public List<StockPrice> Get (string id, string sTime, string eTime, string range) {

            ControllerContext.HttpContext.Response.Headers.Add ("Access-Control-Allow-Origin", "*");
            ControllerContext.HttpContext.Response.Headers.Add ("Access-Control-Allow-Headers", "Content-Type");
            ControllerContext.HttpContext.Response.Headers.Add ("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");

            List<StockPrice> StockList = new List<StockPrice> ();
            database db = new database ();
            MySqlConnection conn = db.dbConect ();

            //先抓取該股票類型, 上市(TWSE)或上櫃(TPEX)
            string type = "";
           //Console.WriteLine ("range " + range);
            try {
                string StrSQL = "select stck002 from stck_t where stckstus = 'Y' and stck001 = ?stck001";

                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                myCmd.Parameters.AddWithValue ("@stck001", id);
                MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader

                while (reader.Read ()) {
                    type = reader.GetString (0); //stck002
                }
                reader.Close ();

            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error2 " + ex.Number + " : " + ex.Message);
            }

            try {
                string StrSQL = "";
                string addWc = "";

                if (type == "TWSE") {
                    switch (range) {
                        case "1_month":
                            break;
                        case "3_month":
                            addWc = "AND DAYOFWEEK(twse002) IN (2,4,6)";
                            break;
                        case "6_month":
                            addWc = "AND DAYOFWEEK(twse002) IN (3,6)";
                            break;
                        case "1_year":
                            addWc = "AND DAYOFWEEK(twse002) = 6";
                            break;
                        case "3_year":
                            addWc = "AND DAYOFWEEK(twse002) = 6";
                            break;
                        case "all_year":
                            addWc = "AND DAYOFWEEK(twse002) = 6";
                            break;
                    }
                    StrSQL = "select twse002,twse007 from twse_t where twse001 = ?id and twse002 between ?sTime and ?eTime AND twse007 is not null " + addWc;
                } else {
                    switch (range) {
                        case "1_month":
                            break;
                        case "3_month":
                            addWc = "AND DAYOFWEEK(tpex002) IN (2,4,6)";
                            break;
                        case "6_month":
                            addWc = "AND DAYOFWEEK(tpex002) IN (3,6)";
                            break;
                        case "1_year":
                            addWc = "AND DAYOFWEEK(tpex002) = 6";
                            break;
                        case "3_year":
                            addWc = "AND DAYOFWEEK(tpex002) = 6";
                            break;
                        case "all_year":
                            addWc = "AND DAYOFWEEK(tpex002) = 6";
                            break;
                    }
                    StrSQL = "select tpex002,tpex007 from tpex_t where tpex001 = ?id and tpex002 between ?sTime and ?eTime AND tpex007 is not null " + addWc;
                }

                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                myCmd.Parameters.AddWithValue ("@id", id);
                myCmd.Parameters.AddWithValue ("@sTime", sTime);
                myCmd.Parameters.AddWithValue ("@eTime", eTime);
                MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader
                //Console.WriteLine ("AAA");

                while (reader.Read ()) {
                    StockPrice info = new StockPrice ();
                    info.date = reader.GetString (0); //日期
                    info.date = info.date.Replace (" 上午 12:00:00", "");
                    info.price = Double.Parse (reader.GetString (1)); //收盤價
                    StockList.Add (info);

                }
                reader.Close ();

            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error2 " + ex.Number + " : " + ex.Message);
            }

            //Console.WriteLine (JsonConvert.SerializeObject (StockList));

            return StockList;
        }

    }

}