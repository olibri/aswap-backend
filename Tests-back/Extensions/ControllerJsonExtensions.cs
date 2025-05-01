using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Tests_back.Extensions;

public static class ControllerJsonExtensions
{
    public static void SetJsonBody(this Controller controller, string json)
    {
        var bytes = Encoding.UTF8.GetBytes(json);
        var stream = new MemoryStream(bytes);
        stream.Position = 0;

        var ctx = new DefaultHttpContext();
        ctx.Request.Body = stream;
        ctx.Request.ContentType = "application/json";
        ctx.Request.ContentLength = bytes.Length;

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = ctx
        };
    }
}