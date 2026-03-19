namespace AParCarWeb.Services
{
    public interface IRazorViewToStringRenderer
    {
        Task<string> RenderAsync<TModel>(string viewPath, TModel model);
    }
}
