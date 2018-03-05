using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using ReportServer.Models;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using WebGrease.Css.Extensions;
using Table = CrystalDecisions.CrystalReports.Engine.Table;

namespace ReportServer.Controllers
{
  public class ReportsController : Controller
  {
    private static readonly string ConStr = ConfigurationManager.ConnectionStrings["SirekasConnectionString"]
      .ConnectionString;

    private static readonly SqlConnectionStringBuilder Builder = new SqlConnectionStringBuilder(ConStr);

    [HttpPost]
    public ActionResult GetReport(ParamReport paramReport)
    {
      try
      {
        if (!ModelState.IsValid) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

        var conInfo = new ConnectionInfo
        {
          ServerName = Builder.DataSource,
          DatabaseName = Builder.InitialCatalog,
          UserID = Builder.UserID,
          Password = Builder.Password
        };

        var rd = new ReportDocument();

        rd.Load(Path.Combine(Server.MapPath("~/Reports"), paramReport.ReportName));

        var crDatabase = rd.Database;
        var crTables = crDatabase.Tables;
        TableLogOnInfo logOnInfo;

        foreach (Table crTable in crTables)
        {
          logOnInfo = crTable.LogOnInfo;
          logOnInfo.ConnectionInfo = conInfo;
          crTable.ApplyLogOnInfo(logOnInfo);
        }

        for (var i = 0; i < rd.Subreports.Count; i++)
        {
          foreach (Table crTbl in rd.Subreports[i].Database.Tables)
          {
            logOnInfo = crTbl.LogOnInfo;
            logOnInfo.ConnectionInfo = conInfo;
            crTbl.ApplyLogOnInfo(logOnInfo);
          }

          rd.Subreports[i].VerifyDatabase();
        }

        if (paramReport.Parameters.Any())
        {
          paramReport.Parameters.ForEach(p =>
          {
            rd.SetParameterValue(p.Key, p.Value);
          });
        }

        rd.VerifyDatabase();

        Response.Buffer = false;
        Response.ClearContent();
        Response.ClearHeaders();

        ExportFormatType streamFormat;

        switch (paramReport.FormatType)
        {
          case ReportType.Pdf:
            streamFormat = ExportFormatType.PortableDocFormat;
            break;
          case ReportType.Word:
            streamFormat = ExportFormatType.WordForWindows;
            break;
          case ReportType.Excel:
            streamFormat = ExportFormatType.Excel;
            break;
          default:
            streamFormat = ExportFormatType.PortableDocFormat;
            break;
        }

        var stream = rd.ExportToStream(streamFormat);

        stream.Seek(0, SeekOrigin.Begin);

        return new FileStreamResult(stream, "application/pdf");
      }
      catch (Exception ex)
      {
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, ex.Message);
      }
    }
  }
}
