using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace WebBrowser
{
    class WebService : IWebService
    {
        UserSettingsService settingsService;
        IConsoleService consoleService;
        private WebClient client;
        private bool downloadInProgress = false;
        private event Action downloadStarted;
        private event Action downloadComplete;
        Timer incrementProgressBarToShowResponsiveness;
        public WebService(UserSettingsService SettingsService, IConsoleService ConsoleService){
            settingsService = SettingsService;
            consoleService = ConsoleService;
            client = new WebClient();
            initializeWebClient();
        }
        private void initializeWebClient(){
            client.Headers.Add("Accept-Language", settingsService.contentLanguage);
            consoleService.printDebug($"Request Headers: {client.Headers.ToString()}");

            client.DownloadProgressChanged += onDownloadProgressChange;
            downloadStarted += onDownloadStart;
            downloadComplete += onDownloadComplete;
        }
        public async Task<string> getStringAsync(WebAddress url){

            Stopwatch watch = new Stopwatch();
            watch.Start();

            consoleService.printDebug($"URL: {url}");

            downloadStarted();
            Byte[] response = await getBytesAndCatchException(url);
            downloadComplete();

            string responseBody;

            if(response == null){
                
                responseBody = await FilePath.createFilePath(
                    new List<string>{"Assets", "Pages", "NoInternet.html"}
                ).getTextOfResidentFileAsync();
            }
            else{
                responseBody = Encoding.UTF8.GetString(response);
            }

            consoleService.printTrace(
                $"GET request round trip time: {watch.Elapsed.ToString()}"
            );

            return responseBody;
        }
        private async Task<Byte[]> getBytesAndCatchException(WebAddress url){
            try{
                return await client.DownloadDataTaskAsync(url);
            }
            catch (Exception ex){
                consoleService.printWarning("Unable to connect to the requested site: ");
                consoleService.printWarning($"Please check your internet connection | Exception: {ex}");
            }
            return null;
        }
        void onDownloadProgressChange(object sender, DownloadProgressChangedEventArgs args){
            if (downloadInProgress){
                consoleService.updateProgressBar(
                    args.ProgressPercentage
                );
            }
            
        }
        void onDownloadStart(){
            downloadInProgress = true;
            consoleService.beginProgressBar();

            incrementProgressBarToShowResponsiveness = new Timer(
                (Object argument) => {consoleService.incrementProgressBar(1);},
                null,
                300,
                700
            );
        }
        void onDownloadComplete(){
            downloadInProgress = false;
            consoleService.closeProgressBar();
            incrementProgressBarToShowResponsiveness.Dispose();
        }
    }
}