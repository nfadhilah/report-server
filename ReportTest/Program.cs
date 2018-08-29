using System.Net.Http;
using System.Text;

namespace ReportTest
{
  class Program
  {
    static void Main(string[] args)
    {
      var content =
        "{\r\n\t\"reportName\": \"APBDRINC.rpt\",\r\n\t\"formatType\": 1,\r\n\t\"parameters\": {\r\n\t\t\"@unitkey\": \"4_\",\r\n\t\t\"@kdtahap\": \"2\",\r\n\t\t\"@kdlevel\": 5,\r\n\t\t\"@tanggal\": null,\r\n\t\t\"@hal\": 1\r\n\t}\r\n}";

      for (int i = 0; i < 2; i++)
      {
        //Thread t = new Thread((() =>
        //{

        var httpClient = new HttpClient();
        var cp = new StringContent(content, Encoding.UTF8, "application/json");
        var response = httpClient.PostAsync("http://localhost:5001/reports/getreport",
              cp).Result;

        //}));
        //t.IsBackground = true;
        //t.Start();
      }

    }
  }
}
