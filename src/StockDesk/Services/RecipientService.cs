using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StockDesk.Data;
using StockDesk.Data.Entities;

namespace StockDesk.Services;

public class RecipientService : IRecipientService
{
    private readonly StockDbContext _dbContext;

    public RecipientService(StockDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<string>> GetRecipientNamesAsync()
    {
        var recipients = await _dbContext.Recipients
            .AsNoTracking()
            .ToListAsync();

        return recipients
            .OrderBy(r => r.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(r => r.Name)
            .ToList();
    }

    public async Task<Recipient> GetOrCreateRecipientAsync(string name)
    {
        string trimmed = name.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            throw new ArgumentException("Təhvil alan şəxs/şöbə adı boş ola bilməz.", nameof(name));
        }

        var all = await _dbContext.Recipients.ToListAsync();
        var existing = all.FirstOrDefault(r => 
            string.Equals(r.Name, trimmed, StringComparison.CurrentCultureIgnoreCase) ||
            string.Equals(r.Name, trimmed, StringComparison.OrdinalIgnoreCase));

        if (existing != null)
        {
            return existing;
        }

        var newRecipient = new Recipient
        {
            Name = trimmed,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Recipients.Add(newRecipient);
        await _dbContext.SaveChangesAsync();

        return newRecipient;
    }
}
