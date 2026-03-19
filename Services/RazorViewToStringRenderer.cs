namespace AParCarWeb.Services
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.Razor;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;

    public class RazorViewToStringRenderer : IRazorViewToStringRenderer
    {
        private readonly IRazorViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;

        public RazorViewToStringRenderer(
            IRazorViewEngine viewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider)
        {
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
        }

        public async Task<string> RenderAsync<TModel>(string viewPath, TModel model)
        {
            var httpContext = new DefaultHttpContext
            {
                RequestServices = _serviceProvider
            };

            var actionContext = new ActionContext(
                httpContext,
                new RouteData(),
                new ActionDescriptor());

            var viewResult = _viewEngine.GetView(
                null,
                viewPath,
                true);

            if (!viewResult.Success)
                throw new InvalidOperationException($"No se encontró {viewPath}");

            var viewData = new ViewDataDictionary<TModel>(
                new EmptyModelMetadataProvider(),
                new ModelStateDictionary())
            {
                Model = model
            };

            await using var sw = new StringWriter();

            var viewContext = new ViewContext(
                actionContext,
                viewResult.View,
                viewData,
                new TempDataDictionary(httpContext, _tempDataProvider),
                sw,
                new HtmlHelperOptions());

            await viewResult.View.RenderAsync(viewContext);
            return sw.ToString();
        }
    }
}
