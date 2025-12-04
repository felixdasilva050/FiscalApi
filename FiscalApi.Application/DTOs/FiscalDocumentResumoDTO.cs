namespace FiscalApi.Application.DTOs;

public class FiscalDocumentResumoDTO
{
    public string Id { get; set; }
    public string TipoDocumento { get; set; }
    public DateTime DataEmissao { get; set; }
    public decimal ValorTotal { get; set; }
    public string Emitente { get; set; }
}