using System.Collections.Generic;
using System.Threading.Tasks;
using StockDesk.Data.Entities;

namespace StockDesk.Services;

public interface IRecipientService
{
    Task<List<string>> GetRecipientNamesAsync();
    Task<Recipient> GetOrCreateRecipientAsync(string name);
}
