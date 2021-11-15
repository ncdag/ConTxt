using Xunit;
using WebBrowser;

namespace ConTxtTests
{
    public class WebAddressUnitTests
    {
        string stringBaseUrlForTests = "https://localhost/x/y/z";

        [Theory]
        [InlineData("//a/b/c", "https://a/b/c")]
        [InlineData("/a/b/c", "https://localhost/a/b/c")]
        [InlineData("/", "https://localhost/")]
        [InlineData(".", "https://localhost/x/y/z")]
        [InlineData("http://a/b/c", "http://a/b/c")]
        [InlineData("https://a/b/c", "https://a/b/c")]
        [InlineData("//a/b/c/", "https://a/b/c/")]
        [InlineData("/a/b/c/", "https://localhost/a/b/c/")]
        [InlineData("http://a/b/c/", "http://a/b/c/")]
        [InlineData("https://a/b/c/", "https://a/b/c/")]
        [InlineData("//a/b/c.d", "https://a/b/c.d")]
        [InlineData("/a/b/c.d", "https://localhost/a/b/c.d")]
        [InlineData("http://a/b/c.d", "http://a/b/c.d")]
        [InlineData("https://a/b/c.d", "https://a/b/c.d")]
        [InlineData("//duckduckgo.com/l","https://duckduckgo.com/l")]
        public void validShorthandAddressesShouldExpandCorrectly(string shorthandUrl, string expectedResult){
            WebAddress addressForContext = WebAddress.createAddress(stringBaseUrlForTests);
            string actualResult = WebAddress.createAddress(shorthandUrl, addressForContext).fullUrl;

            Assert.Equal(expectedResult, actualResult);
        }
        [Theory]
        [InlineData(@"a\b\c")]
        [InlineData("abc")]
        [InlineData("a/b/c")]
        [InlineData("a.b.c")]
        [InlineData("")]
        [InlineData("://a/b/c")]
        public void invalidShorthandAddressesShouldReturnNull(string shorthandUrl){
            WebAddress addressForContext = WebAddress.createAddress(stringBaseUrlForTests);
            WebAddress actualResult = WebAddress.createAddress(shorthandUrl, addressForContext);

            Assert.Null(actualResult);
        }
        [Theory]
        [InlineData("http://a/b/c.html")]
        [InlineData("http://a/b/c.d")]
        [InlineData("http://x.y.z.d/b/c.html")]
        [InlineData("http://x.y.z.d/b/c.d")]
        [InlineData("https://a/b/c.html")]
        [InlineData("https://a/b/c.d")]
        [InlineData("https://x.y.z.d/b/c.html")]
        [InlineData("https://x.y.z.d/b/c.d")]
        [InlineData("abcd://a/b/c.html")]
        [InlineData("abcd://a/b/c.d")]
        [InlineData("abcd://x.y.z.d/b/c.html")]
        [InlineData("abcd://x.y.z.d/b/c.d")]
        [InlineData("abcd.ef://a/b/c.html")]
        [InlineData("abcd.ef://a/b/c.d")]
        public void validFullAddressesShouldNotChange(string fullUrl){
            string expectedResult = fullUrl;
            string actualResult = WebAddress.createAddress(fullUrl).fullUrl;

            Assert.Equal(expectedResult, actualResult);
        }
        [Theory]
        [InlineData("//a/b/c")]
        [InlineData("/a/b/c")]
        [InlineData("/")]
        [InlineData(".")]
        [InlineData("//a/b/c/")]
        [InlineData("/a/b/c/")]
        [InlineData("//a/b/c.d")]
        [InlineData("/a/b/c.d")]
        [InlineData(@"a\b\c")]
        [InlineData("abc")]
        [InlineData("a/b/c")]
        [InlineData("a.b.c")]
        [InlineData("")]
        [InlineData("://a/b/c")]
        public void invalidFullAddressesShouldReturnNull(string fullUrl){
            WebAddress actualResult = WebAddress.createAddress(fullUrl);

            Assert.Null(actualResult);
        }
    }
}
