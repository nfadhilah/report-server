using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ReportServer.Models
{
  public class ParamReport
  {
    [Required]
    public string ReportName { get; set; }
    [Required]
    public ReportType FormatType { get; set; }
    public Dictionary<string, object> Parameters { get; set; }
  }
}
