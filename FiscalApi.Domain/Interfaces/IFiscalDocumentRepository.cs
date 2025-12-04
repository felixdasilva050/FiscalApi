namespace FiscalApi.Domain.Interfaces;
using FiscalApi.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
public interface IFiscalDocumentRepository
{
    Task UpsertAsync(FiscalDocument document);
    Task UpdateAsync(FiscalDocument document); // Para o PUT
    Task<FiscalDocument?> GetByIdAsync(string id);
        
    // Listagem com filtros
    Task<(IEnumerable<FiscalDocument> Data, long Total)> GetAllAsync(
        int page, 
        int pageSize, 
        DateTime? dataInicio, 
        DateTime? dataFim, 
        string? emitenteDoc);
            
    Task DeleteAsync(string id);
}