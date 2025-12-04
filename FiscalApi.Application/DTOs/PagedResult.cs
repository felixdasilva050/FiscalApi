namespace FiscalApi.Application.DTOs;

// Classe Genérica <T>: Serve para paginar Notas, Usuários, Logs, qualquer coisa.
public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public long TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}