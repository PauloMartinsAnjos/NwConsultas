# Exemplos de Queries - NwConsultas

Este documento contém exemplos práticos de queries que podem ser construídas usando o Query Builder.

## Exemplo 1: Listagem Simples de Clientes

### Objetivo
Listar todos os clientes ativos com seus dados básicos.

### Passos no Query Builder

1. **Selecionar Tabela**: CLIENTES
2. **Colunas**:
   - ID
   - NOME (alias: "Nome do Cliente")
   - CPF_CNPJ (alias: "CPF/CNPJ")
   - EMAIL
   - TELEFONE
3. **Filtro**: ATIVO = 'S'

### SQL Gerado
```sql
SELECT 
    CLIENTES.ID,
    CLIENTES.NOME AS "Nome do Cliente",
    CLIENTES.CPF_CNPJ AS "CPF/CNPJ",
    CLIENTES.EMAIL,
    CLIENTES.TELEFONE
FROM CLIENTES
WHERE CLIENTES.ATIVO = 'S'
```

---

## Exemplo 2: Pedidos com Informações do Cliente

### Objetivo
Listar pedidos recentes com dados do cliente.

### Passos no Query Builder

1. **Tabelas**: 
   - PEDIDOS (principal)
   - CLIENTES
2. **JOIN**: 
   - INNER JOIN CLIENTES ON PEDIDOS.CLIENTE_ID = CLIENTES.ID
3. **Colunas**:
   - PEDIDOS.ID (alias: "Nº Pedido")
   - PEDIDOS.DATA_PEDIDO (alias: "Data")
   - CLIENTES.NOME (alias: "Cliente")
   - PEDIDOS.VALOR_TOTAL (alias: "Valor Total")
4. **Filtros**:
   - PEDIDOS.DATA_PEDIDO >= '2024-01-01'
   - AND PEDIDOS.STATUS = 'APROVADO'

### SQL Gerado
```sql
SELECT 
    PEDIDOS.ID AS "Nº Pedido",
    PEDIDOS.DATA_PEDIDO AS "Data",
    CLIENTES.NOME AS "Cliente",
    PEDIDOS.VALOR_TOTAL AS "Valor Total"
FROM PEDIDOS
INNER JOIN CLIENTES ON PEDIDOS.CLIENTE_ID = CLIENTES.ID
WHERE PEDIDOS.DATA_PEDIDO >= '2024-01-01'
AND PEDIDOS.STATUS = 'APROVADO'
```

---

## Exemplo 3: Produtos com Estoque Baixo

### Objetivo
Identificar produtos que precisam de reposição.

### Passos no Query Builder

1. **Tabelas**:
   - PRODUTOS (principal)
   - CATEGORIAS
2. **JOIN**:
   - LEFT JOIN CATEGORIAS ON PRODUTOS.CATEGORIA_ID = CATEGORIAS.ID
3. **Colunas**:
   - PRODUTOS.CODIGO (alias: "Código")
   - PRODUTOS.DESCRICAO (alias: "Produto")
   - CATEGORIAS.NOME (alias: "Categoria")
   - PRODUTOS.ESTOQUE_ATUAL (alias: "Estoque")
   - PRODUTOS.ESTOQUE_MINIMO (alias: "Estoque Mínimo")
4. **Filtros**:
   - PRODUTOS.ESTOQUE_ATUAL < PRODUTOS.ESTOQUE_MINIMO
   - AND PRODUTOS.ATIVO = 'S'

### SQL Gerado
```sql
SELECT 
    PRODUTOS.CODIGO AS "Código",
    PRODUTOS.DESCRICAO AS "Produto",
    CATEGORIAS.NOME AS "Categoria",
    PRODUTOS.ESTOQUE_ATUAL AS "Estoque",
    PRODUTOS.ESTOQUE_MINIMO AS "Estoque Mínimo"
FROM PRODUTOS
LEFT JOIN CATEGORIAS ON PRODUTOS.CATEGORIA_ID = CATEGORIAS.ID
WHERE PRODUTOS.ESTOQUE_ATUAL < PRODUTOS.ESTOQUE_MINIMO
AND PRODUTOS.ATIVO = 'S'
```

