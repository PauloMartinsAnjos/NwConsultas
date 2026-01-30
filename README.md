# NwConsultas - Query Builder Visual para Firebird

Sistema de construção visual de queries SQL para o sistema legado Questor (Firebird 2.5), desenvolvido em ASP.NET MVC com C#.

## 📋 Descrição

NwConsultas é uma ferramenta que permite construir consultas SQL de forma visual sem necessidade de escrever código SQL manualmente. O sistema conecta-se ao banco Firebird 2.5 (Questor) para leitura de dados e utiliza PostgreSQL para armazenar queries salvas e histórico de execuções.

## ✨ Funcionalidades

### 🔧 Query Builder Visual
- Interface visual para seleção de tabelas e colunas
- Suporte a JOINs (INNER, LEFT, RIGHT, FULL)
- Configuração de filtros (WHERE) com operadores diversos
- Aliases personalizados para colunas
- Preview em tempo real do SQL gerado

### 💾 Gerenciamento de Queries
- Salvar queries construídas
- Listar, editar e duplicar queries salvas
- Histórico de execuções
- Busca por nome ou descrição

### 📊 Execução e Resultados
- Execução de queries com medição de performance
- Visualização de resultados em tabela responsiva
- Paginação e ordenação de dados
- Indicadores de sucesso/erro

### 📥 Exportação
- Exportar resultados para CSV
- Exportar resultados para Excel (XLSX)
- Exportar resultados para JSON
- Histórico de exportações

## 🛠️ Tecnologias Utilizadas

- **Framework**: ASP.NET MVC (.NET 8.0)
- **Linguagem**: C#
- **Frontend**: Bootstrap 5, jQuery, Font Awesome, DataTables
- **Banco de Dados Origem**: Firebird 2.5
- **Banco de Dados Armazenamento**: PostgreSQL
- **ORM**: Entity Framework Core
- **Exportação**: ClosedXML (Excel), CsvHelper (CSV), Newtonsoft.Json (JSON)

## 📦 Dependências (NuGet)

```xml
<PackageReference Include="FirebirdSql.Data.FirebirdClient" Version="10.0.0" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0" />
<PackageReference Include="ClosedXML" Version="0.102.0" />
<PackageReference Include="CsvHelper" Version="30.0.1" />
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
```

## 🚀 Instalação e Configuração

### Pré-requisitos

- .NET SDK 8.0 ou superior
- PostgreSQL 12+ (para armazenamento de queries)
- Acesso ao banco Firebird 2.5 (Questor)

### Passo 1: Clonar o Repositório

```bash
git clone https://github.com/PauloMartinsAnjos/NwConsultas.git
cd NwConsultas
```

### Passo 2: Configurar Conexões de Banco

Edite o arquivo `appsettings.json` com as credenciais corretas:

```json
{
  "ConnectionStrings": {
    "Firebird": "User=SYSDBA;Password=masterkey;Database=QUESTOR;DataSource=192.168.0.193;Port=3050;Dialect=3;Charset=NONE;Role=;Connection lifetime=15;Pooling=true;MinPoolSize=0;MaxPoolSize=50;Packet Size=8192;ServerType=0;",
    "PostgreSQL": "Host=192.168.0.40;Database=nwconsultas;Username=postgres;Password=#Rir@dm$;Port=5432;"
  }
}
```

### Passo 3: Criar Schema PostgreSQL

Execute o script de criação do schema:

```bash
psql -h 192.168.0.40 -U postgres -d nwconsultas -f Database/Scripts/CreateSchema.sql
```

Ou execute manualmente via pgAdmin ou similar.

### Passo 4: Restaurar Pacotes

```bash
dotnet restore
```

### Passo 5: Executar Aplicação

```bash
dotnet run
```

A aplicação estará disponível em `https://localhost:5001` ou `http://localhost:5000`.

## 📖 Guia de Uso

### Dashboard

A página inicial exibe:
- Total de queries salvas
- Total de execuções
- Execuções recentes
- Atalhos rápidos para principais funcionalidades

### Construindo uma Query

