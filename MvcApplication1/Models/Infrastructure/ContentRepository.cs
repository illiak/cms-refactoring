using System.Collections.Generic;

namespace MvcApplication1.Models
{
    public class ContentRepository
    {
        private static readonly List<Page> _pages = new List<Page>();
        
        public List<Page> Pages {
            get { return _pages; }
        }

        public void SaveChanges()
        {
            
        }
    }
}