using System.Threading.Tasks;

namespace WebBrowser
{
    interface IWebService
    {
        Task<string> getStringAsync(WebAddress url);
    }
}