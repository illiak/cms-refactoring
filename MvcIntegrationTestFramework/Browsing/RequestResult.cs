using System;
using System.Web;
using System.Web.Mvc;

namespace MvcIntegrationTestFramework.Browsing
{
    /// <summary>
    /// Represents the result of a simulated request
    /// </summary>
    public class RequestResult
    {
        public HttpResponse Response { get; set; }
        public string ResponseText { get; set; }
        public ActionExecutedContext ActionExecutedContext { get; set; }
        public ResultExecutedContext ResultExecutedContext { get; set; }

        public string CookiesString { get; set; }

        public SerializableRequestResult SerializableRequestResult { get; set; }
    }

    [Serializable]
    public class SerializableRequestResult
    {
        public string   ResponseText { get; set; }
        public string   CookiesString { get; set; }
        public int      ResponseStatusCode { get; set; }
    }
}