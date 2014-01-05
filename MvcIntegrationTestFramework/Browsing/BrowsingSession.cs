using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;
using MvcApplication4.Tests.IntegrationTestFramework;
using MvcIntegrationTestFramework.Interception;

namespace MvcIntegrationTestFramework.Browsing
{
    public class BrowsingSession
    {
        public HttpSessionState Session { get; private set; }
        public HttpCookieCollection Cookies { get; private set; }

        private static BrowsingSession _instance;
        private static object __lock = new object();

        private BrowsingSession()
        {
            Cookies = new HttpCookieCollection();
        }

        public static BrowsingSession Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (__lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new BrowsingSession();
                        }
                    }
                }
                return _instance;
            }
        }

        public SerializableRequestResult Get(string url)
        {
            return Get(new Uri(url));
        }

        public SerializableRequestResult Get(Uri url)
        {
            return ProcessRequest(url.AbsolutePath, HttpVerbs.Get, new NameValueCollection());
        }

        public SerializableRequestResult Post(string url)
        {
            var uri = new Uri(url);
            return ProcessRequest(uri.AbsolutePath, HttpVerbs.Post, new NameValueCollection());
        }

        /// <summary>
        /// Sends a post to your url. Url should NOT start with a /
        /// </summary>
        /// <param name="url"></param>
        /// <param name="formData"></param>
        /// <example>
        /// <code>
        /// var result = Post("registration/create", new
        /// {
        ///     Form = new
        ///     {
        ///         InvoiceNumber = "10000",
        ///         AmountDue = "10.00",
        ///         Email = "chriso@innovsys.com",
        ///         Password = "welcome",
        ///         ConfirmPassword = "welcome"
        ///     }
        /// });
        /// </code>
        /// </example>
        public SerializableRequestResult Post(string url, object formData)
        {
            var formNameValueCollection = NameValueCollectionConversions.ConvertFromObject(formData);
            return ProcessRequest(url, HttpVerbs.Post, formNameValueCollection);
        }

        public SerializableRequestResult PostXml(string url, string xmlData)
        {
            xmlData = XmlHelper.Trim(xmlData);
            var formNameValueCollection = NameValueCollectionConversions.ConvertFromObject(new { xmlData });
            return ProcessRequest(url, HttpVerbs.Post, formNameValueCollection);
        }

        /// <param name="cookies">will override browsingSession cookies </param>
        public SerializableRequestResult Post(string url, object formData, string cookies)
        {
            this.Cookies = CookiesFromString(cookies);
            var formNameValueCollection = NameValueCollectionConversions.ConvertFromObject(formData);
            return ProcessRequest(url, HttpVerbs.Post, formNameValueCollection);
        }

        HttpCookieCollection CookiesFromString(string cookiesString)
        {
            var dest = new HttpCookieCollection();
            foreach (var pair in cookiesString.Split(' '))
            {
                if(string.IsNullOrWhiteSpace(pair)) continue;

                string[] cookies = pair.Split('=');
                dest.Add(new HttpCookie(cookies[0], cookies[1]));
            }
            return dest;
        }

        public SerializableRequestResult Post(string url, NameValueCollection formNameValueCollection)
        {
            return ProcessRequest(url, HttpVerbs.Post, formNameValueCollection);
        }

        private SerializableRequestResult ProcessRequest(string url, HttpVerbs httpVerb = HttpVerbs.Get, NameValueCollection formValues = null)
        {
            return ProcessRequest(url, httpVerb, formValues, null);
        }

        private SerializableRequestResult ProcessRequest(string url, HttpVerbs httpVerb, NameValueCollection formValues, NameValueCollection headers)
        {
            if (url == null) throw new ArgumentNullException("url");

            // Fix up URLs that incorrectly start with / or ~/
            if (url.StartsWith("~/"))
                url = url.Substring(2);
            else if(url.StartsWith("/"))
                url = url.Substring(1);

            // Parse out the querystring if provided
            string query = "";
            int querySeparatorIndex = url.IndexOf("?");
            if (querySeparatorIndex >= 0) {
                query = url.Substring(querySeparatorIndex + 1);
                url = url.Substring(0, querySeparatorIndex);
            }                

            // Perform the request
            LastRequestData.Reset();
            var output = new StringWriter();
            string httpVerbName = httpVerb.ToString().ToLower();
            var workerRequest = new SimulatedWorkerRequest(url, query, output, Cookies, httpVerbName, formValues, headers);
            HttpRuntime.ProcessRequest(workerRequest);

            // Capture the output
            AddAnyNewCookiesToCookieCollection();
            Session = LastRequestData.HttpSessionState;

            var cookiesString = string.Empty;
            if(LastRequestData.Response != null) cookiesString = CookiesToString(LastRequestData.Response.Cookies);

            var result =  new RequestResult
            {
                ResponseText = output.ToString(),
                ActionExecutedContext = LastRequestData.ActionExecutedContext,
                ResultExecutedContext = LastRequestData.ResultExecutedContext,
                Response = LastRequestData.Response,
                SerializableRequestResult = new SerializableRequestResult()
                {
                    CookiesString = cookiesString, 
                    ResponseText = output.ToString(), 
                    ResponseStatusCode = LastRequestData.Response != null ? LastRequestData.Response.StatusCode : 500
                }
            };
            return result.SerializableRequestResult;
        }

        string CookiesToString(HttpCookieCollection cookies)
        {
            var sb = new StringBuilder();
            foreach (var key in cookies.AllKeys)
            {
                var cookie = cookies[key];
                sb.AppendFormat("{0}={1} ", cookie.Name, cookie.Value);
            }
            return sb.ToString();
        }

        private void AddAnyNewCookiesToCookieCollection()
        {
            if(LastRequestData.Response == null)
                return;

            HttpCookieCollection lastResponseCookies = LastRequestData.Response.Cookies;
            if(lastResponseCookies == null)
                return;

            foreach (string cookieName in lastResponseCookies) {
                HttpCookie cookie = lastResponseCookies[cookieName];
                if (Cookies[cookieName] != null)
                    Cookies.Remove(cookieName);
                if((cookie.Expires == default(DateTime)) || (cookie.Expires > DateTime.Now))
                    Cookies.Add(cookie);
            }
        }
    }
}