1. **Acesse o Query Builder** pelo menu ou dashboard
2. **Selecione Tabelas**: Clique nas tabelas disponíveis no painel esquerdo
3. **Escolha Colunas**: Marque as colunas desejadas de cada tabela
4. **Defina Aliases** (opcional): Renomeie colunas para exibição personalizada
5. **Configure JOINs** (se necessário): Adicione relacionamentos entre tabelas
6. **Adicione Filtros** (opcional): Configure condições WHERE
7. **Gere SQL**: Clique em "Gerar/Atualizar SQL" para visualizar
8. **Execute**: Clique em "Executar Query" para ver resultados
9. **Salve**: Clique em "Salvar Query" para armazenar

### Gerenciando Queries Salvas

- **Listar**: Menu "Minhas Queries" exibe todas as queries
- **Buscar**: Use o campo de busca para filtrar
- **Ver Detalhes**: Visualize SQL, histórico de execuções e exportações
- **Editar**: Carrega a query no Builder para modificação
- **Duplicar**: Cria uma cópia para usar como base
- **Excluir**: Remove a query (soft delete)

### Exportando Resultados

Após executar uma query:
1. Visualize os resultados na tabela
2. Clique em um dos botões de exportação:
   - **CSV**: Para planilhas e análise de dados
   - **Excel**: Formato .xlsx com formatação
   - **JSON**: Para integração com APIs

## 📁 Estrutura do Projeto

```
NwConsultas/
├── Controllers/
│   ├── HomeController.cs              # Dashboard
│   ├── QueryBuilderController.cs      # Construção de queries
│   ├── SavedQueriesController.cs      # Gerenciamento de queries
│   └── ExportController.cs            # Exportações
├── Models/
│   ├── QueryBuilder/                  # Modelos do builder
│   ├── Database/                      # Entities do PostgreSQL
│   └── ViewModels/                    # ViewModels
├── Services/
│   ├── FirebirdService.cs             # Conexão Firebird
│   ├── QueryBuilderService.cs         # Geração de SQL
│   └── ExportService.cs               # Exportações
├── Views/
│   ├── Home/                          # Dashboard
│   ├── QueryBuilder/                  # Interface do builder
│   ├── SavedQueries/                  # Gerenciamento
│   └── Shared/                        # Layout comum
├── Database/
│   └── Scripts/
│       └── CreateSchema.sql           # Schema PostgreSQL
└── wwwroot/
    ├── js/
    │   └── querybuilder.js            # Lógica frontend
    └── css/
```

## 🔒 Segurança

- Proteção contra SQL Injection via parametrização
- Validação de queries antes da execução
- Escape de valores especiais em filtros
- Limite de 10.000 linhas por consulta (configurável)
- Timeout de 300 segundos (5 minutos) por query

## ⚙️ Configurações

Edite `appsettings.json`:

```json
{
  "QueryBuilder": {
    "MaxResultRows": 10000,      // Limite de linhas retornadas
    "QueryTimeout": 300,          // Timeout em segundos
    "EnableQueryCache": true      // Cache de queries (futuro)
  }
}
```

## 🐛 Troubleshooting

### Erro de conexão com Firebird

- Verifique se o IP e porta estão corretos
- Confirme que o usuário/senha SYSDBA está correto
- Teste conectividade de rede: `ping 192.168.0.193`

### Erro de conexão com PostgreSQL

- Verifique se o PostgreSQL está rodando
- Confirme credenciais no `appsettings.json`
- Execute o script de criação do schema

### Queries muito lentas

- Adicione índices nas tabelas do Firebird
- Reduza o número de JOINs
- Use filtros para limitar resultados
- Ajuste `MaxResultRows` para valores menores

## 📝 Licença

Este projeto é de código aberto. Use conforme necessário.

## 👥 Contribuição

Contribuições são bem-vindas! Por favor:
1. Fork o projeto
2. Crie uma branch para sua feature
3. Commit suas mudanças
4. Push para a branch
5. Abra um Pull Request

## 📧 Suporte

Para dúvidas ou problemas, abra uma issue no GitHub.

---

**Desenvolvido para facilitar consultas ao sistema Questor (Firebird 2.5)**