using System;
using System.Web;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WebBrowser
{
    class Browser : IBrowser
    { 
        UserSettingsService userSettingsService;
        ParserSettingsService parserSettingsService;
        IWebService webService;
        IConsoleService documentConsoleService;
        IConsoleService userInputConsoleService;
        WebAddress currentPageAddress;
        XhtmlDocument currentPage;
        Stack<WebAddress> addressHistory = new Stack<WebAddress>();
        Stack<WebAddress> addressFuture = new Stack<WebAddress>();
        bool userWantsToExitProgram = false;
        string lastUserInput = null;
        WebAddress externalRedirectAwaitingApproval;
        WebAddress externalLinkAwaitingApproval;
        HashSet<string> trustedOrigins = new HashSet<string>();
        string searchSubmitUrlQueryArugment = @"q";
        // lookahead matches the meta tag opening, [^<>’”\\s]+ the body, and lookbehind the meta tag close
        Regex metaRedirectPattern = new Regex("(?<=(<meta[\\s]http-equiv=['\"]refresh['\"][\\s]content=['\"][0-9]+;url=))[^<>’”\\s]+(?=(['\"]>))");
        public Browser(UserSettingsService SettingsService, ParserSettingsService ParserSettingsService, IWebService WebService, IConsoleService DocumentConsoleService, IConsoleService UserInputConsoleService){
            
            webService = WebService;
            userSettingsService = SettingsService;
            parserSettingsService = ParserSettingsService;
            documentConsoleService = DocumentConsoleService;
            userInputConsoleService = UserInputConsoleService;  // use a seperate console service for user input, since we don't
                                                                // want the level of indentation of user input messages to mess
                                                                // up that of the document
                                                                
            documentConsoleService.updateWindowTitle();
            

            currentPageAddress = WebAddress.createAddress(userSettingsService.searchBaseUrl);

            foreach (string originString in userSettingsService.trustedOriginsStringList){

                documentConsoleService.printDebug($"Loaded origin: ${originString}");

                try{
                    trustOrigin(
                    WebAddress.createAddress(originString)
                    );
                }
                catch{
                    documentConsoleService.printWarning($"Failed to trust origin: ${originString}");
                }
            }
        }
        private WebAddress buildSearchUrlFromKeywords(string searchKeywords){
            return WebAddress.createAddress(
                $"{userSettingsService.searchBaseUrl}?{searchSubmitUrlQueryArugment}={HttpUtility.UrlEncode(searchKeywords)}"
            );
        }
        private bool isOnTrustedOrigin(WebAddress address){
            return trustedOrigins.Contains(address.origin);
        }
        private void trustOrigin(WebAddress address){
            trustOrigin(address.origin);
        }
        private void trustOrigin(string origin){
            trustedOrigins.Add(origin);
            documentConsoleService.printDebug($"Origin ${origin} has been added to the trusted origins list.");
        }
        private async Task prompForRedirectApprovalAsync(WebAddress addressNeedingApproval){
            externalRedirectAwaitingApproval = addressNeedingApproval;

            // Console.Clear();
            documentConsoleService.newLine();
            documentConsoleService.horizontalRuler();
            await printWholeHtmlFragmentFromFragmentsFolderAsync("RedirectPrompt.html");
            documentConsoleService.newLine(2);
            documentConsoleService.printUriOriginCenter(externalRedirectAwaitingApproval);
            documentConsoleService.newLine(2);
            await printWholeHtmlFragmentFromFragmentsFolderAsync("NewSiteConfirm.html");
            documentConsoleService.horizontalRuler();
            documentConsoleService.newLine();
        }
        private async Task prompForExternalNavigationApprovalAsync(WebAddress addressNeedingApproval){
            externalLinkAwaitingApproval = addressNeedingApproval;

            // Console.Clear();
            documentConsoleService.newLine();
            documentConsoleService.horizontalRuler();
            await printWholeHtmlFragmentFromFragmentsFolderAsync("NewSitePrompt.html");
            documentConsoleService.newLine(2);
            documentConsoleService.printUriOriginCenter(externalLinkAwaitingApproval);
            documentConsoleService.newLine(2);
            await printWholeHtmlFragmentFromFragmentsFolderAsync("NewSiteConfirm.html");
            documentConsoleService.horizontalRuler();
            documentConsoleService.newLine();
        }
        public async Task navigateToInternalPageAsync(FilePath filePath){
            string fileAsString = await filePath.getTextOfResidentFileAsync();
            currentPage = await XhtmlDocument.createPageAsync(fileAsString, userSettingsService, parserSettingsService, documentConsoleService, webService); 

            documentConsoleService.horizontalRuler();
            documentConsoleService.prettyPrintCenter(
                currentPage.title, ConsoleColor.Gray
            );
            documentConsoleService.horizontalRuler();
            
            currentPage.readNextSectionOfPageToConsole(0.7);
        }
        private async Task printWholeHtmlFragmentFromFragmentsFolderAsync(string fileName){
            await printWholeHtmlFragmentAsync(
                FilePath.createFilePath(new List<string>{
                    "Assets", "Fragments", fileName
                })
            );
        }
        private async Task printWholeHtmlFragmentAsync(FilePath filePath){
            string htmlFragment = await filePath.getTextOfResidentFileAsync();
            await printWholeHtmlFragmentAsync(htmlFragment);
        }
        private async Task printWholeHtmlFragmentAsync(string htmlFragmentMarkup){
            XhtmlDocument fragmentAsPage = await XhtmlDocument.createPageAsync(htmlFragmentMarkup, userSettingsService, parserSettingsService, documentConsoleService, webService); 
            fragmentAsPage.readWholePageToConsole();
        }
        public async Task navigateToAsync(WebAddress destination){

            Console.Clear();

            if (!isOnTrustedOrigin(destination)){
                await prompForExternalNavigationApprovalAsync(destination);
            }
            else{

                string documentAsString = await webService.getStringAsync(destination);

                WebAddress metaRedirectUrl = getMetaRedirectUrl(documentAsString);
                bool pageHasRedirectPresent = (metaRedirectUrl != null);
                
                if (pageHasRedirectPresent && !isOnTrustedOrigin(metaRedirectUrl)){
                    await prompForRedirectApprovalAsync(metaRedirectUrl);
                }
                else if (pageHasRedirectPresent && isOnTrustedOrigin(metaRedirectUrl)){
                    await navigateToAsync(metaRedirectUrl);
                }
                else if (!pageHasRedirectPresent){
                    currentPageAddress = destination;
                    addressHistory.Push(destination);

                    currentPage = await XhtmlWebPage.createWebPageAsync(documentAsString, destination, userSettingsService, parserSettingsService, documentConsoleService, webService); 

                    // documentConsoleService.horizontalRuler();
                    documentConsoleService.newLine(2);
                    documentConsoleService.prettyPrintInQuotesCenter(
                        currentPage.title, ConsoleColor.Cyan,
                        "\"", ConsoleColor.DarkGray
                    );
                    documentConsoleService.printUriCenter(destination);
                    documentConsoleService.horizontalRuler();

                    documentConsoleService.updateWindowTitle(currentPage.title, destination.origin);
                    
                    currentPage.readNextSectionOfPageToConsole(0.7);
                }
            }
        }

        private WebAddress getMetaRedirectUrl(string documentAsString){
            Match redirectingMetaTagMatch = metaRedirectPattern.Match(documentAsString.ToLower());
            return WebAddress.createAddress(
                redirectingMetaTagMatch.Value
            );
        }

        private async Task showHelp(){
            await navigateToInternalPageAsync(
                FilePath.createFilePath(new List<string>{
                    "Assets","Pages","Welcome.html"
                })
            );
        }
        public async Task searchAsync(string keywords){
            await navigateToAsync(
                buildSearchUrlFromKeywords(
                    keywords
                )
            );
        }

        public async Task mainUserInputLoopAsync(){
            await showHelp();
            while (!userWantsToExitProgram)
            {
                await userInputReader();
            }
        }
        private async Task userInputReader(){

            lastUserInput = userInputConsoleService.
                getLineOfUserInputAndMoveBackTheCursorBackToWhereItWas();

            string lastUserInputLower = lastUserInput.ToLower();
            string lastUserInputUpper = lastUserInput.ToUpper();

            if ((currentPage != null) && currentPage.linkHexIdNumberToUrl.ContainsKey(lastUserInputUpper)){
                
                Console.Clear();
                WebAddress targetAddress = WebAddress.createAddress(
                    currentPage.linkHexIdNumberToUrl[lastUserInputUpper],
                    currentPageAddress
                );

                addressFuture.Clear();

                await navigateToAsync(targetAddress);
            }

            else if (lastUserInput == "" && currentPage != null){
                currentPage.readNextSectionOfPageToConsole();
            }

            else if (lastUserInputLower == "exit" || lastUserInputLower == "bye"){
                userWantsToExitProgram = true;
            }

            else if (lastUserInputLower == "help" || lastUserInputLower == "h"){
                await showHelp();
            }

            else if (lastUserInputLower == "reload"){
                userSettingsService.reloadSettings();
            }

            else if (lastUserInputLower == "back" || lastUserInputLower == "<"){
                
                try{
                    addressFuture.Push(addressHistory.Pop());
                    // Console.Clear();
                    await navigateToAsync(addressHistory.Pop());
                }
                catch{
                    documentConsoleService.newLine();
                    documentConsoleService.horizontalRuler();
                    await printWholeHtmlFragmentFromFragmentsFolderAsync("CantGoBackward.html");
                    documentConsoleService.horizontalRuler();
                    documentConsoleService.newLine();
                }
            }

            else if (lastUserInputLower == "forward" || lastUserInputLower == ">"){
                
                try{
                    // Console.Clear();
                    await navigateToAsync(addressFuture.Pop());
                }
                catch{
                    documentConsoleService.newLine();
                    documentConsoleService.horizontalRuler();
                    await printWholeHtmlFragmentFromFragmentsFolderAsync("CantGoForward.html");
                    documentConsoleService.horizontalRuler();
                    documentConsoleService.newLine();
                }
            }

            else if (externalRedirectAwaitingApproval != null){
                if (lastUserInputLower == "yes" || lastUserInputLower == "y"){
                    // Console.Clear();
                    trustOrigin(externalRedirectAwaitingApproval);
                    await navigateToAsync(externalRedirectAwaitingApproval);
                    externalRedirectAwaitingApproval = null;
                }
            }

            else if (externalLinkAwaitingApproval != null){
                if (lastUserInputLower == "yes" || lastUserInputLower == "y"){
                    // Console.Clear();
                    trustOrigin(externalLinkAwaitingApproval);
                    await navigateToAsync(externalLinkAwaitingApproval);
                    externalLinkAwaitingApproval = null;
                }
            }

            else{
                // Console.Clear();
                addressFuture.Clear();
                await searchAsync(lastUserInput);
            }
        }
    }
}