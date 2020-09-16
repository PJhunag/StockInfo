using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace StockInfoAPI.Controllers {
    [ApiController]
    [Route ("[controller]")]
    [Route ("[controller]/{num}")]
    public class getStockList : ControllerBase {
        [HttpGet]
        public List<Stock> Get (string num) {

            ControllerContext.HttpContext.Response.Headers.Add ("Access-Control-Allow-Origin", "*");
            ControllerContext.HttpContext.Response.Headers.Add ("Access-Control-Allow-Headers", "Content-Type");
            ControllerContext.HttpContext.Response.Headers.Add ("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");

            List<Stock> StockList = new List<Stock> ();
            database db = new database ();
            MySqlConnection conn = db.dbConect ();

            try {
                string StrSQL;

                //判斷是否有下額外條件
                if (string.IsNullOrEmpty (num)) {
                    //無條件
                    StrSQL = "select stck001,name003 from stck_t left join name_t on stck001=name001 and name002='zh_TW' where stckstus = 'Y' limit 50";
                } else {
                    //有條件
                    StrSQL = "select stck001,name003 from stck_t left join name_t on stck001=name001 and name002='zh_TW' where stckstus = 'Y' and (stck001 like ?stck001 or name003 like ?stck001) limit 50";
                }

                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                myCmd.Parameters.AddWithValue ("@stck001", "%" + num + "%");
                MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader

                while (reader.Read ()) {
                    Stock info = new Stock ();
                    info.id = reader.GetString (0); //stck001
                    info.name = reader.GetString (1); //name003
                    StockList.Add (info);
                }
                reader.Close ();

            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error2 " + ex.Number + " : " + ex.Message);
            }
            //Console.WriteLine ("NUM:" + num);

            //Console.WriteLine (JsonConvert.SerializeObject (StockList));

            return StockList;
        }

    }

}