using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using IMS.Models;
public class ActionsAttribute : ActionFilterAttribute
{
    Details logClass=new Details();
    Stopwatch? watch;
    public override void OnActionExecuted(ActionExecutedContext filterContext)
    {
        watch?.Stop();

        Action("OnActionExecuted", filterContext.RouteData);
        // filterContext.HttpContext.Response.WriteAsync("Action Time"+

    }
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        Action("OnActionExecuting", filterContext.RouteData);
        watch = Stopwatch.StartNew();
    }
    public override void OnResultExecuted(ResultExecutedContext filterContext)
    {
        Action("OnResultExecuted", filterContext.RouteData);
    }
    public override void OnResultExecuting(ResultExecutingContext filterContext)
    {
        Action("OnResultExecuting", filterContext.RouteData);
    }
    private void Action(string methodName, RouteData routeData)
    {
        var controllerName = routeData.Values["controller"];
        var actionName = routeData.Values["action"];
        var message = methodName + " -Controller:" + controllerName + ", Action:" + actionName + ", Time :" + watch?.ElapsedMilliseconds.ToString() + "\n";
        logClass.WriteIntoLog(message);
    }

}

public class ExceptionFilter : ActionFilterAttribute, IExceptionFilter
{
    Details logClass=new Details();
    public void OnException(ExceptionContext context)
    {
        Exception exception = context.Exception;
       logClass.WriteIntoLog("Exception : "+exception.Message);
       // Console.WriteLine(exception);
        context.ExceptionHandled = true;
        context.Result = new ViewResult()
        {
            ViewName = "Exception"
        };
    }
}