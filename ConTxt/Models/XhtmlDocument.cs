using System;
using System.Xml;
using System.Web;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace WebBrowser
{
    class XhtmlDocument 
    {
        protected IConsoleService consoleService;
        protected IWebService webService;
        protected ParserSettingsService parserSettingsService;
        protected UserSettingsService userSettingsService;
        protected string rawDocumentString;
        protected string reducedWhiteSpaceDocumentString;
        protected XmlDocument xml;
        protected XmlNodeReader documentReader;
        protected Stack<string> documentReaderElementNameStack;
        public string title {get; protected set;}
        public Dictionary<string, string> linkHexIdNumberToUrl {get; protected set;} = new Dictionary<string, string>();
        protected double advancePageByThisFractionOfConsoleHeight = 0.75;
        protected XhtmlDocument(string rawDocumentStringFromWeb, UserSettingsService UserSettingsService, ParserSettingsService ParserSettingsService, IConsoleService ConsoleService, IWebService WebService){
            userSettingsService = UserSettingsService;
            parserSettingsService = ParserSettingsService;
            rawDocumentString = rawDocumentStringFromWeb;
            consoleService = ConsoleService;
            webService = WebService;
        } 
        public static async Task<XhtmlDocument> createPageAsync(string rawDocumentStringFromWeb, UserSettingsService UserSettingsService, ParserSettingsService ParserSettingsService,  IConsoleService ConsoleService, IWebService WebService){

            XhtmlDocument newInstance = new XhtmlDocument(rawDocumentStringFromWeb, UserSettingsService, ParserSettingsService, ConsoleService, WebService);

            await Task.Run(
                // this step is quite a lot of work, so do it on another thread
                () => newInstance.parseAndAnnotateDocument()
            );
            return newInstance;
        }
        protected void parseAndAnnotateDocument(){

            parseXmlIntoDOM();
            removeTagsNotExplicitlyAllowed();
            labelHyperlinks();
            markAllListItems();
            fillImageNodesWithAltText();
            loadDocumentTitle();

            documentReader = getXmlReader();
            documentReaderElementNameStack = new Stack<string>();
        }

        protected void parseXmlIntoDOM(){
            xml = new XmlDocument();
            xml.XmlResolver = null; // prevent loading of DTD / schema from web
            generateSimplifiedDocumentString();
            
            try{
                HtmlDocument documentAsHtml = new HtmlDocument();

                documentAsHtml.OptionOutputAsXml = true;
                documentAsHtml.OptionFixNestedTags = true;
                documentAsHtml.OptionWriteEmptyNodes = true;

                documentAsHtml.LoadHtml(reducedWhiteSpaceDocumentString);
                xml.LoadXml(documentAsHtml.DocumentNode.InnerHtml);
            }
            catch (XmlException ex){
                consoleService.printError($"The requested web page is not well-formed XHTML and the HTML parser was unable to transform it into XHTML: {ex.Message}");
            }
        }

        protected void generateSimplifiedDocumentString(){
            reducedWhiteSpaceDocumentString = new Regex(@"\s\s+").Replace(rawDocumentString, " ");
        }
        protected void loadDocumentTitle(){
            XmlNodeList titleTags = xml.GetElementsByTagName("title");

            if (titleTags.Count > 0){
                // if a title tag exists, set the title of the page as the first match
                title = HttpUtility.HtmlDecode(
                    titleTags[0].InnerText.Trim().Trim('/').Trim() // extra trims to handle HTMLAgilityPack adding extra '/' chars
                );
            }
            else{
                title = "";
            }
        }
        protected void removeTagsNotExplicitlyAllowed(){

            XmlNodeList allNodes = xml.SelectNodes("//*");
            HashSet<XmlNode> xmlNodeRemovalSet = new HashSet<XmlNode>();

            // split into identification and action loops, so that 
            // allNodes is not modified while we loop through it

            foreach (XmlNode node in allNodes){
                // identification loop
                if (!parserSettingsService.allowedTextContainingElementStringList.Contains(node.Name.ToLower())){
                    xmlNodeRemovalSet.Add(node);
                }

                if (node.InnerText.Trim() == "" && !parserSettingsService.allowedSelfClosingElementStringList.Contains(node.Name)){
                    // if the node and all its children do not contain any
                    // text, mark it for removal
                    xmlNodeRemovalSet.Add(node);
                }

                XmlAttribute hiddenForAccessibilityAttribute = node.Attributes["aria-hidden"];

                if (hiddenForAccessibilityAttribute != null){
                    if (hiddenForAccessibilityAttribute.Value.ToLower() == "true"){
                        xmlNodeRemovalSet.Add(node);
                    }
                }
            }

            foreach (XmlNode node in xmlNodeRemovalSet){
                // action loop
                node.ParentNode.RemoveChild(node);
            }
        }
        protected virtual bool stringHrefIsValidWebAddress(string hrefAttribute){
            return WebAddress.createAddress(hrefAttribute) != null;
        }
        protected void labelHyperlinks(){
            XmlNodeList allHyperlinkElements = xml.SelectNodes("//a");

            int nodeIndex = 1;

            foreach (XmlNode node in allHyperlinkElements){

                string linkHexId = nodeIndex.ToString("X");
                string linkUrl;

                try{
                    linkUrl = node.Attributes["href"].Value;

                    if (!stringHrefIsValidWebAddress(linkUrl)){
                        // if the link's href can't be interpreted as a valid web address, skip it
                        continue;
                    }
                }
                catch{
                    consoleService.printWarning("Unable to read href attribute of an <a> tag.");
                    continue;
                }

                node.InnerText += $"{userSettingsService.linkLabelPrefix}{linkHexId}{userSettingsService.linkLabelSuffix}";
                linkHexIdNumberToUrl.Add(linkHexId, linkUrl);
                nodeIndex++;
            }
        }
        protected void markAllListItems(){
            XmlNodeList allListItemElements = xml.SelectNodes("//li");

            foreach (XmlNode node in allListItemElements){
                node.InnerXml = $"{userSettingsService.listItemPrefix}{node.InnerXml}";
            }
        }
        
        protected void fillImageNodesWithAltText(){
            XmlNodeList allImageElements = xml.SelectNodes("//img");

            foreach (XmlNode node in allImageElements){
                try{
                    string altText;

                    try{
                        altText = node.Attributes["alt"].Value;
                    }
                    catch{
                        altText = "";
                    }

                    if (altText == null || altText == ""){
                        altText = userSettingsService.imagePlaceholderIfNoAltText;
                    }

                    XmlElement italicImageSubstitute = xml.CreateElement("i");
                    italicImageSubstitute.InnerText = $"{userSettingsService.imageLabelPrefix}{altText}{userSettingsService.imageLabelSuffix}";

                    node.AppendChild(italicImageSubstitute);
                }
                catch{
                    consoleService.printWarning("Could not set alt text for image.");
                }
            }
        }
        protected XmlNodeReader getXmlReader(){
            // returns a brand new XML Reader
            return new XmlNodeReader(xml);
        }
        protected void readDocumentToConsole(int linesToRead = 100, bool startOverAtTopOfFile = false){

            if (startOverAtTopOfFile){
                documentReader = getXmlReader();
                documentReaderElementNameStack = new Stack<string>();
            }

            int linesReadSoFar = 0;
            
            while (linesReadSoFar < linesToRead){

                if (!documentReader.Read()){
                    // try to read the next node, and exit the loop if there is nothing more to read
                    break;
                }
                
                switch (documentReader.NodeType)
                {
                    case (XmlNodeType.Element):

                        documentReaderElementNameStack.Push(documentReader.Name);
                        linesReadSoFar += consoleService.breakLineIfRequiredByElementAndCountLinesWritten(documentReader.Name);
                        break;

                    case (XmlNodeType.EndElement):
                        linesReadSoFar += consoleService.breakLineIfRequiredByElementAndCountLinesWritten(documentReaderElementNameStack.Pop());
                        break;

                    case (XmlNodeType.Text):
                            
                            if (documentReaderElementNameStack.Peek() != "title"){

                                linesReadSoFar += consoleService.prettyPrintAndCountLinesWritten(
                                    documentReader.Value.Trim(),
                                    documentReaderElementNameStack.Peek()
                                );
                            }
                        break;
                    
                    default:
                        break;
                }
            }
        }
        public void readWholePageToConsole(){
            readDocumentToConsole(
                Int32.MaxValue - 1
            );
        }
        public void readNextSectionOfPageToConsole(){
            readDocumentToConsole(
                Convert.ToInt32(
                    Convert.ToSingle(Console.WindowHeight) * advancePageByThisFractionOfConsoleHeight
                )
            );
        }
        public void readNextSectionOfPageToConsole(double linesToPrintAsFractionOfConsoleHeight){
            readDocumentToConsole(
                Convert.ToInt32(
                    Convert.ToSingle(Console.WindowHeight) * linesToPrintAsFractionOfConsoleHeight
                )
            );
        }
    }
}