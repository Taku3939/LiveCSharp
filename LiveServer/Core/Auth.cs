using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace LiveServer
{
    public class Auth
    {
        private string serverAddress = "127.0.0.1";
        private string user = "root";
        private string password = "root";
        private string databaseName = "test_database";
        private string tableName = "test_table";
        public void Start()
        {
            string connStr = $"server={serverAddress};user id={user};password={password};database={databaseName}";
            MySqlConnection conn = new MySqlConnection(connStr);

            try
            {
             
                // 接続を開く
                conn.Open();
                MySqlDataAdapter dataAdapter = new MySqlDataAdapter("SELECT * FROM `test_table`", conn);
                // データを取得するテーブル
                DataTable tbl = new DataTable();
                dataAdapter.Fill(tbl);

                foreach (var row in tbl.Rows)
                {
                    DataRow r = (DataRow)row;
                    Console.WriteLine(r[0].ToString());
                    Console.WriteLine(r[1].ToString());
                    Console.WriteLine(r[2].ToString());
                    Console.WriteLine(r[3].ToString());
                    Console.WriteLine(r[4].ToString());
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}