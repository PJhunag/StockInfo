using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace StockInfoAPI.Controllers {
    [ApiController]
    [Route ("[controller]")]
    [Route ("[controller]/{para}")]

    public class googleLogin : ControllerBase {
        static string google_client_id = "1043116507129-flcmle4rt4a4u9mdr11gf3eme7jrv86r.apps.googleusercontent.com";
        static string google_secret_id = "MA2QoMA1mWgewnb8V1Rac70v";
        static string google_callback_url = "http://mystockbyth.ddns.net:5000/googleLogin/callback";
        static string serverSite = "http:/mystockbyth.ddns.net:3000";

        [HttpGet]
        public string Get (string para) {

            ControllerContext.HttpContext.Response.Headers.Add ("Access-Control-Allow-Origin", "*");
            ControllerContext.HttpContext.Response.Headers.Add ("Access-Control-Allow-Headers", "Content-Type");
            ControllerContext.HttpContext.Response.Headers.Add ("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");

            if (para == "callback") {
                string rtn_url = "";
                try {

                    string queryCode = Request.Query["Code"];
                    //queryCode = queryCode.Replace("%2F","/"); //把保留字轉回

                    //定義POST內容
                    string url = "https://www.googleapis.com/oauth2/v4/token";
                    HttpWebRequest request = (HttpWebRequest) WebRequest.Create (url);
                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";

                    //必須透過ParseQueryString()來建立NameValueCollection物件，之後.ToString()才能轉換成queryString
                    NameValueCollection postParams = System.Web.HttpUtility.ParseQueryString (string.Empty);
                    postParams.Add ("code", queryCode);
                    postParams.Add ("client_id", google_client_id);
                    postParams.Add ("client_secret", google_secret_id);
                    postParams.Add ("grant_type", "authorization_code");
                    postParams.Add ("redirect_uri", google_callback_url);

                    //Console.WriteLine(postParams.ToString());// 將取得"version=1.0&action=preserveCodeCheck&pCode=pCode&TxID=guid&appId=appId", key和value會自動UrlEncode
                    //要發送的字串轉為byte[] 
                    byte[] byteArray = Encoding.UTF8.GetBytes (postParams.ToString ());
                    using (Stream reqStream = request.GetRequestStream ()) {
                        reqStream.Write (byteArray, 0, byteArray.Length);
                    } //end using

                    //API回傳的字串(token資訊)
                    string responseStr = "";
                    //發出Request
                    using (WebResponse response = request.GetResponse ()) {
                        using (StreamReader sr = new StreamReader (response.GetResponseStream (), Encoding.UTF8)) {
                            responseStr = sr.ReadToEnd ();
                        } //end using  
                    }
                    Console.WriteLine ("responseStr:" + responseStr);
                    GoogleAccessToken token = JsonConvert.DeserializeObject<GoogleAccessToken> (responseStr);
                    string accessToken = token.access_token;
                    Console.WriteLine ("token:" + accessToken);

                    //用token再拿到使用者資訊(信箱)
                    url = "https://www.googleapis.com/oauth2/v1/userinfo?alt=json&access_token=" + accessToken;
                    HttpWebRequest request2 = (HttpWebRequest) WebRequest.Create (url);
                    request2.Credentials = CredentialCache.DefaultCredentials;
                    //Stream NewReqstream = request.GetRequestStream();
                    request2.Method = "GET";

                    // Get the response
                    HttpWebResponse response2 = (HttpWebResponse) request2.GetResponse ();
                    string userString = "";
                    using (Stream resStream = response2.GetResponseStream ()) {
                        using (StreamReader reader = new StreamReader (resStream, Encoding.UTF8)) {
                            userString = reader.ReadToEnd ().ToString ();
                        }
                    }

                    //轉回物件
                    GoogleUserInfo userInfo = JsonConvert.DeserializeObject<GoogleUserInfo> (userString);

                    //取得名稱
                    Console.WriteLine ("email:" + userInfo.email);
                    Console.WriteLine ("name:" + userInfo.name);

                    //連線資料庫
                    database db = new database ();
                    MySqlConnection conn = db.dbConect ();

                    //開始比對資料庫是否已有該使用者資訊
                    try {
                        string StrSQL = "select count(1) from user_t where user001 = ?user001";

                        MySqlCommand myCmd = new MySqlCommand (StrSQL, conn);
                        myCmd.Parameters.AddWithValue ("@user001", userInfo.email);
                        MySqlDataReader reader = myCmd.ExecuteReader (); //execure the reader

                        //檢核是否已註冊
                        int chkExist = 0;
                        while (reader.Read ()) {
                            chkExist = Int32.Parse (reader.GetString (0)); //存在否
                        }
                        reader.Close ();

                        if (chkExist == 0) {
                            //不存在, 開始建立帳號
                            StrSQL = "insert into user_t (userstus,user001,user002,user003,user004,user005) " +
                                " values ('Y',?user001,?user002,?user003,?user004,?user005) ";
                            myCmd = new MySqlCommand (StrSQL, conn);
                            myCmd.Parameters.AddWithValue ("@user001", userInfo.email); //ID(信箱/ID)
                            myCmd.Parameters.AddWithValue ("@user002", "google"); //來源(google,FB)
                            myCmd.Parameters.AddWithValue ("@user003", userInfo.name); //使用者名稱
                            myCmd.Parameters.AddWithValue ("@user004", 0); //權限(0~100)
                            myCmd.Parameters.AddWithValue ("@user005", DateTime.Now); //建立日期
                            if (myCmd.ExecuteNonQuery () > 0) {
                                //Console.WriteLine ("數據新增成功！");
                            }
                        } else {
                            //已存在, 不處理
                        }

                    } catch (MySql.Data.MySqlClient.MySqlException ex) {
                        Console.WriteLine ("Error2 " + ex.Number + " : " + ex.Message);
                    }

                    //跳轉回原頁面並帶上使用者資訊
                    Console.WriteLine ("轉址回首頁");

                    if (userInfo.name == "徐英哲") {
                        userInfo.name = "Wuchi";
                    }

                    Response.Headers["Location"] = "http://mystockbyth.ddns.net:3000/login?name=" + userInfo.name + "&id=" + userInfo.email;
                    Response.Redirect ("http://mystockbyth.ddns.net:3000/login?name=" + userInfo.name + "&id=" + userInfo.email);

                } catch (MySql.Data.MySqlClient.MySqlException ex) {
                    Console.WriteLine ("Error2 " + ex.Number + " : " + ex.Message);
                }

                return rtn_url;
            } else {
                string google_oauth_url = "";
                try {

                    //組合登入URL
                    google_oauth_url = "https://accounts.google.com/o/oauth2/v2/auth?" +
                        //Scope可以參考文件裡各式各樣的scope，可以貼scope url或是個別命名
                        "scope=email%20profile&" +
                        "redirect_uri=" + google_callback_url + "&" +
                        "response_type=code&" +
                        "client_id=" + google_client_id;

                } catch (MySql.Data.MySqlClient.MySqlException ex) {
                    Console.WriteLine ("Error2 " + ex.Number + " : " + ex.Message);
                }

                return google_oauth_url;
            }
        }

    }

}