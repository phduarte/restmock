# CLAUDE.md — RestMock

Guia de referência para desenvolvimento e manutenção do RestMock.

## Visão geral

RestMock é um servidor HTTP de mocks em memória com persistência em arquivo JSON. Expõe dois pontos de entrada: uma API REST (`/mocks`) e uma UI Blazor Server (`/` ou `/client`).

O coração do sistema é o middleware `RewritterMiddleware`, que intercepta **todas** as requisições antes do roteamento normal. Qualquer URL que não seja uma rota reservada é comparada contra os mocks cadastrados.

---

## Arquitetura

```text
Requisição HTTP
    │
    ▼
RewritterMiddleware          ← intercepta antes do roteamento
    ├── URL em prefixo reservado? → _next() (roteamento normal)
    └── EndpointCollection.Find() → match encontrado?
            ├── Sim → ResponseTemplateProcessor.Process() → resposta
            └── Não → _next() (roteamento normal → 404)

Roteamento normal
    ├── MockController   (/mocks)       ← gerencia mocks via API
    └── Blazor Server    (/client, /)   ← UI de gerenciamento
```

### Camadas

| Camada | Pasta | Responsabilidade |
| --- | --- | --- |
| Domain | `Domain/` | Modelos, coleção em memória, processamento de templates |
| Middleware | `Middlewares/` | Interceptação e despacho das requisições mockadas |
| Controllers | `Controllers/` | API REST para CRUD de mocks |
| Services | `Services/` | Fachada usada pela UI Blazor; dispara evento `OnChange` |
| Repositories | `Repositories/` | Persistência dos mocks em `mocks.json` |
| Pages | `Pages/` | Interface Blazor Server |

---

## Arquivos-chave

| Arquivo | Papel |
| --- | --- |
| `Domain/EndpointModel.cs` | Modelo do mock + lógica de pattern matching |
| `Domain/EndpointCollection.cs` | Lista estática em memória (fonte de verdade em runtime) |
| `Domain/ResponseTemplateProcessor.cs` | Substitui `{{uuid}}` e `{{$.path}}` no response body |
| `Middlewares/RewritterMiddleware.cs` | Intercepta requisições e despacha mocks |
| `Services/MockClientService.cs` | Fachada usada pelo Blazor; persiste via `MockRepository` |
| `Repositories/MockRepository.cs` | Lê e escreve `mocks.json` no ContentRoot |
| `Controllers/MockController.cs` | API REST `/mocks` |
| `Pages/Client.razor` | UI Blazor de gerenciamento |
| `Program.cs` | Composição do app; carrega mocks persistidos na inicialização |

---

## Regras de negócio importantes

### Prefixos reservados (nunca podem ser mockados)

Definidos em `RewritterMiddleware.ProtectedPrefixes`. Qualquer URL que comece com um desses prefixos passa direto para o roteamento normal:

```text
/client  /mocks  /swagger  /_blazor  /_framework  /_content  /css  /js  /favicon
```

Se precisar adicionar uma nova rota de sistema, inclua o prefixo nessa lista.

### Pattern matching

O matching é feito em `EndpointModel.Match()` e é sempre **case-insensitive**. A ordem de avaliação:

1. Método HTTP deve ser igual
2. Path deve corresponder ao regex gerado por `BuildPathRegex()`
3. Se o pattern tem query string, cada parâmetro declarado deve estar presente com o tipo correto

Tipos suportados no path: `uuid`/`guid`, `int`/`long`/`number`, `date`, `datetime`, qualquer outro nome → `[^/]+`.
Tipos suportados na query string: `[guid]`, `[int]`, `[date]`/`[datetime]`, `[string]`.

Para adicionar um novo tipo de path, edite `EndpointModel.TypeToRegex()`.

### Response templating

Processado em `ResponseTemplateProcessor.Process()` via regex `\{\{([^}]+)\}\}`.

Variáveis suportadas:

- `{{uuid}}` → `Guid.NewGuid().ToString()`
- `{{$.caminho}}` → `JToken.SelectToken(expression)` no request body (JSONPath via Newtonsoft.Json)

Para adicionar uma nova variável, insira um novo `if` no lambda de substituição em `ResponseTemplateProcessor.Process()` antes do bloco `{{$.`.

Comportamento quando a propriedade JSONPath não existe: retorna `string.Empty` (nunca lança exceção).

### Persistência

`MockRepository` usa Newtonsoft.Json para serializar/deserializar `List<EndpointModel>` em `mocks.json` no `ContentRootPath`.

Fluxo de persistência:

- **Startup** → `Program.cs` chama `repository.LoadAll()` e popula `EndpointCollection`
- **Add/Remove** → `MockClientService` chama `repository.SaveAll()` após cada mutação

`EndpointCollection` é uma lista estática (sem DI). `MockRepository` e `MockClientService` são singletons.

Se a leitura do arquivo falhar (JSON corrompido, arquivo ausente), o sistema inicializa com lista vazia e não lança exceção.

### ResponseBody

O campo `ResponseBody` é `object?`. Pode conter:

- `string` (quando enviado pelo formulário Blazor)
- `JObject` / `JArray` (quando enviado via API com JSON aninhado)
- `null` (resposta sem body)

Sempre use `.ToString()` para serializar para a resposta HTTP. Nunca assuma o tipo concreto.

### Description

Campo `string?` opcional em `EndpointModel`. Puramente informativo — não afeta matching, templating nem persistência além do próprio valor. Exibido na linha de detalhes expandida da UI.

