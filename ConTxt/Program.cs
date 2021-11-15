using System.Threading.Tasks;

namespace WebBrowser
{
    static class Program
    {
        static UserSettingsService userSettingsService;
        static ParserSettingsService parserSettingsService;
        static IConsoleService documentConsoleService;
        static IConsoleService userInputConsoleService;
        static IWebService webService;
        static IBrowser browser;
        static async Task Main(string[] args)
        {
            createServiceInstances(out userSettingsService, out parserSettingsService, out webService, out documentConsoleService, out userInputConsoleService);

            browser = new Browser(userSettingsService, parserSettingsService, webService, documentConsoleService, userInputConsoleService);
            await browser.mainUserInputLoopAsync();
        }
        static void createServiceInstances(out UserSettingsService userSettingsService, out ParserSettingsService parserSettingsService, out IWebService webService, out IConsoleService documentConsoleService, out IConsoleService userInputConsoleService){
            userSettingsService = new UserSettingsService("UserSettings.xml");
            parserSettingsService = new ParserSettingsService("ParserSettings.xml");
            documentConsoleService = new ConsoleService(userSettingsService);
            userInputConsoleService = new ConsoleService(userSettingsService);
            webService = new WebService(userSettingsService, documentConsoleService);
        }
    }
}
