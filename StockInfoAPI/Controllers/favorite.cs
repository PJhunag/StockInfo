using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Web.Http.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace StockInfoAPI.Controllers {
    [ApiController]
    [Route ("[controller]")]
    //http://114.33.59.86:5000/favorite/get/test/None
    //http://114.33.59.86:5000/favorite/add/ggg/0050
    //http://114.33.59.86:5000/favorite/chk/ggg/0050
    //http://114.33.59.86:5000/favorite/del/ggg/0050
    //get/使用者ID/{None} -> 取得我的最愛清單
    //chk/使用者ID/股票ID -> 是否存在於我的最愛
    //add/使用者ID/股票ID -> 新增我的最愛
    //del/使用者ID/股票ID -> 刪除我的最愛

    public class favorite : ControllerBase {

        [HttpPost]
        public string Post (string type, string id, string stock) {
            ControllerContext.HttpContext.Response.Headers.Add ("Access-Control-Allow-Origin", "*");
            ControllerContext.HttpContext.Response.Headers.Add ("Access-Control-Allow-Headers", "Content-Type");
            ControllerContext.HttpContext.Response.Headers.Add ("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");

            Console.WriteLine ("aaa:" + type);

            string rtn = "";
            switch (type) {
                case "get": //取得我的最愛清單(JSON)
                    rtn = getFavorite (id);
                    break;
                case "add": //新增我的最愛
                    rtn = setFavorite (id, stock, "add");
                    break;
                case "del": //刪除我的最愛
                    rtn = setFavorite (id, stock, "del");
                    break;
                case "chk": //刪除我的最愛
                    rtn = chkFavorite (id, stock);
                    break;
            }

            return rtn;
        }

        public static string getFavorite (string id) {

            List<Stock> StockList = new List<Stock> ();
            database db = new database ();
            MySqlConnection conn = db.dbConect ();

            Console.WriteLine ("開始取得 " + id + " 的 我的最愛清單 ");

            try {
                string StrSQL = "select twse001,name003,twse007,twse008,ROUND(twse008/twse007*100,2) percent from ( " +
                    "select twse001 ,twse007,twse008 from twse_t,twseMaxDay,fvrt_t where twse002 = day and fvrt001 = ?fvrt001 and fvrt002 = twse001 UNION " +
                    "select tpex001 twse001,tpex007 twse007,tpex008 twse008 from tpex_t,tpexMaxDay,fvrt_t where tpex002 = day and fvrt001 = ?fvrt001 and fvrt002 = tpex001 ) stock " +
                    "left join name_t on name001 = twse001 and name002 = 'zh_TW' ";

                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                myCmd.Parameters.AddWithValue ("@fvrt001", id);
                MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader

                while (reader.Read ()) {
                    Stock s = new Stock ();
                    s.id = reader.GetString (0); //twse001
                    s.name = reader.GetString (1); //name003
                    s.price = reader.GetString (2); //twse007
                    s.fluct = reader.GetString (3); //twse008
                    s.percent = reader.GetString (4); //percent
                    StockList.Add (s);
                }
                reader.Close ();

            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error2 " + ex.Number + " : " + ex.Message);
            }

            return JsonConvert.SerializeObject (StockList);
        }

        public static string setFavorite (string id, string stock, string type) {

            database db = new database ();
            MySqlConnection conn = db.dbConect ();

            Console.WriteLine ("開始設置 " + id + " 的 " + stock + " 狀態[" + type + "]");

            try {
                string StrSQL = "";
                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);

                if (type == "add") {
                    //新增
                    StrSQL = "insert into fvrt_t (fvrt001,fvrt002,fvrt003) values (?fvrt001,?fvrt002,?fvrt003)";
                    myCmd = new MySqlCommand (StrSQL, conn);
                    myCmd.Parameters.AddWithValue ("@fvrt001", id);
                    myCmd.Parameters.AddWithValue ("@fvrt002", stock);
                    myCmd.Parameters.AddWithValue ("@fvrt003", DateTime.Now);
                } else {
                    //刪除
                    StrSQL = "delete from fvrt_t where fvrt001 = ?fvrt001 and fvrt002 = ?fvrt002";
                    myCmd = new MySqlCommand (StrSQL, conn);
                    myCmd.Parameters.AddWithValue ("@fvrt001", id);
                    myCmd.Parameters.AddWithValue ("@fvrt002", stock);
                }

                if (myCmd.ExecuteNonQuery () > 0) {
                    //Console.WriteLine ("數據新增成功！");
                }

            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error2 " + ex.Number + " : " + ex.Message);
                return "Error2 " + ex.Number + " : " + ex.Message;
            }

            return "true";

        }

        public static string chkFavorite (string id, string stock) {

            database db = new database ();
            MySqlConnection conn = db.dbConect ();

            Console.WriteLine ("開始檢查 " + id + " 是否添加了我的最愛 " + stock);

            string rtn = "";

            try {
                string StrSQL = "select count(1) from fvrt_t where fvrt001 = ?fvrt001 and fvrt002 = ?fvrt002";

                MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                myCmd.Parameters.AddWithValue ("@fvrt001", id);
                myCmd.Parameters.AddWithValue ("@fvrt002", stock);
                MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader

                reader.Read ();
                String chk = reader.GetString (0);
                if (chk == "1") {
                    rtn = "true";
                } else {
                    rtn = "false";
                }
                reader.Close ();

            } catch (MySql.Data.MySqlClient.MySqlException ex) {
                Console.WriteLine ("Error2 " + ex.Number + " : " + ex.Message);
            }

            Console.WriteLine ("開始檢查 " + id + " 是否添加了我的最愛 " + stock + " 結果為 " + rtn);

            return rtn;
        }

    }

}