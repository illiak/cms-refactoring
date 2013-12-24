using System;

namespace MvcApplication1.Models
{
    public class Page
    {
        Guid _id;

        public Page()
        {
            
        }

        public PageData GetData()
        {
            throw new NotImplementedException();
        }
        public void     Publish() { }
        public void     Unpublish() { }
        public void     Delete() { }
    }

    public class PageData
    {
        public Guid         Id;
        public string       Name;
        public string       Language;
        public PageStatus   Status;
        public string       Url;
        public string       Filepath;
    }

    public enum PageStatus { Draft, Published, Unpublished }
}