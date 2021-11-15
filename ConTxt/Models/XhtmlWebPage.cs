using System.Threading.Tasks;

namespace WebBrowser
{
    class XhtmlWebPage : XhtmlDocument
    {
        // this class extends XhtmlDocument to the specific case where the page is from a web address (rather than a local file, like the welcome page)
        // the benefit of using this derived class is that the web address of the page can be used as a reference to 
        // expand shorthand href's in the page into full, valid web addresses, by using the current page's address to fill in the blanks
        // the more general base class does not have this abililty, and so it does not recognize shorthand links as navigable ones, preventing users from following them
        protected WebAddress pageAddress;
        protected XhtmlWebPage(string rawDocumentStringFromWeb, WebAddress pageAddressOnWeb, UserSettingsService UserSettingsService, ParserSettingsService ParserSettingsService, IConsoleService ConsoleService, IWebService WebService) : base(rawDocumentStringFromWeb, UserSettingsService, ParserSettingsService, ConsoleService, WebService){
            pageAddress = pageAddressOnWeb;
        } 
        public static async Task<XhtmlWebPage> createWebPageAsync(string rawDocumentStringFromWeb, WebAddress pageAddressOnWeb, UserSettingsService UserSettingsService, ParserSettingsService ParserSettingsService,  IConsoleService ConsoleService, IWebService WebService){

            XhtmlWebPage newInstance = new XhtmlWebPage(rawDocumentStringFromWeb, pageAddressOnWeb, UserSettingsService, ParserSettingsService, ConsoleService, WebService);

            await Task.Run(
                // this step is quite a lot of work, so do it on another thread
                () => newInstance.parseAndAnnotateDocument()
            );
            return newInstance;
        }
        protected override bool stringHrefIsValidWebAddress(string hrefAttribute){
            return WebAddress.createAddress(hrefAttribute, pageAddress) != null;
        }
    }
}