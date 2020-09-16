using System;
using MySql.Data.MySqlClient;
public class database {

    public MySqlConnection dbConect () {
        string dbHost = "localhost";
        string dbUser = "root";
        string dbPass = "root";
        string dbName = "stock";
        MySqlConnection conn;

        // 如果有特殊的編碼在database後面請加上;CharSet=編碼, utf8請使用utf8_general_ci
        string connStr = "server=" + dbHost + ";uid=" + dbUser + ";pwd=" + dbPass + ";database=" + dbName;
        conn = new MySqlConnection (connStr);

        // 連線到資料庫
        try {
            //Console.WriteLine ("資料庫連線成功!");
            conn.Open ();
        } catch (MySql.Data.MySqlClient.MySqlException ex) {
            switch (ex.Number) {
                case 0:
                    Console.WriteLine ("無法連線到資料庫.");
                    break;
                case 1045:
                    Console.WriteLine ("使用者帳號或密碼錯誤,請再試一次.");
                    break;
            }
        }

        return conn;
    }
}