using System.IO;
using System.Text;
using System.Web.Mvc;

namespace MvcApplication1.Models
{
    public class MvcRequestContext
    {
        private readonly ViewDataDictionary _viewData;
        private readonly TempDataDictionary _tempData;
        private readonly ControllerContext _controllerContext;

        //just for testing
        protected MvcRequestContext() {}

        public MvcRequestContext(ControllerContext controllerContext, ViewDataDictionary viewData, TempDataDictionary tempData)
        {
            _controllerContext = controllerContext;
            _viewData = viewData;
            _tempData = tempData;
        }

        public virtual StringBuilder RenderRazorViewToString(string viewFilePath, object model)
        {
            _viewData.Model = model;
            using (var sw = new StringWriter())
            {
                ViewEngineResult viewResult;

                viewResult = ViewEngines.Engines.FindPartialView(_controllerContext, viewFilePath);

                if (viewResult.View == null)
                    return null;

                var viewContext = new ViewContext(_controllerContext, viewResult.View, _viewData, _tempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(_controllerContext, viewResult.View);
                return sw.GetStringBuilder();
            }
        }

        public virtual bool HasCookie(string previewCookieName)
        {
            throw new System.NotImplementedException();
        }
    }
}