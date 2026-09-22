using System;
using System.Collections.Generic;
using System.IO;
using Kombos.Models;
using Kombos.Services;
using Xunit;

namespace Kombos.Tests
{
    /// <summary>Ίδιο μοτίβο backup/restore με το SettingsStoreTests - το ConversationStore διαβάζει/γράφει το
    /// σταθερό %AppData%\Kombos\conversations.json, οπότε τα tests δεν πρέπει να πειράξουν πραγματικά δεδομένα.</summary>
    public class ConversationStoreTests : IDisposable
    {
        private readonly string _filePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Kombos", "conversations.json");
        private readonly string? _backup;

        public ConversationStoreTests()
        {
            _backup = File.Exists(_filePath) ? File.ReadAllText(_filePath) : null;
        }

        public void Dispose()
        {
            if (_backup != null) File.WriteAllText(_filePath, _backup);
            else if (File.Exists(_filePath)) File.Delete(_filePath);
        }

        [Fact]
        public void Save_ThenLoad_RoundTripsConversationsAndTurns()
        {
            var conversations = new List<Conversation>
            {
                new Conversation
                {
                    Id = "abc123",
                    Title = "Test chat",
                    History = new List<ChatTurn>
                    {
                        new ChatTurn { Role = "user", Content = "Γεια" },
                        new ChatTurn { Role = "assistant", Content = "Γεια σου!" }
                    }
                }
            };

            ConversationStore.Save(conversations);
            var loaded = ConversationStore.Load();

            var conv = Assert.Single(loaded);
            Assert.Equal("abc123", conv.Id);
            Assert.Equal("Test chat", conv.Title);
            Assert.Equal(2, conv.History.Count);
            Assert.Equal("user", conv.History[0].Role);
            Assert.Equal("Γεια", conv.History[0].Content);
        }

        [Fact]
        public void Load_WhenFileMissing_ReturnsEmptyList()
        {
            if (File.Exists(_filePath)) File.Delete(_filePath);
            var loaded = ConversationStore.Load();
            Assert.Empty(loaded);
        }

        [Fact]
        public void Load_WhenFileCorrupt_ReturnsEmptyListInsteadOfThrowing()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            File.WriteAllText(_filePath, "{ this is not valid json ][");
            var loaded = ConversationStore.Load();
            Assert.Empty(loaded);
        }
    }
}
