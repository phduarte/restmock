# restmock
[![.NET](https://github.com/phduarte/restmock/actions/workflows/dotnet.yml/badge.svg)](https://github.com/phduarte/restmock/actions/workflows/dotnet.yml)

Api web para criação de endpoints mockados que podem ser úteis por exemplo em testes de integração, estresse ou caos, debugging, etc.

## Glossário

### Endpoint
É o endereço de localização de um recurso web assim como as características de acesso, como tipo de método http e conteúdo de resposta.

### Mock
A tradução mais adequada é "maquete", basicamente a referência que podemos ter é que é uma cópia não funcional ou um esboço do que se espera ter. 
No caso aplicado ao propósito o que temos é uma api que se parece ao que veremos porém, sem lógica, apenas os endereços e métodos http.

## Usage

### Domínio

<table>
<tr>
 <th>campo</th>
 <th>tipo</th>
 <th>descrição</th>
 <th>exemplo</th>
</tr>
<tr>
 <td>id</td>
 <td>uuid</td>
 <td>Identificador único do endpoint. É criado automaticamente, não precisa ser informado.</td>
 <td>3fa85f64-5717-4562-b3fc-2c963f66afa6</td>
</tr>

<tr>
 <td>statusCode</td>
 <td>int</td>
 <td>Statuscode que deseja receber ao acionar esse endpoint</td>
 <td>200, 400, 404, 500</td>
</tr>
<tr>
 <td>httpMethod</td>
 <td>string</td>
 <td>Método http. Ex: GET, POST, PUT, DELETE</td>
 <td>GET</td>
</tr>
<tr>
 <td>pattern</td>
 <td>string</td>
 <td>Enedereço sem o endereço do servidor (baseurl). Pode ser usado campos variáveis dentro de colchetes.</td>
 <td>/v1/movies/fiction?year=[int]&rateMin=[int]</td>
</tr>
<tr>
 <td>processingTime</td>
 <td>int</td>
 <td>Tempo de execução do endpoint ao ser acionado. Duração em milisegundos. Campo opcional.</td>
 <td>1000</td>
</tr>
<tr>
 <td>responseBody</td>
 <td>object</td>
 <td>Resposta que o endpoint deve dar. Normalmente é um objeto Json.</td>
 <td>
 { "message": "hello world!" }
 </td>
</tr>
<tr>
 <td>contentType</td>
 <td>string</td>
 <td>Tipo de conteúdo. Deve ser um tipo compatível com os padrões <a href="https://developer.mozilla.org/en-US/docs/Web/HTTP/Basics_of_HTTP/MIME_types/Common_types" target="_blank">MIME types</a>.</td>
 <td>application/json</td>
</tr>
</table>

### POST /mocks

``` json
{
  "statusCode": 0,
  "httpMethod": "POST",
  "pattern": "string",
  "processingTime": 0,
  "responseBody": "string",
  "contentType": "string"
}
```

### GET /mocks
Busca todos os mocks existentes

### GET /mocks/{guid}
Busca os dados de um mock de endpoint

### DELETE /mocks/{guid}
Exclui um mock de endpoint existente.

## Case
Criação de um endpoint que simula um comportamento de erro de timeout para avaliarmos como o componente chamador irá lidar com esse comportamento anormal.

POST /mocks
``` json
{
  "httpMethod": "GET",
  "statuscode": 504,
  "pattern": "/timeout",
  "processingTime": 60000,
  "responseBody": {
	"statuscode": 504
  }
}
```


## Tecnologias
- Dotnet Core 7 
- Swagger
- Visual Studio 2022
