using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace StockInfoAPI.Controllers {
    [ApiController]
    [Route ("[controller]/{id}")]
    public class getProfit : ControllerBase {
        [HttpGet]
        public List<StockProfit> Get (string id) {

            ControllerContext.HttpContext.Response.Headers.Add ("Access-Control-Allow-Origin", "*");
            ControllerContext.HttpContext.Response.Headers.Add ("Access-Control-Allow-Headers", "Content-Type");
            ControllerContext.HttpContext.Response.Headers.Add ("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");

            List<StockProfit> StockList = new List<StockProfit> ();
            database db = new database ();
            MySqlConnection conn = db.dbConect ();

            try {
                string StrSQL = "select prft002,prft003,prft004,prft005,prft006,prft007,prft008 from prft_t where prft001 = ?prft001 order by prft002 desc,prft003 desc";

                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                myCmd.Parameters.AddWithValue ("@prft001", id);
                MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader

                while (reader.Read ()) {
                    StockProfit info = new StockProfit ();
                    info.prft002 = Int32.Parse (reader.GetString (0)); //年度	
                    info.prft003 = reader.GetString (1); //季別	
                    if (!reader.IsDBNull (2)) {
                        info.prft004 = reader.GetString (2); //營收	
                    }
                    if (!reader.IsDBNull (3)) {
                        info.prft005 = reader.GetString (3); //毛利
                    }
                    if (!reader.IsDBNull (4)) {
                        info.prft006 = reader.GetString (4); //營業利益	
                    }
                    if (!reader.IsDBNull (5)) {
                        info.prft007 = reader.GetString (5); //稅前淨利	  
                    }
                    if (!reader.IsDBNull (6)) {
                        info.prft008 = reader.GetString (6); //稅後淨利	  
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