using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace StockInfoAPI.Controllers {
    [ApiController]
    [Route ("[controller]/{id}")]
    public class getDividend : ControllerBase {
        [HttpGet]
        public List<StockDividend> Get (string id) {

            ControllerContext.HttpContext.Response.Headers.Add ("Access-Control-Allow-Origin", "*");
            ControllerContext.HttpContext.Response.Headers.Add ("Access-Control-Allow-Headers", "Content-Type");
            ControllerContext.HttpContext.Response.Headers.Add ("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");

            List<StockDividend> StockList = new List<StockDividend> ();
            database db = new database ();
            MySqlConnection conn = db.dbConect ();

            try {
                string StrSQL = "select divd002,divd003,divd004,divd005,divd006,divd007,divd008,divd009,divd010 from divd_t where divd001 = ?divd001 order by divd002 desc";

                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                myCmd.Parameters.AddWithValue ("@divd001", id);
                MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader

                while (reader.Read ()) {
                    StockDividend info = new StockDividend ();
                    info.divd002 = Int32.Parse (reader.GetString (0)); //發放年度	
                    if (!reader.IsDBNull (1)) {
                        info.divd003 = reader.GetString (1); //除權日	
                    }
                    if (!reader.IsDBNull (2)) {
                        info.divd004 = reader.GetString (2); //除息日	
                    }
                    if (!reader.IsDBNull (3)) {
                        info.divd005 = reader.GetString (3); //除權息前股價
                    }
                    if (!reader.IsDBNull (4)) {
                        info.divd006 = reader.GetString (4); //股票股利	
                    }
                    if (!reader.IsDBNull (5)) {
                        info.divd007 = reader.GetString (5); //現金股利	  
                    }
                    if (!reader.IsDBNull (6)) {
                        info.divd008 = reader.GetString (6); //EPS	  
                    }
                    if (!reader.IsDBNull (7)) {
                        info.divd009 = reader.GetString (7); //配息率	
                    }
                    if (!reader.IsDBNull (8)) {
                        info.divd010 = reader.GetString (8); //現金殖利率   
                    }
                    StockList.Add (info);
                }
                reader.Close ();

            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error2 " + ex.Number + " : " + ex.Message);
            }

            Console.WriteLine (JsonConvert.SerializeObject (StockList));

            return StockList;
        }

    }

}