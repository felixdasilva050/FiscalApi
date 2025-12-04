namespace FiscalApi.Domain.Interfaces;

public interface IParserFactory
{
    IDocumentParserStrategy GetStrategy(string xmlContent);
}