using FiscalApi.Domain.Interfaces;
namespace FiscalApi.Application.Factories;

public class ParserFactory : IParserFactory
{
    private readonly IEnumerable<IDocumentParserStrategy> _strategies;

    public ParserFactory(IEnumerable<IDocumentParserStrategy> strategies)
    {
        _strategies = strategies;
    }

    public IDocumentParserStrategy GetStrategy(string xmlContent)
    {
        var strategy = _strategies.FirstOrDefault(s => s.CanParse(xmlContent));
        return strategy ?? throw new NotSupportedException("Layout XML desconhecido.");
    }
}