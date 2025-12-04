namespace FiscalApi.Domain.ValueObjects;

public class FiscalDataResult
{
    public string UniqueId { get; set; }
    public string TipoDocumento { get; set; }
    public DateTime DataEmissao { get; set; }
    public decimal ValorTotal { get; set; }
    public string EmitenteRaw { get; set; }     // CNPJ/CPF Puro
    public string DestinatarioRaw { get; set; } // CNPJ/CPF Puro
}