using System.Collections.Generic;

namespace MvcApplication1.Models
{
    public class ContentRepository
    {
        private static readonly List<PageData> _pages = new List<PageData>();
        
        public List<PageData> Pages {
            get { return _pages; }
        }

        public void SaveChanges()
        {
            
        }
    }
}