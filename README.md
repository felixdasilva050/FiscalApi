# SIEG - Teste Técnico com C# .Net Core
## O desafio
1. Receba arquivos XML fiscais (NFe, CTe ou NFSe).
2. Armazene os dados conforme achar melhor — escolha entre SQL ou MongoDB
   e justifique no README.
3. Exponha endpoints REST para:
- Listar documentos com paginação e filtros (por exemplo: por data e CNPJ/UF);
- Consultar detalhes de um documento específico;
- Atualizar um documento existente;
- Excluir documentos.
4. Implemente testes unitários (pode usar FluentAssertions ou outra biblioteca
   similar).
5. Garanta idempotência: não insira dados duplicados ao receber o mesmo XML
   mais de uma vez e trate reprocessamentos de forma segura.
6. Forneça documentação mínima da API (pode ser via Swagger ou README com
   exemplos de requisições e respostas).

## 🚀 Tecnologias

- **.NET 8** (ASP.NET Core Web API)
- **MongoDB** (Persistência NoSQL)
- **Clean Architecture** (Domain, Application, Infrastructure, Api)
- **xUnit + FluentAssertions** (Testes Automatizados)
- **Swagger/OpenAPI** (Documentação Interativa)

## Requisitos
- .NET SDK 8.0+
- **MongoDB** instalado e rodando na porta **27017**

## Instruções para rodar o projeto
1. Baixe o projeto (ou clone o repo se como preferir)
2. Na raiz do projeto (onde tem um arquivo **.sln**) rode o comando ``dotnet run --project FiscalApi.Api``
3. Acesse o Swagger: http://localhost:5166/swagger

## Como testar alguns endpoints (Swagger já irá mostrá o que deve ser utilizado)
- **POST** _/api/v1/documentos_
- - Selecione um arquivo .xml via multipart/form-data.
- - A API identificará automaticamente o tipo (NFe ou NFSe) e processará.
- **GET** _/api/v1/documentos_
- - Parâmetros opcionais: **pagina**, **cnpj**.
- - Retorna a lista paginada dos documentos armazenados.

## Instruções para rodar os testes unitários
1. Baixe o projeto (caso ainda não tenha obtido no tópico anterior)
2. Na raiz do projeto (onde tem um arquivo .sln) rode o comando ``dotnet test``


## Explicação de tomadas de decisão
1. Qual SGBD escolhi e por qual motivo?
    1. Escolhi o **MongoDB** por ser a decisão mais estratégica, visando principalmente a escalabilidade e a flexibilidade de **schema**. Como lidaremos com um alto volume de notas fiscais com estruturas variadas, o modelo rígido do SQL traria uma complexidade desnecessária. Com o MongDB, ganhamos agilidade na ingestão desses dados sem precisar criar entidades para cada variação. Mesmo não sendo minha stack principal, a decisão foi pautada na arquitetura que melhor resolve o desafio.
2. Como lidei com dados sensíveis, principalmente CPF/CNPJ
    1. Optei por fazer um enconde base64 para dificultar a visualização no banco. Com isso, por mais que alguém tenha acesso ao banco, não terá uma visualização rápida dos dados.
    2. Se tivesse mais tempo acredito que “tokenizaria” o CPF e guardaria em um “key vault” de acesso altamente restrito deixando no banco principal, somente o token desse **CPF/CNPJ**. Isso claro no ambiente ideal
3. Arquitetura escolhida
    1. POO utilizando conceitos de SOLID e REST
4. O que eu faria se tivesse mais tempo?
    1. além do melhorar o tópico 2 eu com certeza melhoraria a integração entre as NF aumentando a possibilidade de mais tipos dentre as notas fiscais citadas.
    2. Testes mais robustos como de estresse de carga
    3. Sistemas de login
    4. Sistema de validação melhorada do XML
5. Como estamos falando de dados fiscais, "atualizar" um documento geralmente significa corrigir uma inconsistência de processamento ou substituição do XML. Optei por receber um novo XML > reprocessar > Atualizar os metadados no banco.
