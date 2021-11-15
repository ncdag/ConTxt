using System;

namespace WebBrowser
{
    interface IConsoleService
    {
        void updateWindowTitle();
        void updateWindowTitle(string pageName, string pageOrigin);
        void getCursorPosition(out int savedConsoleCursorX, out int savedConsoleCursorY );
        void setCursorPosition(int newX, int newY);
        string getLineOfUserInputAndMoveBackTheCursorBackToWhereItWas();
        void breakLineIfRequiredByElement(string parentElementName);
        int breakLineIfRequiredByElementAndCountLinesWritten(string parentElementName);
        void newLine(int numberOfNewlines);
        void newLine();
        void horizontalRuler(char characterToBuildWith = '_');
        void prettyPrint(string textToOutput, string parentElementName);
        void prettyPrint(string textToOutput, ConsoleColor textColor);
        int prettyPrintAndCountLinesWritten(string textToOutput, string parentElementName);
        int prettyPrintAndCountLinesWritten(string textToOutput, ConsoleColor textColor);
        void prettyPrint(string textToOutput);
        void prettyPrintWithoutPadding(string textToOutput);
        void printError(string errorMessage);
        void printWarning(string errorMessage);
        void printDebug(string errorMessage);
        void printTrace(string errorMessage);
        void prettyPrintInQuotesCenter(string text, ConsoleColor textColor, string quoteOrOtherWrappingString, ConsoleColor quoteColor);
        void prettyPrintCenter(string text, ConsoleColor textColor);
        void printUriCenter(WebAddress address);
        void printUriOriginCenter(WebAddress address);
        void printUri(string address);
        void printUriOrigin(string address);
        void beginProgressBar();
        void updateProgressBar(int progressPercentage);
        void incrementProgressBar(int progressPercentageChange);
        void closeProgressBar();
    }
}