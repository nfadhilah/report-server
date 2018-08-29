using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using ReportServer.Models;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ReportServer.Controllers
{
  public class ReportsController : Controller
  {
    private static readonly string ConStr = ConfigurationManager.ConnectionStrings["SirekasConnectionString"]
      .ConnectionString;

    private readonly SqlConnectionStringBuilder _builder = new SqlConnectionStringBuilder(ConStr);

    [HttpPost]
    public ActionResult GetReport(ParamReport paramReport)
    {

      var fileName = GetFileExt(paramReport);

      try
      {

        if (!ModelState.IsValid) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

        Task.Factory.StartNew(() => { ExportReport(paramReport); });

        var returnPath = $"Reports/tmp/{fileName}";

        return new ContentResult { Content = $"<a href=\"${returnPath}\">File</a>" };
      }
      catch (Exception ex)
      {
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, ex.InnerException?.Message ?? ex.Message);
      }
    }

    private void ExportReport(ParamReport paramReport)
    {
      var conInfo = new ConnectionInfo
      {
        ServerName = _builder.DataSource,
        DatabaseName = _builder.InitialCatalog,
        UserID = _builder.UserID,
        Password = _builder.Password
      };

      using (var rd = new ReportDocument())
      {
        rd.Load(Path.Combine(Server.MapPath("~/Reports"), paramReport.ReportName));

        rd.DataSourceConnections[0]
          .SetConnection(conInfo.ServerName, conInfo.DatabaseName, conInfo.UserID, conInfo.Password);

        if (paramReport.Parameters.Any())
        {
          foreach (var param in paramReport.Parameters)
          {
            rd.SetParameterValue(param.Key, param.Value);
          }
        }

        var formatType = GetExportFormatType(paramReport);

        var path = Path.Combine(Server.MapPath("~/Reports/tmp"), GetFileExt(paramReport));

        rd.ExportToDisk(formatType, path);
      }
    }

    private static ExportFormatType GetExportFormatType(ParamReport paramReport)
    {
      ExportFormatType formatType;

      switch (paramReport.FormatType)
      {
        case ReportType.Pdf:
          formatType = ExportFormatType.PortableDocFormat;
          break;
        case ReportType.Word:
          formatType = ExportFormatType.WordForWindows;
          break;
        case ReportType.Excel:
          formatType = ExportFormatType.Excel;
          break;
        default:
          formatType = ExportFormatType.PortableDocFormat;
          break;
      }

      return formatType;
    }

    private static string GetFileExt(ParamReport paramReport)
    {
      var filename = Guid.NewGuid().ToString();

      switch (paramReport.FormatType)
      {
        case ReportType.Pdf:
          filename += ".pdf";
          break;
        case ReportType.Word:
          filename += ".doc";
          break;
        case ReportType.Excel:
          filename += ".xls";
          break;
        default:
          filename += ".pdf";
          break;
      }

      return filename;
    }
  }
}
