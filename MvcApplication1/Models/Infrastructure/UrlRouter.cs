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

        public void RegisterRouteIfNotRegistered(string pattern, TRoutable routable)
        {
            if (_routables.ContainsValue(routable)) return;
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

        public bool IsRegistered(TRoutable routable)
        {
            return _routables.ContainsValue(routable);
        }

        public void RemoveRoutableIfRegistered(TRoutable routable)
        {
            if (!IsRegistered(routable)) return;
            var pair = _routables.Single(x => x.Value.Equals(routable));
            _routables.Remove(pair.Key);
        }

        public void UnregisterAll()
        {
            _routables.Clear();
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