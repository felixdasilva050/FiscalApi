namespace FiscalApi.Domain.Interfaces;
using FiscalApi.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
public interface IFiscalDocumentRepository
{
    Task UpsertAsync(FiscalDocument document);
    Task<FiscalDocument> GetByIdAsync(string id);
    Task<IEnumerable<FiscalDocument>> GetAllAsync(int page, int pageSize, DateTime? dataInicio, string? emitenteDoc);
    Task DeleteAsync(string id);
}