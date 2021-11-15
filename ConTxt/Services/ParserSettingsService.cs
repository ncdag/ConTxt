using System.Collections.Generic;

namespace WebBrowser
{
    class ParserSettingsService : SettingsService
    {
        public List<string> allowedSelfClosingElementStringList {get; protected set;} = new List<string>();
        public List<string> allowedTextContainingElementStringList {get; protected set;} = new List<string>();
        public ParserSettingsService(string SettingsFilePath) : base(SettingsFilePath){}
    }
}