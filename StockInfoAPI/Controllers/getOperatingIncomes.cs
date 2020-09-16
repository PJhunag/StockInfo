using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace StockInfoAPI.Controllers {
    [ApiController]
    [Route ("[controller]/{id}")]
    public class getOperatingIncomes : ControllerBase {
        [HttpGet]
        public List<StockOperatingIncomes> Get (string id) {

            ControllerContext.HttpContext.Response.Headers.Add ("Access-Control-Allow-Origin", "*");
            ControllerContext.HttpContext.Response.Headers.Add ("Access-Control-Allow-Headers", "Content-Type");
            ControllerContext.HttpContext.Response.Headers.Add ("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");

            List<StockOperatingIncomes> StockList = new List<StockOperatingIncomes> ();
            database db = new database ();
            MySqlConnection conn = db.dbConect ();

            try {
                string StrSQL = "select opme002,opme003,opme004,opme005,opme006,opme007,opme008,opme009,opme010 from opme_t where opme001 = ?opme001 and opme004 is not null order by opme002 desc,opme003 desc";
                
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                myCmd.Parameters.AddWithValue ("@opme001", id);
                MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader

                while (reader.Read ()) {
                    StockOperatingIncomes info = new StockOperatingIncomes ();
                    info.opme002 = Int32.Parse(reader.GetString (0)); //年度
                    info.opme003 = Int32.Parse(reader.GetString (1)); //月份
                    info.opme004 = Double.Parse(reader.GetString (2)); //單月營收	
                    info.opme005 = Double.Parse(reader.GetString (3)); //去年同月營收	
                    info.opme006 = Double.Parse(reader.GetString (4)); //單月月增率	
                    info.opme007 = Double.Parse(reader.GetString (5)); //單月年增率	
                    info.opme008 = Double.Parse(reader.GetString (6)); //累計營收	
                    info.opme009 = Double.Parse(reader.GetString (7)); //去年累計營收	
                    info.opme010 = Double.Parse(reader.GetString (8)); //累積年增率   
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