using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcApplication1.Models.Domain
{
    public class UrlRouter<TRoutable>
    {
        public void RegisterRoute(string pattern, TRoutable routable)
        {
            throw new NotImplementedException();
        }

        public RouteMatch<TRoutable> MatchOrNull(Uri url)
        {
            throw new NotImplementedException();
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