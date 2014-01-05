namespace MvcApplication1.Models
{
    public class CreatePageData
    {
        public string Name;
        public string RoutePattern;
        public string Markup;
    }

    public class UpdatePageData : CreatePageData { }
}