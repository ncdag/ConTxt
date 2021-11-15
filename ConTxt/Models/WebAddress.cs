using System;
using System.Text.RegularExpressions;

namespace WebBrowser
{
    public class WebAddress : Uri
    {
        public string origin {
            get {
                return $"{Scheme}://{Host}:{Port}";
            }
        }
        public string fullUrl{
            get {
                return ToString();
            }
        }
        private static string expandShortAddressString(string shortAddress, WebAddress addressForContext){

            if (shortAddress == "."){
                return addressForContext.fullUrl;
            }

            // components of the 'addressForContext' (e.g. protocol, host) are used to fill in the gaps which a shorthand url may have
            
            if (shortAddress.Length >= 2 && shortAddress.Substring(0,2) == "//"){
                // if the site uses the 'same protocol' shorthand
                shortAddress = $"{addressForContext.Scheme}:{shortAddress}";
            }
            if (new Regex("^/[^/]*").IsMatch(shortAddress)){
                // if the site gives a relative path
                shortAddress = $"{addressForContext.Scheme}://{addressForContext.Host}{shortAddress}";
            }
            
            return shortAddress;
        }
        private void assertThatFileProtocolWasNotAssumedByDefaultByUriConstructor(string rawAddressSuppliedByUser, string uriBaseClassConstructorResult){
            string fileProtocolRegex = @"^file://";

            bool uriBaseClassConstructorResultUsesFileProtocol = Regex.Match(
                uriBaseClassConstructorResult,
                fileProtocolRegex
            ).Success;

            bool addressExplicitlySpecifiedFileProtocol = Regex.Match(
                rawAddressSuppliedByUser,
                fileProtocolRegex
            ).Success;

            bool uriBaseClassAssumedFileProtocol = uriBaseClassConstructorResultUsesFileProtocol && (!addressExplicitlySpecifiedFileProtocol);

            if (uriBaseClassAssumedFileProtocol){
                throw new FormatException();
            }
        }
        private WebAddress(string address) : base(address){
            // the Uri base class's constructor sometimes appends the file protocol onto
            // supplied addresses when no protocol is supplied. This check is to prevent
            // this class from making the same assumption, and instead treating the input
            // address as invalid
            assertThatFileProtocolWasNotAssumedByDefaultByUriConstructor(address, fullUrl);
        }
        public static WebAddress createAddress(string address){
            try {
                return new WebAddress(address);
            }
            catch{
                return null;
            }
        }
        public static WebAddress createAddress(string address, WebAddress addressForContext){
            try {
                return new WebAddress(
                    expandShortAddressString(address, addressForContext)
                );
            }
            catch{
                return null;
            }
        }
    }
}