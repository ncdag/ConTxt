using System.Threading.Tasks;

namespace WebBrowser
{
    interface IBrowser
    {
        Task navigateToInternalPageAsync(FilePath filePath);
        Task navigateToAsync(WebAddress destination);
        Task searchAsync(string keywords);
        Task mainUserInputLoopAsync();
    }
}