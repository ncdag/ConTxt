using System;
using System.Collections.Generic;

namespace WebBrowser
{
    class UserSettingsService : SettingsService
    {
        public int logLevel {get; protected set;} = 0;
        public int textBodyPadding {get; protected set;} = 2;
        public int aTagColor {get; protected set;} = (int) ConsoleColor.DarkYellow;
        public int bTagColor {get; protected set;} = (int) ConsoleColor.Blue;
        public int iTagColor {get; protected set;} = (int) ConsoleColor.DarkGreen;
        public int emTagColor {get; protected set;} = (int) ConsoleColor.DarkGreen;
        public int h1TagColor {get; protected set;} = (int) ConsoleColor.Blue;
        public int h2TagColor {get; protected set;} = (int) ConsoleColor.Blue;
        public int h3TagColor {get; protected set;} = (int) ConsoleColor.Blue;
        public int h4TagColor {get; protected set;} = (int) ConsoleColor.Blue;
        public int preTagColor {get; protected set;} = (int) ConsoleColor.DarkGray;
        public int defaultTagColor {get; protected set;} = (int) ConsoleColor.Gray;

        public string contentLanguage {get; protected set;} = "en";
        public string searchBaseUrl {get; protected set;} = "https://lite.duckduckgo.com/lite";
        public string searchQueryArgName {get; protected set;} = "q";
        public string linkLabelPrefix {get; protected set;} = " {";
        public string linkLabelSuffix {get; protected set;} = "}";
        public string imageLabelPrefix {get; protected set;} = "[Image: ";
        public string imageLabelSuffix {get; protected set;} = "]";
        public string listItemPrefix {get; protected set;} = "&#8226; ";
        public string imagePlaceholderIfNoAltText {get; protected set;} = "no text description provided";

        public List<string> trustedOriginsStringList {get; protected set;} = new List<string>();

        public UserSettingsService(string SettingsFilePath) : base(SettingsFilePath){}
    }
}