---

## Exemplo 4: Vendas por Vendedor (Agregação)

### Objetivo
Totalizar vendas por vendedor em um período.

### Nota
⚠️ Para agregações (SUM, COUNT, etc), é necessário escrever SQL manualmente. O Query Builder pode gerar a estrutura base.

### Passos no Query Builder

1. Construa a query básica com as tabelas e JOINs
2. Copie o SQL gerado
3. Adicione manualmente as funções de agregação
4. Execute na aba "SQL Preview"

### SQL Manual (após gerar base)
```sql
SELECT 
    VENDEDORES.NOME AS "Vendedor",
    COUNT(PEDIDOS.ID) AS "Total de Pedidos",
    SUM(PEDIDOS.VALOR_TOTAL) AS "Valor Total Vendido"
FROM PEDIDOS
INNER JOIN VENDEDORES ON PEDIDOS.VENDEDOR_ID = VENDEDORES.ID
WHERE PEDIDOS.DATA_PEDIDO BETWEEN '2024-01-01' AND '2024-12-31'
GROUP BY VENDEDORES.NOME
ORDER BY SUM(PEDIDOS.VALOR_TOTAL) DESC
```

---

## Exemplo 5: Clientes sem Pedidos (LEFT JOIN)

### Objetivo
Identificar clientes cadastrados que nunca fizeram pedidos.

### Passos no Query Builder

1. **Tabelas**:
   - CLIENTES (principal)
   - PEDIDOS
2. **JOIN**:
   - LEFT JOIN PEDIDOS ON CLIENTES.ID = PEDIDOS.CLIENTE_ID
3. **Colunas**:
   - CLIENTES.ID
   - CLIENTES.NOME (alias: "Cliente")
   - CLIENTES.EMAIL
   - CLIENTES.DATA_CADASTRO (alias: "Cadastrado em")
4. **Filtro**:
   - PEDIDOS.ID IS NULL (adicionar manualmente no SQL)

### SQL Gerado (ajustado manualmente)
```sql
SELECT 
    CLIENTES.ID,
    CLIENTES.NOME AS "Cliente",
    CLIENTES.EMAIL,
    CLIENTES.DATA_CADASTRO AS "Cadastrado em"
FROM CLIENTES
LEFT JOIN PEDIDOS ON CLIENTES.ID = PEDIDOS.CLIENTE_ID
WHERE PEDIDOS.ID IS NULL
```

---

## Dicas para Queries Eficientes

### 1. Use Filtros Sempre que Possível
- Evite retornar todas as linhas de tabelas grandes
- Use datas para limitar períodos
- Filtre por status quando disponível

### 2. Limite de Resultados
- Configure `MaxResultRows` apropriadamente
- Para análises, exporte e processe offline

### 3. Índices
- Verifique se colunas usadas em JOINs têm índices
- Colunas em WHERE devem ter índices para melhor performance

### 4. JOINs
- Use INNER JOIN quando precisar apenas de registros relacionados
- Use LEFT JOIN quando quiser incluir registros sem relacionamento
- Evite múltiplos JOINs desnecessários

### 5. Aliases
- Use nomes descritivos para facilitar leitura
- Evite aliases muito longos
- Mantenha consistência nos nomes

---

## Salvando Queries de Exemplo

Recomenda-se salvar as queries acima com nomes descritivos:

- "Clientes Ativos - Lista Completa"
- "Pedidos Aprovados 2024"
- "Produtos em Falta no Estoque"
- "Vendas por Vendedor 2024"
- "Clientes Inativos (Sem Pedidos)"

Isso facilita o reuso e compartilhamento entre usuários.

---

## Próximos Exemplos a Serem Adicionados

Conforme o uso do sistema, documente queries úteis:

- [ ] Relatório de faturamento mensal
- [ ] Produtos mais vendidos
- [ ] Clientes inadimplentes
- [ ] Histórico de alterações de preços
- [ ] Pedidos pendentes de entrega

---

**Última atualização**: Janeiro 2026
