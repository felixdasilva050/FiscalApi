using FiscalApi.Domain.Entities;
using FiscalApi.Domain.Interfaces;
using System.Text;
namespace FiscalApi.Application.Services;

public interface IFiscalDocumentService
{
    Task ProcessarXmlAsync(string xmlContent);
    Task<IEnumerable<FiscalDocument>> ListarAsync(int page, int size, DateTime? inicio, string? cnpj);
}

public class FiscalDocumentService : IFiscalDocumentService
{
    private readonly IFiscalDocumentRepository _repository;
    private readonly IParserFactory _parserFactory;

    public FiscalDocumentService(IFiscalDocumentRepository repository, IParserFactory parserFactory)
    {
        _repository = repository;
        _parserFactory = parserFactory;
    }

    public async Task ProcessarXmlAsync(string xmlContent)
    {
        // 1. Escolhe estratégia (OCP)
        var strategy = _parserFactory.GetStrategy(xmlContent);
            
        // 2. Faz o parse
        var dados = strategy.Parse(xmlContent);

        // 3. Encode de segurança (Base64 como solicitado)
        var emitenteEnc = Convert.ToBase64String(Encoding.UTF8.GetBytes(dados.EmitenteRaw ?? ""));
        var destEnc = Convert.ToBase64String(Encoding.UTF8.GetBytes(dados.DestinatarioRaw ?? ""));

        // 4. Cria Entidade
        var doc = new FiscalDocument(
            dados.UniqueId, 
            xmlContent, 
            dados.TipoDocumento, 
            dados.DataEmissao, 
            emitenteEnc, 
            destEnc, 
            dados.ValorTotal
        );

        // 5. Salva (Idempotente)
        await _repository.UpsertAsync(doc);
    }

    public async Task<IEnumerable<FiscalDocument>> ListarAsync(int page, int size, DateTime? inicio, string? cnpj)
    {
        return await _repository.GetAllAsync(page, size, inicio, cnpj);
    }
}