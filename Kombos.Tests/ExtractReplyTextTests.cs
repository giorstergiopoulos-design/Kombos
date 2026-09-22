using Kombos;
using Xunit;

namespace Kombos.Tests
{
    /// <summary>MainWindow.ExtractReplyText/StripFences είναι static (δεν χρειάζονται instance παραθύρου) -
    /// αναλύουν την απάντηση JSON που περιμένουμε από τον Claude ({"reply": "...", "tools": [...]}) .</summary>
    public class ExtractReplyTextTests
    {
        [Fact]
        public void ExtractReplyText_OnCleanJson_ReturnsReplyAndTools()
        {
            var raw = "{\"reply\": \"Δοκιμή\", \"tools\": [{\"tool\": \"Claude\", \"why\": \"γενική χρήση\"}]}";
            var reply = MainWindow.ExtractReplyText(raw, out var tools);

            Assert.Equal("Δοκιμή", reply);
            var tool = Assert.Single(tools);
            Assert.Equal("Claude", tool.ToolName);
            Assert.Equal("γενική χρήση", tool.Why);
        }

        [Fact]
        public void ExtractReplyText_OnJsonWrappedInMarkdownFences_StillParses()
        {
            var raw = "```json\n{\"reply\": \"Δοκιμή\", \"tools\": []}\n```";
            var reply = MainWindow.ExtractReplyText(raw, out var tools);

            Assert.Equal("Δοκιμή", reply);
            Assert.Empty(tools);
        }

        [Fact]
        public void ExtractReplyText_OnNonJsonText_ReturnsRawTextInstead()
        {
            // Ο Claude δεν επιστρέφει πάντα καθαρό JSON - το ExtractReplyText δεν πρέπει να πετάξει εξαίρεση
            // αλλά να επιστρέψει το ίδιο το κείμενο ως έχει.
            var raw = "Αυτό δεν είναι JSON, απλά μια κανονική απάντηση.";
            var reply = MainWindow.ExtractReplyText(raw, out var tools);

            Assert.Equal(raw, reply);
            Assert.Empty(tools);
        }

        [Fact]
        public void ExtractReplyText_WithMoreThanFiveTools_StillReturnsAllProvided()
        {
            // Ο κανόνας "το πολύ 5 εργαλεία" επιβάλλεται από το system prompt (τι ζητάμε από το μοντέλο),
            // όχι από τον client - το ExtractReplyText είναι απλός parser, δεν πρέπει να κόβει σιωπηλά.
            var raw = "{\"reply\": \"ok\", \"tools\": [" +
                      "{\"tool\":\"A\",\"why\":\"1\"},{\"tool\":\"B\",\"why\":\"2\"},{\"tool\":\"C\",\"why\":\"3\"}," +
                      "{\"tool\":\"D\",\"why\":\"4\"},{\"tool\":\"E\",\"why\":\"5\"},{\"tool\":\"F\",\"why\":\"6\"}]}";
            MainWindow.ExtractReplyText(raw, out var tools);
            Assert.Equal(6, tools.Count);
        }

        [Fact]
        public void StripFences_RemovesJsonCodeFenceMarkers()
        {
            var input = "```json\n{\"a\":1}\n```";
            Assert.Equal("{\"a\":1}", MainWindow.StripFences(input));
        }

        [Fact]
        public void StripFences_OnPlainTextWithoutFences_ReturnsTrimmedTextUnchanged()
        {
            Assert.Equal("hello", MainWindow.StripFences("  hello  "));
        }
    }
}
