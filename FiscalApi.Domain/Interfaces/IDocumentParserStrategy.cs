namespace FiscalApi.Domain.Interfaces;
using FiscalApi.Domain.ValueObjects;
public interface IDocumentParserStrategy
{
    bool CanParse(string xmlContent);
    FiscalDataResult Parse(string xmlContent);
}