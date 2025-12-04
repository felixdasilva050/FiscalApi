namespace FiscalApi.Application.DTOs;

public class FiscalDocumentDetalheDTO : FiscalDocumentResumoDTO
{
    public string XmlOriginal { get; set; }
    public string Destinatario { get; set; }
}