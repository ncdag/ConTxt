using System;
using System.Web;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace WebBrowser
{
    class ConsoleService : IConsoleService
    {
        private static object locker = new Object();
        private static HashSet<string> tagsToBreakLineBeforeAndAfter = new HashSet<string>{
            "h1",
            "h2",
            "h3",
            "h4",
            "h5",
            "h6",
            "li",
            "p",
            "br",
            "tr",
            "ol",
            "img"
        };
        private UserSettingsService userSettingsService;
        private bool progressBarIsActive = false;
        private Stopwatch progressBarStopwatch = new Stopwatch();
        private int charactersUsedThisLine = 0;
        private char characterToBreakLinesOn = ' ';
        private string userInputIndicator = ">>>";
        private string userInputPlaceholder = "&#8230;"; // html entity for the single character version of "..."
        private string titleSpacingString = "   |   ";
        private string applicationNameString = "ConTxt";
        private ConsoleColor userInputColor = ConsoleColor.Yellow;
        private Regex newLineRegex = new Regex("[\r\n]+");
        public ConsoleService(UserSettingsService SettingsService){
            Console.CursorVisible = false;
            userSettingsService = SettingsService;
        }
        public void updateWindowTitle(){
            Console.Title = applicationNameString;
        }
        public void updateWindowTitle(string pageName, string pageOrigin){
            string titleInProgress = applicationNameString;

            if (pageName != null && pageName != ""){
                titleInProgress += titleSpacingString + pageName;
            }
            if (pageOrigin != null && pageOrigin != ""){
                titleInProgress += titleSpacingString + pageOrigin;
            }
            Console.Title = titleInProgress;
        }
        public void getCursorPosition(out int savedConsoleCursorX, out int savedConsoleCursorY ){
            savedConsoleCursorX = Console.CursorLeft;
            savedConsoleCursorY = Console.CursorTop;
        }
        public void setCursorPosition(int newX, int newY){

            if (newX >= 0 && newY >= 0 && newX <= Console.WindowWidth && newY <= Console.WindowHeight){
                Console.CursorLeft = newX;
                Console.CursorTop = newY;
            }
            else{
                printError($"Invalid console position to restore: [{newX},{newY}]. Range is [0,0] to [{Console.WindowWidth},{Console.WindowHeight}]");
            }
        }
        public string getLineOfUserInputAndMoveBackTheCursorBackToWhereItWas(){
            // readline is wrapped in a backup-restore of the cursor position
            // to prevent the user's input from staying on the console. The 
            // restore of position allows the program's next write to
            // overwrite the string provided by the user

            int initialX;
            int initialY;
            getCursorPosition(out initialX, out initialY);

            newLine();

            prettyPrint(userInputIndicator, userInputColor);

            Console.ForegroundColor = userInputColor;

            int postIndicatorX;
            int postIndicatorY;
            getCursorPosition(out postIndicatorX, out postIndicatorY);

            prettyPrint(userInputPlaceholder, ConsoleColor.DarkGray);
            Console.ForegroundColor = userInputColor;

            setCursorPosition(postIndicatorX, postIndicatorY);

            string userInput = Console.ReadLine();
            
            setCursorPosition(0, Console.CursorTop - 1);

            Console.Write(
                new String(
                    ' ', 
                    Console.WindowWidth
                )
            );
            setCursorPosition(initialX, Console.CursorTop - 2);

            return userInput;
        }
        private ConsoleColor lookupTextColorByParentElementName(string parentElementName){
            switch (parentElementName)
            {
                case "a":
                    return (ConsoleColor) userSettingsService.aTagColor;
                case "b":
                    return (ConsoleColor) userSettingsService.bTagColor;
                case "i":
                    return (ConsoleColor) userSettingsService.iTagColor;
                case "em":
                    return (ConsoleColor) userSettingsService.emTagColor;
                case "h1":
                    return (ConsoleColor) userSettingsService.h1TagColor;
                case "h2":
                    return (ConsoleColor) userSettingsService.h2TagColor;
                case "h3":
                    return (ConsoleColor) userSettingsService.h3TagColor;
                case "h4":
                    return (ConsoleColor) userSettingsService.h4TagColor;
                case "pre":
                    return (ConsoleColor) userSettingsService.preTagColor;
                case "":
                    return (ConsoleColor) userSettingsService.defaultTagColor;
                default:
                    return (ConsoleColor) userSettingsService.defaultTagColor;
            }
        }
        private void setConsoleColorByParentElementName(string parentElementName){
            Console.ForegroundColor = lookupTextColorByParentElementName(parentElementName);
        }
        public void breakLineIfRequiredByElement(string parentElementName){
            if (tagsToBreakLineBeforeAndAfter.Contains(parentElementName)){
                newLine();
            }
        }
        public int breakLineIfRequiredByElementAndCountLinesWritten(string parentElementName){
            if (tagsToBreakLineBeforeAndAfter.Contains(parentElementName)){
                newLine();
                return 1;
            }
            return 0;
        }
        public void newLine(int numberOfNewlines){
            int iterations = 0;
            while (iterations < numberOfNewlines){
                newLine();
                iterations++;
            }
        }
        public void newLine(){
            Console.Write("\n");
            charactersUsedThisLine = 0;
            progressBarIsActive = false; // a progress bar must stay on one line if it
                                         // overflows, it means progress is complete
        }
        public void horizontalRuler(char characterToBuildWith = '_'){
            newLine();
            prettyPrintWithoutPadding(
                $" {new String(characterToBuildWith, Console.WindowWidth - 2)} "
            );
            newLine();
        }
        public void prettyPrint(string textToOutput, string parentElementName){

            prettyPrintAndCountLinesWritten(textToOutput, parentElementName);
        }

        public void prettyPrint(string textToOutput, ConsoleColor textColor){

            prettyPrintAndCountLinesWritten(textToOutput, textColor);
        }
        public int prettyPrintAndCountLinesWritten(string textToOutput, string parentElementName){

            setConsoleColorByParentElementName(parentElementName);
            return prettyPrintAndCountLines(textToOutput);
        }

        public int prettyPrintAndCountLinesWritten(string textToOutput, ConsoleColor textColor){

            Console.ForegroundColor = textColor;
            return prettyPrintAndCountLines(textToOutput);
        }

        private int prettyPrintAndCountLines(string textToOutput, bool addPadding = true){

            textToOutput = newLineRegex.Replace(textToOutput, new String(characterToBreakLinesOn, 1));

            string[] words = textToOutput.Split(characterToBreakLinesOn);

            int newLinesAdded = 0;

            int padding;

            if (addPadding){
                padding = userSettingsService.textBodyPadding;
            }
            else {
                padding = 0;
            }

            foreach (string word in words){
                int charactersUsedIfNotBreakingLine = charactersUsedThisLine + word.Length + padding;

                if (charactersUsedIfNotBreakingLine >= Console.WindowWidth){
                    newLine();
                    newLinesAdded++;
                }

                if (charactersUsedThisLine == 0){
                    Console.Write(new String(' ', padding));
                    charactersUsedThisLine += padding;
                }

                Console.Write(
                    HttpUtility.HtmlDecode(
                        $"{word}{characterToBreakLinesOn}"
                    )
                    
                );
                charactersUsedThisLine += word.Length + 1;
            }
            return newLinesAdded;
        }
        public void prettyPrint(string textToOutput){
            prettyPrint(textToOutput, "");
        }

        public void prettyPrintWithoutPadding(string textToOutput){
            Console.ForegroundColor = ConsoleColor.Gray;
            prettyPrintAndCountLines(textToOutput, false);
        }
        public void printError(string errorMessage){
            if (userSettingsService.logLevel >= 1){
                newLine();
                prettyPrint(
                    "Error: ",
                    ConsoleColor.Red
                );
                prettyPrint(
                    errorMessage,
                    ConsoleColor.DarkGray
                );
            }
        }
        public void printWarning(string errorMessage){
            if (userSettingsService.logLevel >= 2){
                newLine();
                prettyPrint(
                    "Warning: ",
                    ConsoleColor.Yellow
                );
                prettyPrint(
                    errorMessage,
                    ConsoleColor.DarkGray
                );
            }
        }
        public void printDebug(string errorMessage){
            if (userSettingsService.logLevel >= 3){
                newLine();
                prettyPrint(
                    "Debug: ",
                    ConsoleColor.White
                );
                prettyPrint(
                    errorMessage,
                    ConsoleColor.DarkGray
                );
            }
        }
        public void printTrace(string errorMessage){
            if (userSettingsService.logLevel >= 4){
                newLine();
                prettyPrint(
                    "Trace: ",
                    ConsoleColor.Gray
                );
                prettyPrint(
                    errorMessage,
                    ConsoleColor.DarkGray
                );
            }
        }
        public void prettyPrintInQuotesCenter(string text, ConsoleColor textColor, string quoteOrOtherWrappingString, ConsoleColor quoteColor){
            int padding = Convert.ToInt32(
                (Console.WindowWidth - text.Length) / 2 - 1
            );

            if (padding > 0){
                string stringPadding = new String(' ', padding);
                Console.Write(stringPadding);
                Console.ForegroundColor = quoteColor;
                Console.Write(quoteOrOtherWrappingString);
                Console.ForegroundColor = textColor;
                Console.Write(text);
                Console.ForegroundColor = quoteColor;
                Console.Write(quoteOrOtherWrappingString);
                Console.Write(stringPadding.Remove(padding - 1));
            }
            else{
                Console.Write(text, textColor);
            }
        }
        public void prettyPrintCenter(string text, ConsoleColor textColor){
            int padding = Convert.ToInt32(
                (Console.WindowWidth - text.Length) / 2 - 1
            );

            if (padding > 0){
                string stringPadding = new String(' ', padding);
                Console.ForegroundColor = textColor;
                Console.Write(stringPadding);
                Console.Write(text, textColor);
                Console.Write(stringPadding.Remove(padding - 1));
            }
            else{
                Console.Write(text, textColor);
            }
        }
        public void printUriCenter(WebAddress address){
            int padding = Convert.ToInt32(
                (Console.WindowWidth - address.fullUrl.Length) / 2 - 1
            );

            if (padding > 0){
                string stringPadding = new String(' ', padding);
                Console.Write(stringPadding);
                printUri(address.fullUrl);
                Console.Write(stringPadding.Remove(padding - 1));
            }
            else{
                printUri(address.fullUrl);
            }
        }
        public void printUriOriginCenter(WebAddress address){
            int padding = Convert.ToInt32(
                (Console.WindowWidth - address.origin.Length) / 2 - 1
            );

            if (padding > 0){
                string stringPadding = new String(' ', padding);
                Console.Write(stringPadding);
                printUriOrigin(address.origin);
                Console.Write(stringPadding);
            }
            else{
                printUriOrigin(address.origin);
            }
        }
        public void printUri(string address){
            printUri(
                new Uri(address)
            );
        }
        public void printUriOrigin(string address){
            printUri(
                new Uri(address),
                true
            );
        }
        private void printUri(Uri address, bool originOnly = false){

            switch (address.Scheme)
            {
                case ("https"):
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case ("http"):
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
            }
            Console.Write(address.Scheme);

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(@"://");

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(address.Host);

            if (!originOnly){
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(address.PathAndQuery);
            }
        }
        private void clearCurrentLine(){
            Console.CursorLeft = 0;
            Console.Write(new String(' ', Console.WindowWidth));
            Console.CursorLeft = 0;
        }
        public void beginProgressBar(){
            lock(locker){
                if (!progressBarIsActive){
                    progressBarStopwatch.Restart();
                    newLine();
                    Console.ForegroundColor = ConsoleColor.Gray;
                    writeTextInMiddleOfCurrentLineWithoutOverwritingAnythingElse($"[ {0}% ]");
                    Console.Write(" _");
                    progressBarIsActive = true;
                }
            }
        }
        public void updateProgressBar(int progress){
            lock(locker){
                if (progressBarIsActive){

                    int progressRemaining() => (100 - progress)*Console.WindowWidth / 100;
                    int unfilledProgressBarSpaceRemaining() => Console.WindowWidth - Console.CursorLeft - 1;

                    while (progressRemaining() < unfilledProgressBarSpaceRemaining()){
                        if (unfilledProgressBarSpaceRemaining() > 0){
                            Console.Write("_");
                            writeTextInMiddleOfCurrentLineWithoutOverwritingAnythingElse($"[ {progress}% ]");
                        }
                        else{
                            break;
                        }
                    }
                }
            } 
        }
        public void incrementProgressBar(int progressPercentageChange){
            lock(locker){
                updateProgressBar(Console.CursorLeft + progressPercentageChange);
            }
        }
        public void closeProgressBar(){
            lock(locker){
                progressBarStopwatch.Stop();
                writeTextInMiddleOfCurrentLineWithoutOverwritingAnythingElse($"[ {progressBarStopwatch.Elapsed.TotalSeconds.ToString("F2")}s ]");
                progressBarIsActive = false;
            }
        }
        private void writeTextInMiddleOfCurrentLineWithoutOverwritingAnythingElse(string text){
            int originalPosition = Console.CursorLeft;
            Console.CursorLeft = Console.WindowWidth/2 - text.Length/2;
            Console.Write(text);
            Console.CursorLeft = originalPosition;
        }
    }
}