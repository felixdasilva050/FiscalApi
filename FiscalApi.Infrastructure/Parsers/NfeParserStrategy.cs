using FiscalApi.Domain.Interfaces;
using FiscalApi.Domain.ValueObjects;
using FiscalApi.Infrastructure.Helpers;
using System.Xml.Linq;
namespace FiscalApi.Infrastructure.Parsers;

public class NfeParserStrategy : IDocumentParserStrategy
{
    public bool CanParse(string xmlContent) => xmlContent.Contains("<NFe") || xmlContent.Contains("infNFe");

    public FiscalDataResult Parse(string xmlContent)
    {
        var doc = XDocument.Parse(xmlContent);
        
        var infNfe = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "infNFe");
        var id = infNfe?.Attribute("Id")?.Value?.Replace("NFe", "") ?? Guid.NewGuid().ToString();

        return new FiscalDataResult
        {
            UniqueId = id,
            TipoDocumento = "NFe",
            DataEmissao = DateTime.UtcNow, 
            ValorTotal = decimal.Parse(doc.GetValue("vNF").Replace(".", ",") ?? "0"),
            EmitenteRaw = doc.GetValue("CNPJ"),
            DestinatarioRaw = doc.GetValue("dest")
        };
    }
}