namespace FiscalApi.Domain.Entities;
using System;
public class FiscalDocument
{
    // O Id será a Chave de Acesso (NFe) ou o Hash Gerado (NFSe)
    public string Id { get; private set; } 
    public string XmlOriginal { get; private set; }
    public string TipoDocumento { get; private set; } // NFe, CTe, NFSe-Recife, etc.
    public DateTime DataEmissao { get; private set; }
        
    // Dados sensíveis encodados (Base64 - Regra de Negócio)
    public string EmitenteDocEncodado { get; private set; } 
    public string DestinatarioDocEncodado { get; private set; }
        
    public decimal ValorTotal { get; private set; }

    protected FiscalDocument() { }

    public FiscalDocument(string id, string xml, string tipo, DateTime data, string emitente, string destinatario, decimal valor)
    {
        if (string.IsNullOrEmpty(id)) throw new ArgumentException("ID do documento é obrigatório.");
            
        Id = id;
        XmlOriginal = xml;
        TipoDocumento = tipo;
        DataEmissao = data;
        EmitenteDocEncodado = emitente;
        DestinatarioDocEncodado = destinatario;
        ValorTotal = valor;
    }

    public void AtualizarXml(string novoXml) => XmlOriginal = novoXml;
}