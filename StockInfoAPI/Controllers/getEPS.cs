using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace StockInfoAPI.Controllers {
    [ApiController]
    [Route ("[controller]/{id}")]
    public class getEPS : ControllerBase {
        [HttpGet]
        /// <summary>
        /// 取得指定股票歷年EPS紀錄
        /// </summary>
        /// <param name="id">指定股票編號</param>
        /// <returns>返回歷年EPS紀錄[{year:"年度",season:"季度",eps:"EPS"}</returns>
        public List<StockEPS> Get (string id) {

            ControllerContext.HttpContext.Response.Headers.Add ("Access-Control-Allow-Origin", "*");
            ControllerContext.HttpContext.Response.Headers.Add ("Access-Control-Allow-Headers", "Content-Type");
            ControllerContext.HttpContext.Response.Headers.Add ("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");

            List<StockEPS> StockList = new List<StockEPS> ();
            database db = new database ();
            MySqlConnection conn = db.dbConect ();

            try {
                string StrSQL = "select * from (select epsl002,epsl003,epsl004 from epsl_t where epsl001 = ?epsl001 and epsl004 is not null and epsl003 <> 'ALL' order by epsl002 desc,epsl003 desc limit 12) t order by epsl002,epsl003 ";
                Console.WriteLine("StrSQL:"+StrSQL);
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                myCmd.Parameters.AddWithValue ("@epsl001", id);
                MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader

                while (reader.Read ()) {
                    StockEPS info = new StockEPS ();
                    info.year = Int32.Parse(reader.GetString (0)); //年度
                    info.season = reader.GetString (1); //季度
                    info.eps = reader.GetString (2); //eps
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