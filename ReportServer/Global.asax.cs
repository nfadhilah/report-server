using System;
using System.Threading;
using System.Web.Mvc;
using System.Web.Routing;

namespace ReportServer
{
  public class MvcApplication : System.Web.HttpApplication
  {
    public static DateTime LastCheck = DateTime.Now;
    protected void Application_Start()
    {
      AreaRegistration.RegisterAllAreas();
      FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
      RouteConfig.RegisterRoutes(RouteTable.Routes);

      System.Threading.Thread t = new Thread(
        () =>
        {
          while (true)
          {
            Thread.Sleep(1000 * 60 * 60);
            DateTime now = DateTime.Now;

            if (now.Date != LastCheck.Date)
            {
              LastCheck = now;
              //lakukan pembersihan file report
            }

          }
        }
        );

    }
  }
}