---

## Convenções de código

- Sem comentários óbvios; apenas onde o **porquê** não é evidente
- Sem abstrações preventivas: classes e interfaces surgem quando há segunda implementação real
- Validações apenas nas bordas (entrada de API e formulário Blazor); código interno confia nos invariantes
- `ProcessingTime` é sempre `>= 0` — o setter garante isso com `Math.Max`
- Não use `--no-verify` nem `--force-push` sem instrução explícita do usuário

---

## Como adicionar funcionalidades

### Nova variável de template (`{{foo}}`)

1. Em `ResponseTemplateProcessor.cs`, adicione um `if` no lambda:

   ```csharp
   if (expression.Equals("foo", StringComparison.OrdinalIgnoreCase))
       return /* valor */;
   ```

2. Atualize o tooltip em `Client.razor` (`<ul>` dentro do `.tooltip` do Response body)
3. Documente no README.md na tabela de variáveis

### Novo tipo de path (`{tipo}`)

1. Em `EndpointModel.TypeToRegex()`, adicione um `case`:

   ```csharp
   "foo" => @"seu-regex-aqui",
   ```

2. Atualize o tooltip em `Client.razor` (campo Pattern)
3. Documente no README.md

### Novo campo no modelo

1. Adicione a propriedade em `EndpointModel.cs`
2. Adicione o campo no formulário em `Client.razor`
3. Atualize `MockController.cs` se necessário
4. O campo será automaticamente persistido pelo `MockRepository` (Newtonsoft serializa todos os membros públicos)

---

## Desenvolvimento

```bash
# rodar localmente (abre o navegador automaticamente)
run.bat

# build sem servidor rodando
dotnet build RestMock

# o servidor deve estar parado para o build sobrescrever o executável
```

### Portas padrão

| Ambiente | URL |
| --- | --- |
| HTTP | `http://localhost:5087` |
| HTTPS | `https://localhost:7253` |

### Arquivo de mocks persistido

`RestMock/mocks.json` — criado automaticamente na primeira gravação. Pode ser deletado para limpar os mocks sem reiniciar. Não versionar este arquivo (está ou deve estar no `.gitignore`).

---

## Testes unitários

O projeto de testes fica em `RestMock.Tests/` e usa xUnit + FluentAssertions + Moq. Para executar:

```bash
dotnet test RestMock.Tests
```

**Regra obrigatória:** toda modificação de código deve vir acompanhada de testes. A cobertura deve ser mantida ou ampliada — nunca reduzida.

- **Nova lógica** → novos testes cobrindo o caminho feliz e os casos de borda
- **Bug corrigido** → teste que reproduz o bug antes da correção (e passa depois)
- **Comportamento alterado** → testes existentes atualizados para refletir o novo contrato
- **Código removido** → testes correspondentes também removidos

### Onde cada classe é testada

| Classe | Arquivo de testes |
| --- | --- |
| `EndpointModel` | `Domain/EndpointModelTests.cs` |
| `EndpointCollection` | `Domain/EndpointCollectionTests.cs` |
| `ResponseTemplateProcessor` | `Domain/ResponseTemplateProcessorTests.cs` |
| `MockClientService` | `Services/MockClientServiceTests.cs` |
| `MockRepository` | `Repositories/MockRepositoryTests.cs` |

### Detalhes de infraestrutura

- `AssemblyInfo.cs` desabilita paralelização entre classes (`DisableTestParallelization = true`) porque `EndpointCollection` usa lista estática compartilhada
- Classes que usam `EndpointCollection` implementam `IDisposable` e chamam `EndpointCollection.Clear()` no construtor e no `Dispose`
- `MockRepository` tem construtor interno que aceita um caminho direto de arquivo — use-o nos testes para evitar mockar `IWebHostEnvironment`
- `IMockRepository` é injetado em `MockClientService` — use `Mock<IMockRepository>` para isolar o serviço

---

## Contrato da API — app.http

O arquivo `app.http` na raiz do repositório contém exemplos executáveis de todas as chamadas à API REST. Ele serve como documentação viva e pode ser executado diretamente no VS Code (extensão REST Client) ou no Visual Studio.

> **Nota:** `{{uuid}}` e `{{$.x}}` no `responseBody` dos exemplos são variáveis do RestMock, não do REST Client. A extensão pode exibir avisos de "variável não encontrada", mas o request é enviado com o texto literal correto.

**Regra obrigatória:** sempre que houver:

- alteração no contrato de um endpoint existente (rota, método, campos do body, campos da resposta)
- criação de um novo endpoint no `MockController` ou em qualquer outro controller adicionado futuramente

o arquivo `app.http` **deve ser atualizado na mesma entrega**, com um exemplo representativo da mudança. Nenhuma alteração de contrato é considerada completa sem a atualização correspondente no `app.http`.

---

## O que NÃO fazer

- Não adicionar lógica de negócio dentro de `EndpointCollection` — é apenas uma lista thread-unsafe simples
- Não injetar `EndpointCollection` via DI — ela é estática por design; acesso direto é intencional
- Não mockar rotas que comecem com prefixos reservados — elas nunca chegam ao middleware de mock
- Não assumir que `ResponseBody` é `string` — pode ser `JObject` ou outro tipo após desserialização
- Não adicionar persistência em banco de dados sem discutir primeiro — o escopo do projeto é propositalmente simples (arquivo JSON)
