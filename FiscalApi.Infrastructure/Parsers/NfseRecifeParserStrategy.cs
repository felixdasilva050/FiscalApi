using FiscalApi.Domain.Interfaces;
using FiscalApi.Domain.ValueObjects;
using FiscalApi.Infrastructure.Helpers;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
namespace FiscalApi.Infrastructure.Parsers;

public class NfseRecifeParserStrategy : IDocumentParserStrategy
{
    public bool CanParse(string xmlContent) => xmlContent.Contains("recife.pe.gov.br");

    public FiscalDataResult Parse(string xmlContent)
    {
        var doc = XDocument.Parse(xmlContent);
            
        var numero = doc.GetValue("Numero");
        var cnpj = doc.GetValue("Cnpj");
        var valor = doc.GetValue("ValorServico").Replace(".", ",");
        
        var uniqueId = GenerateHash($"{cnpj}|{numero}|RECIFE");

        return new FiscalDataResult
        {
            UniqueId = uniqueId,
            TipoDocumento = "NFSe-Recife",
            DataEmissao = DateTime.UtcNow,
            ValorTotal = decimal.TryParse(valor, out var v) ? v : 0,
            EmitenteRaw = cnpj,
            DestinatarioRaw = "Consumidor"
        };
    }
    private string GenerateHash(string input)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        return Convert.ToBase64String(sha.ComputeHash(bytes));
    }
}