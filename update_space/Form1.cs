using MySql.Data.MySqlClient;
using MySqlX.XDevAPI;
using Newtonsoft.Json;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Net.Http;
using System.Windows.Forms;


namespace update_space
{
    public partial class 更新車位數 : Form
    {
        public 更新車位數()
        {
            InitializeComponent();

        }

        public int Id = 0;
        public int total = 0;
        public int now = 0;


        public class LotData
        {
            public int id { get; set; }
            public int total_lots { get; set; }
            public int available_lots { get; set; }
            public int moto_total_lots { get; set; }
            public int? moto_available_lots { get; set; }
        }

        private void UpdateLotsData(int Id, int total, int now)
        {
            // 更新屬性值
            lotsData[0].id = Id;
            lotsData[0].total_lots = total;
            lotsData[0].available_lots = now;
        }


        clsSetting.AppSettings Parm = clsSetting.LoadSettings("Setting.xml");
        private LotData[] lotsData;

        private RequestData requestData;

        public class RequestData
        {
            public LotData[] lots_data { get; set; }
        }

        public string url = "https://partner-api.parkinglotapp.com/v1/parkinglots/bulk";
        public string token = "秘密";
        public string connectionString = "server=127.0.0.1;port=3306;uid=root;password=root;database=aps4;";
        public async void bulk()
        {

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = "SELECT * FROM space";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                total = int.Parse(reader["MAX"].ToString());
                                now = int.Parse(reader["AVAILABLE"].ToString());
                            }
                            reader.Close();
                        }
                        command.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                connection.Close();
                connection.Dispose();

            }



            UpdateLotsData(Id, total, now);

            UpdateRequestData(lotsData);

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                    var json = JsonConvert.SerializeObject(requestData);
                    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                    using (HttpResponseMessage response = await client.PostAsync(url, content))
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        label1.Text = "更新成功 " + DateTime.Now.ToString();
                        response.Dispose();
                    }
                    
                    client.Dispose();

                }
            }
            catch (Exception ex)
            {
                label1.Text = "更新失敗 \r\n" + ex.ToString();
            }



        }

        private void UpdateRequestData(LotData[] updatedLotsData)
        {
            // 更新 lots_data 屬性的值
            requestData.lots_data = updatedLotsData;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            timer1.Enabled = false;

            bulk();

            timer1.Enabled = true;

        }

        private void 更新車位數_Load(object sender, EventArgs e)
        {

            Id = Parm.ID;

            lotsData = new[]
            {
               new LotData
               {
                   id = 0,  // 初始值為 0 或其他預設值
                   total_lots = 0,
                   available_lots = 0,
                   moto_total_lots = 0,
                   moto_available_lots = null
               }
            };

            requestData = new RequestData
            {
                lots_data = new LotData[0]
            };

            timer1 = new Timer();
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Interval = 3000; // 設定每秒讀取一次資料
            timer1.Start();

        }
    }
}
