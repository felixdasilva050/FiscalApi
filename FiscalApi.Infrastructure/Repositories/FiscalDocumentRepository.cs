using FiscalApi.Domain.Entities;
using FiscalApi.Domain.Interfaces;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace FiscalApi.Infrastructure.Repositories;

public class FiscalDocumentRepository : IFiscalDocumentRepository
{
    private readonly IMongoCollection<FiscalDocument> _collection;

    public FiscalDocumentRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<FiscalDocument>("notas_fiscais");
    }

    public async Task UpsertAsync(FiscalDocument document)
    {
        // O Id já vem preenchido (Chave ou Hash), garantindo idempotência no Replace
        await _collection.ReplaceOneAsync(
            x => x.Id == document.Id,
            document,
            new ReplaceOptions { IsUpsert = true }
        );
    }

    public async Task<IEnumerable<FiscalDocument>> GetAllAsync(int page, int pageSize, DateTime? dataInicio, string? emitenteDoc)
    {
        var builder = Builders<FiscalDocument>.Filter;
        var filter = builder.Empty;

        if (dataInicio.HasValue)
            filter &= builder.Gte(x => x.DataEmissao, dataInicio.Value);

        if (!string.IsNullOrEmpty(emitenteDoc))
        {
            // Codifica para Base64 antes de buscar, pois no banco está codificado
            var encoded = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(emitenteDoc));
            filter &= builder.Eq(x => x.EmitenteDocEncodado, encoded);
        }

        return await _collection.Find(filter)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<FiscalDocument> GetByIdAsync(string id) =>
        await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task DeleteAsync(string id) =>
        await _collection.DeleteOneAsync(x => x.Id == id);
}