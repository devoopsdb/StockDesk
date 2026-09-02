using System;
using System.Threading.Tasks;
using StockDesk.Services;
using Xunit;

namespace StockDesk.Tests;

public class RecipientServiceTests
{
    [Fact]
    public async Task GetOrCreateRecipient_NewName_AddsAndPersists()
    {
        var (context, connection) = TestDbContextFactory.CreateInMemoryDbContext();
        using (connection)
        using (context)
        {
            var recipientService = new RecipientService(context);

            var r1 = await recipientService.GetOrCreateRecipientAsync("IT Şöbəsi");
            Assert.True(r1.Id > 0);
            Assert.Equal("IT Şöbəsi", r1.Name);

            // Fetch list
            var list = await recipientService.GetRecipientNamesAsync();
            Assert.Contains("IT Şöbəsi", list);

            // Re-fetch existing with different casing/whitespace
            var r2 = await recipientService.GetOrCreateRecipientAsync("  it şöbəsi  ");
            Assert.Equal(r1.Id, r2.Id);

            var listAfter = await recipientService.GetRecipientNamesAsync();
            Assert.Single(listAfter);
        }
    }
}
