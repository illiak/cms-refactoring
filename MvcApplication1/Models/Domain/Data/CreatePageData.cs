namespace MvcApplication1.Models
{
    public class CreatePageData
    {
        public string Name;
        public string Route;
        public string Markup;
    }

    public class UpdatePageData : CreatePageData { }
}