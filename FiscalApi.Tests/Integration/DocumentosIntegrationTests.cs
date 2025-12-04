using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Xunit;
namespace FiscalApi.Tests.Integration;

public class DocumentosIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public DocumentosIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Upload_XmlRecife_DeveRetornarSucesso()
    {
        var xmlRecife = @"<Nfse xmlns=""http://nfse.recife.pe.gov.br"">
                        <Numero>999</Numero>
                        <Cnpj>12345678000199</Cnpj>
                        <ValorServico>150.00</ValorServico>
                        <DataEmissao>2023-10-25</DataEmissao>
                      </Nfse>";
    
        // simulando um arquivo xml
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xmlRecife));
        
        var content = new MultipartFormDataContent();
        content.Add(new StreamContent(stream), "arquivo", "nota_recife.xml");

        var response = await _client.PostAsync("/api/v1/documentos", content);
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task Upload_XmlInvalido_DeveRetornarErro()
    {
        
        var xmlLixo = "<lixo>Isso nao é uma nota fiscal</lixo>";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xmlLixo));
    
        var content = new MultipartFormDataContent();
        
        //Vamos simular aqui o envio do arquivo
        content.Add(new StreamContent(stream), "arquivo", "teste_invalido.xml");
        
        var response = await _client.PostAsync("/api/v1/documentos", content);
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }
}