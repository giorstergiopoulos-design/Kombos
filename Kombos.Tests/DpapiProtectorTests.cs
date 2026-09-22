using Kombos.Services;
using Xunit;

namespace Kombos.Tests
{
    public class DpapiProtectorTests
    {
        [Fact]
        public void Protect_ThenUnprotect_RoundTripsOriginalText()
        {
            var original = "sk-ant-test-1234567890";
            var protectedText = DpapiProtector.Protect(original);
            var result = DpapiProtector.TryUnprotect(protectedText);
            Assert.Equal(original, result);
        }

        [Fact]
        public void Protect_ProducesDifferentTextThanInput()
        {
            var original = "my-secret-key";
            var protectedText = DpapiProtector.Protect(original);
            Assert.NotEqual(original, protectedText);
        }

        [Fact]
        public void TryUnprotect_OnPlaintextLegacyValue_ReturnsNull()
        {
            // Πριν το v2.3.0 τα κλειδιά αποθηκεύονταν σε καθαρό κείμενο - το TryUnprotect πρέπει να
            // επιστρέφει null (όχι να πετάξει exception) ώστε ο καλών να κάνει migration στο plaintext.
            var result = DpapiProtector.TryUnprotect("just-a-plain-old-key");
            Assert.Null(result);
        }

        [Fact]
        public void TryUnprotect_OnGarbageBase64_ReturnsNullInsteadOfThrowing()
        {
            var result = DpapiProtector.TryUnprotect("not valid base64 !!!");
            Assert.Null(result);
        }
    }
}
