# Decision Log

Registro das decisões técnicas do NexCommerce. Cada entrada documenta o contexto,
a alternativa considerada e o trade-off aceito.

|  #  |           Decisão              |   Data  | Status |
|-----|--------------------------------|---------|--------|
| 001 | .NET 9 fixado via global.json  | 2026-07 | Aceita |
| 002 | FluentAssertions 6.12.2        | 2026-07 | Aceita |
| 003 | Value Objects não implementados| 2026-07 | Aceita |
| 004 | Guid v7 como chave primária    | 2026-07 | Aceita |
| 005 | SHA-256 no TokenHash           | 2026-07 | Aceita |
| 006 | Snapshot de preço em OrderItem | 2026-07 | Aceita |
| 007 | TimeProvider adiado            | 2026-07 | Adiada |

---

## ADR-001: .NET 9 fixado via global.json

**Contexto:** a máquina de desenvolvimento tinha apenas o SDK 10.0.109. O template
`webapi` gerou `net10.0` e o build falhou (`NETSDK1226`): o SDK 10 não possui os
dados de pruning do `Microsoft.AspNetCore.App`.

**Decisão:** fixar o SDK em `9.0.315` com `rollForward: latestFeature`.

**Alternativa considerada:** permanecer no .NET 10 e definir
`AllowMissingPrunePackageData`. Descartada: silencia o sintoma sem resolver a
incompatibilidade de pacotes de terceiros (EF Core, MediatR, Stripe.net) ainda
não publicados para o 10.

**Trade-off:**
- Ganho: build reproduzível, ecossistema NuGet estável, CI/CD previsível.
- Custo: quem clonar o repositório precisa do SDK 9 instalado.

---

## ADR-002: FluentAssertions fixado em 6.12.2

**Contexto:** a versão 7 mudou para licença comercial paga após aquisição do
projeto pela Xceed. A 6.12.2 é a última sob Apache 2.0.

**Decisão:** pinar a versão 6.12.2 nos projetos de teste.

**Alternativa considerada:** migrar para AwesomeAssertions (fork livre da v6) ou
usar os asserts nativos do xUnit. Descartada por ora: a 6.12.2 é estável e mantém
a sintaxe mais reconhecida do ecossistema .NET.

**Trade-off:**
- Ganho: zero custo de licença, sem dívida jurídica em uso comercial.
- Custo: versão congelada, sem receber melhorias futuras da biblioteca.

---

## ADR-003: Value Objects não implementados

**Contexto:** a ordem de implementação prevê Value Objects na camada Domain.
Os candidatos naturais eram `Money` (amount + currency) e `Email`.

**Decisão:** não implementar. `Price` permanece `decimal` e `Email` permanece
`string`, com normalização e validação dentro de `User.Create`.

**Alternativa considerada:** `Money` como VO. Descartada: o projeto opera em
moeda única. Um VO de moeda sem multi-moeda encapsula um problema que não existe,
e exigiria mapeamento `ComplexProperty` no EF Core sem retorno prático.

**Trade-off:**
- Ganho: menos cerimônia, mapeamento EF Core direto, entidades legíveis.
- Custo: a invariante de e-mail vive na entidade, não em um tipo reutilizável.
  Se surgir e-mail em outro agregado, a validação precisa ser extraída.

**Revisar quando:** houver multi-moeda ou e-mail em mais de um agregado.

---

## ADR-004: Guid v7 como chave primária

**Contexto:** entidades precisam de identidade única. `Guid.NewGuid()` gera
valores aleatórios, causando fragmentação no índice clusterizado do SQL Server.

**Decisão:** `Guid.CreateVersion7()` (.NET 9) no `BaseEntity`, gerando GUIDs
ordenáveis por timestamp.

**Alternativa considerada:** `int IDENTITY`. Descartada: exige round-trip ao
banco para obter o ID e expõe contagem de registros quando usado em URL
(enumeração de recursos).

**Trade-off:**
- Ganho: ID gerado na aplicação, sem round-trip; seguro para expor em rota;
  inserção sequencial preserva a ordem física do índice.
- Custo: 16 bytes contra 4 do `int`, encarecendo todo índice não clusterizado.

---

## ADR-005: SHA-256 no TokenHash, BCrypt no PasswordHash

**Contexto:** senha e refresh token são credenciais persistidas, mas com padrões
de acesso opostos. Senha é verificada contra um candidato conhecido; refresh
token precisa ser **localizado** no banco a partir do valor recebido.

**Decisão:** dois contratos separados no Domain: `IPasswordHasher` (hash lento
com salt) e `ITokenHasher` (SHA-256 determinístico).

**Alternativa considerada:** BCrypt para ambos. Descartada: o salt aleatório do
BCrypt produz hash diferente a cada chamada, impossibilitando
`WHERE TokenHash = @hash`. A validação viraria table scan comparando linha a
linha a ~100ms cada.

**Trade-off:**
- Ganho: lookup do refresh token em índice único, O(log n); vazamento do banco
  não expõe token utilizável.
- Custo: SHA-256 sem salt é teoricamente vulnerável a rainbow table. Aceitável
  porque o token é aleatório de 256 bits, não uma senha humana adivinhável.

  ---

## ADR-006: Snapshot de preço e nome em OrderItem

**Contexto:** um item de pedido referencia um produto que pode mudar de preço,
nome ou ser desativado depois da compra.

**Decisão:** `OrderItem` copia `ProductName` e `UnitPrice` no momento da criação.
O `ProductId` é mantido apenas como referência.

**Alternativa considerada:** navegar até `Product` para obter preço e nome.
Descartada: quebraria o histórico (pedido antigo exibiria o preço atual) e
geraria JOIN ou N+1 em toda listagem de pedidos.

**Trade-off:**
- Ganho: histórico imutável e correto; listagem de pedidos sem JOIN em Products.
- Custo: duplicação de dados. Correção de digitação no nome do produto não se
  propaga a pedidos já emitidos.