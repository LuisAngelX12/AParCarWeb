namespace AParCarWeb.Services
{
    using Microsoft.AspNetCore.Identity.UI.Services;
    public interface ITemplatedEmailSender : IEmailSender
    {
        Task SendTemplateAsync<TModel>(string email, EmailTemplate template, TModel model);
    }
}
