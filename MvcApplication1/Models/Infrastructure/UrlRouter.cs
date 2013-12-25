using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcApplication1.Models.Domain
{
    public class UrlRouter<TRoutable>
    {
        private readonly Dictionary<string, TRoutable> _routables = new Dictionary<string,TRoutable>(); 

        public void RegisterRoute(string pattern, TRoutable routable)
        {
           _routables.Add(pattern, routable);
        }

        public RouteMatch<TRoutable> MatchOrNull(Uri url)
        {
            //exact match implementation only
            var urlString = url.ToString();
            if (!_routables.ContainsKey(urlString)) return null;
            var routable = _routables[urlString];

            return new RouteMatch<TRoutable> { IsExactMatch = true, Pattern = urlString, Routable = routable, Score = 1000};
        }
    }

    public class RouteMatch<TRoutable>
    {
        public TRoutable Routable;
        public string Pattern;
        public int Score;
        public bool IsExactMatch;
        public dynamic Data;
        public IEnumerable<RouteMatch<TRoutable>> OtherMatches;
    }
}