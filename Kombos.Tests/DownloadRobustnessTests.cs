using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Kombos.Services;
using Xunit;

namespace Kombos.Tests
{
    /// <summary>Σώμα HTTP που στέλνει μερικά bytes και μετά "κολλάει" (καμία άλλη ανάγνωση δεν ολοκληρώνεται).</summary>
    internal class StallingStream : Stream
    {
        private bool _first = true;
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken ct) => await ReadAsync(buffer.AsMemory(offset, count), ct);
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken ct = default)
        {
            if (_first) { _first = false; buffer.Span[0] = 1; return 1; }
            await Task.Delay(Timeout.Infinite, ct);
            return 0;
        }
        public override bool CanRead => true; public override bool CanSeek => false; public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }

    public class DownloadRobustnessTests : IDisposable
    {
        private readonly string _dir = Path.Combine(Path.GetTempPath(), "kombos-tests-" + Guid.NewGuid().ToString("N"));
        public DownloadRobustnessTests() => Directory.CreateDirectory(_dir);
        public void Dispose() { try { Directory.Delete(_dir, true); } catch { } }

        private static readonly ReleaseAsset Asset = new("S.exe", 100, "https://github.com/a/b/releases/download/v1/S.exe", null);
        private static HttpClient Stalling() => new(new FakeHandler { Respond = _ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(new StallingStream()) } });

        [Fact]
        public async Task StalledConnection_TimesOut_AndLeavesNoPartFile()
        {
            await Assert.ThrowsAsync<TimeoutException>(() =>
                ReleaseDownloader.DownloadVerifiedAsync(Stalling(), Asset, new string('0', 64), _dir, null, default, TimeSpan.FromMilliseconds(300)));
            Assert.Empty(Directory.GetFiles(_dir));
        }

        [Fact]
        public async Task Cancellation_Throws_AndLeavesNoPartFile()
        {
            using var cts = new CancellationTokenSource();
            var task = ReleaseDownloader.DownloadVerifiedAsync(Stalling(), Asset, new string('0', 64), _dir, null, cts.Token, TimeSpan.FromSeconds(30));
            await Task.Delay(200);
            cts.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
            Assert.Empty(Directory.GetFiles(_dir));
        }

        [Fact]
        public async Task FailedRequest_LeavesNothing_AndCanBeRetried()
        {
            var calls = 0;
            var data = System.Text.Encoding.UTF8.GetBytes("ok-bytes");
            var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(data)).ToLowerInvariant();
            var http = new HttpClient(new FakeHandler
            {
                Respond = _ => ++calls == 1 ? new HttpResponseMessage(HttpStatusCode.InternalServerError) : new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(data) }
            });
            await Assert.ThrowsAsync<HttpRequestException>(() => ReleaseDownloader.DownloadVerifiedAsync(http, Asset, hash, _dir));
            Assert.Empty(Directory.GetFiles(_dir));
            var path = await ReleaseDownloader.DownloadVerifiedAsync(http, Asset, hash, _dir);   // ξαναπάτημα: δουλεύει
            Assert.True(File.Exists(path));
            Assert.False(File.Exists(path + ".part"));
        }
    }
}
