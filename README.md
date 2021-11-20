# ConTxt
ConTxt is text-based web browser software that runs in the command line console. It is written in C#.

## Disclaimer
__The "Software" means this software and associated documentation files.__
__THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.__

## Documentation
  ### Installation
  1. Verify that your machine has Microsoft's .NET 5 runtime installed by opening a command line terminal and running the command: ```dotnet --version```. 
      - If the command returns a version number where the __first digit__ is __equal to 5__, you have .NET 5 installed already. 
      - If the __first digit__ is __not 5__, or a __command-not-found error__ is returned, you don't have the .NET 5 runtime installed. You will need to get it from Microsoft before this software can be used.
  1. Clone or download the ConTxt software source code (this repository) into a folder on your machine. Note the path of that folder. 
  1. Open a command line terminal to the directory: __&lt;folder path from step 2&gt;/ConTxt/ConTxt__
      - For example:
         - If you're on __Linux or MacOS__ and you cloned the repository to __/Users/ExampleUser/downloads/software/__, then the directory would be __/Users/ExampleUser/downloads/software/ConTxt/Contxt__ for that hypothetical case.
         - If you're on __Windows__ and you cloned the repository to __C:\Users\ExampleUser\Downloads__, then the directory would be __C:\Users\ExampleUser\Downloads\ConTxt\Contxt__ for that hypothetical case.
  1. Build the application by running the command: ```dotnet publish -C Release``` in the __ConTxt/ConTxt__ directory.
  1. Start the program by running the command: ```dotnet ./bin/Release/net5.0/ConTxt.dll``` in the __ConTxt/ConTxt__ directory.

  ### Getting Started
  Type in search keywords and press enter to search the web. Then, enter the hexadecimal number which follows the link result you want to follow. 

  ### Command List
  ConTxt responds to user input commands you provide in the terminal while the program is running.
  - Empty string / return key __=>__ While on a page, read the next console-length of content to the screen.
  - 'Help' or 'h' command __=>__ Open this page to reread the instructions.
  - 'Back' or '<' command __=>__ Navigate backward in page history. 
  - 'Forward' or '>' command __=>__ Navigate forward in page history. 
  - 'Exit' or 'bye' command __=>__ Close the program and return to the shell. 
  -  Hexadecimal number __=>__ Navigates to the link with that number shown in curly brackets following it.
  -  String not listed above __=>__ Search the web with the string as the keywords, using the search engine specified in UserSettings.xml (default is DuckDuckGo). 


  ### Configuration 

  You can customize ConTxt by editing the UserSettings.xml file located in the same directory as this application. The list 
  below describes what the inner text value of each element in the hierarchy is.                                                

  - / settings / web / contentlanguage __=>__ two-letter abbreviation for the language you want sites to send content in, e.g. 
  en for English, es for Spanish. 
  - / settings / navigation / trustedoriginsstringlist / origin __=>__ site origin which you trust not to serve content which 
  would harm your computer or steal your data. Manually listing a site here will tell ConTxt to skip the step of asking for 
  your permission to navigate to that listed site if you have not yet visited it this browsing session. 
  - / settings / navigation / searchbaseurl __=>__ the base URL of the desired web search service, e.g. DuckDuckGo. 
  - / settings / navigation / searchqueryargname __=>__ the query string argument name used by the above search service for 
  submitting new search keywords. 
  - / settings / console / textbodypadding __=>__ number of character cells you want to leave empty as padding between content 
  and the left and right edges of the console window. 
  - / settings / general / loglevel __=>__ for troubleshooting the application. Higher positive integer values will log more 
  information. 
