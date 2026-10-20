using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Kombos.Services;

using Xunit;

namespace Kombos.Tests
{
    /// <summary>"Κόμβος Gateway" (v4.6.0) - επιβεβαιώνει ότι ο HttpListener πράγματι δένει σε loopback και
    /// επιστρέφει τους σωστούς κωδικούς κατάστασης, χωρίς να χρειάζεται πραγματικό κλειδί API (η διαδρομή
    /// "κανένα agent ρυθμισμένο" καλύπτεται πλήρως τοπικά).</summary>
    public class GatewayServerTests
    {
        [Fact]
        public async Task NoAgentsConfigured_Returns503()
        {
            var server = new GatewayServer(() => new List<GatewayAgentEntry>(), _ => { });
            Assert.True(server.TryStart(28421, out _));
            try
            {
                using var http = new HttpClient();
                var response = await http.PostAsync("http://127.0.0.1:28421/v1/chat/completions",
                    new StringContent("{\"messages\":[{\"role\":\"user\",\"content\":\"hi\"}]}", Encoding.UTF8, "application/json"));
                Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
            }
            finally { server.Stop(); }
        }

        [Fact]
        public async Task WrongPathOrMethod_Returns404()
        {
            var server = new GatewayServer(() => new List<GatewayAgentEntry>(), _ => { });
            Assert.True(server.TryStart(28422, out _));
            try
            {
                using var http = new HttpClient();
                var response = await http.GetAsync("http://127.0.0.1:28422/");
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
            finally { server.Stop(); }
        }

        [Fact]
        public async Task EmptyMessages_Returns400()
        {
            var server = new GatewayServer(() => new List<GatewayAgentEntry> { new("Claude", "fake-key", null, null) }, _ => { });
            Assert.True(server.TryStart(28423, out _));
            try
            {
                using var http = new HttpClient();
                var response = await http.PostAsync("http://127.0.0.1:28423/v1/chat/completions",
                    new StringContent("{\"messages\":[]}", Encoding.UTF8, "application/json"));
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            }
            finally { server.Stop(); }
        }
    }
}
