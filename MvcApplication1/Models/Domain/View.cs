using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace MvcApplication1.Models
{
    public class View
    {
        private Guid                _id;
        private readonly ViewData   _data;

        public View(Guid id)
        {
            _id = id;
            _data = new ViewData { Id = _id };
        }

        public View(Guid id, string virtualPath, ViewStatus status, string routePattern)
        {
            _id = id;
            _data = new ViewData
            {
                Id = id,
                VirtualPath = virtualPath,
                Status = status,
                RoutePattern = routePattern
            };
        }

        public ViewData Data { get { return _data; } }
     
        public void     Publish() { }
        public void     Delete() { }
    }

    public class ViewData
    {
        public Guid         Id;
        public ViewStatus   Status;
        public string       RoutePattern;
        public string       VirtualPath;
    }

    public enum ViewStatus { Draft, Release }

    public class ViewDataRepository
    {
        private static readonly List<ViewData> _views = new List<ViewData>();
        
        public List<ViewData> Views {
            get { return _views; }
        }
    }
}