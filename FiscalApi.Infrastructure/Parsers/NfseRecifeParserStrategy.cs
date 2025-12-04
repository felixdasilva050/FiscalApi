using FiscalApi.Domain.Interfaces;
using FiscalApi.Domain.ValueObjects;
using FiscalApi.Infrastructure.Helpers;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
namespace FiscalApi.Infrastructure.Parsers;

public class NfseRecifeParserStrategy : IDocumentParserStrategy
{
    public bool CanParse(string xmlContent)
    {
        if (string.IsNullOrWhiteSpace(xmlContent)) return false;

        bool temNamespaceRecife = xmlContent.Contains("recife.pe.gov.br", StringComparison.OrdinalIgnoreCase);
        
        bool temEstruturaRecife = xmlContent.Contains("<Nfse", StringComparison.OrdinalIgnoreCase) 
                               && !xmlContent.Contains("<CompNfse", StringComparison.OrdinalIgnoreCase);

        return temNamespaceRecife || temEstruturaRecife;
    }

    public FiscalDataResult Parse(string xmlContent)
    {
        try 
        {
            var doc = XDocument.Parse(xmlContent);
            
            var numero = doc.GetValue("Numero");
            var cnpj = doc.GetValue("Cnpj");
            
            var valorStr = doc.GetValue("ValorServico").Replace(".", ",");
            
            var dataStr = doc.GetValue("DataEmissao");
            DateTime.TryParse(dataStr, out var dataEmissao);
            if (dataEmissao == DateTime.MinValue) dataEmissao = DateTime.UtcNow;

            decimal.TryParse(valorStr, out var valorTotal);

            var uniqueId = GenerateHash($"{cnpj}|{numero}|RECIFE");

            return new FiscalDataResult
            {
                UniqueId = uniqueId,
                TipoDocumento = "NFSe-Recife",
                DataEmissao = dataEmissao,
                ValorTotal = valorTotal,
                EmitenteRaw = cnpj,
                DestinatarioRaw = "Tomador Diversos"
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao ler XML Recife: {ex.Message}");
        }
    }

    private string GenerateHash(string input)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha.ComputeHash(bytes);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}