# Guia de Configuração Inicial - NwConsultas

Este documento fornece instruções passo a passo para configurar o NwConsultas pela primeira vez.

## 1. Configuração do PostgreSQL

### 1.1. Criar o Banco de Dados

Conecte-se ao PostgreSQL como superusuário e execute:

```sql
CREATE DATABASE nwconsultas;
```

### 1.2. Executar Script de Schema

Navegue até a pasta do projeto e execute:

```bash
psql -h 192.168.0.40 -U postgres -d nwconsultas -f Database/Scripts/CreateSchema.sql
```

**Ou** conecte-se via pgAdmin e execute o conteúdo do arquivo `Database/Scripts/CreateSchema.sql`.

### 1.3. Verificar Criação das Tabelas

Execute no PostgreSQL:

```sql
\c nwconsultas
\dt
```

Você deve ver as tabelas:
- `saved_queries`
- `query_executions`
- `query_exports`

## 2. Configuração do Firebird

### 2.1. Verificar Conectividade

Teste a conexão com o Firebird:

```bash
ping 192.168.0.193
telnet 192.168.0.193 3050
```

### 2.2. Testar Credenciais

Use um cliente Firebird (como FlameRobin ou IBExpert) para testar:
- **Host**: 192.168.0.193
- **Porta**: 3050
- **Database**: QUESTOR
- **Usuário**: SYSDBA
- **Senha**: masterkey

## 3. Configuração da Aplicação

### 3.1. Editar appsettings.json

Ajuste as strings de conexão conforme seu ambiente:

```json
{
  "ConnectionStrings": {
    "Firebird": "User=SYSDBA;Password=masterkey;Database=QUESTOR;DataSource=192.168.0.193;Port=3050;Dialect=3;Charset=NONE;",
    "PostgreSQL": "Host=192.168.0.40;Database=nwconsultas;Username=postgres;Password=#Rir@dm$;Port=5432;"
  },
  "QueryBuilder": {
    "MaxResultRows": 10000,
    "QueryTimeout": 300,
    "EnableQueryCache": true
  }
}
```

### 3.2. Restaurar Pacotes NuGet

```bash
dotnet restore
```

### 3.3. Compilar o Projeto

```bash
dotnet build
```

Se houver erros, verifique:
- Versão do .NET SDK (deve ser 8.0+)
- Pacotes NuGet instalados corretamente
- Sem erros de sintaxe no código

## 4. Executar a Aplicação

### 4.1. Modo Desenvolvimento

```bash
dotnet run
```

A aplicação estará disponível em:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`

### 4.2. Modo Produção

Para publicar a aplicação:

```bash
dotnet publish -c Release -o ./publish
```

Configure um servidor web (IIS, Nginx, Apache) para hospedar a aplicação publicada.

## 5. Teste Inicial

### 5.1. Acessar Dashboard

Abra o navegador e acesse `https://localhost:5001`

Você deve ver o dashboard com:
- Contadores zerados
- Botões para acessar funcionalidades

### 5.2. Testar Conexão Firebird

1. Clique em "Query Builder"
2. Verifique se as tabelas do Firebird aparecem no painel esquerdo
3. Se aparecer erro, verifique:
   - String de conexão Firebird
   - Conectividade de rede
   - Credenciais SYSDBA

### 5.3. Construir Query Simples

1. Selecione uma tabela (ex: a primeira da lista)
2. Marque algumas colunas
3. Clique em "Gerar/Atualizar SQL"
4. Verifique se o SQL aparece na aba "SQL Preview"
5. Clique em "Executar Query"
6. Verifique se os resultados aparecem

### 5.4. Salvar Query

1. Após executar, volte ao Query Builder
2. Clique em "Salvar Query"
3. Preencha nome e descrição
4. Clique em "Salvar"
5. Verifique se a query aparece em "Minhas Queries"

### 5.5. Testar Exportação

1. Execute uma query
2. Na página de resultados, clique em "Exportar CSV"
3. Verifique se o arquivo baixa corretamente
4. Teste também Excel e JSON

## 6. Solução de Problemas Comuns

### Erro: "Connection refused" ao Firebird

**Causa**: Firewall bloqueando porta 3050 ou servidor Firebird desligado

**Solução**:
1. Verifique se o Firebird está rodando no servidor
2. Abra a porta 3050 no firewall
3. Teste conectividade: `telnet 192.168.0.193 3050`

### Erro: "Could not connect to PostgreSQL"

**Causa**: Servidor PostgreSQL inacessível ou credenciais incorretas

**Solução**:
1. Verifique se o PostgreSQL está rodando: `systemctl status postgresql`
2. Teste conexão: `psql -h 192.168.0.40 -U postgres -d nwconsultas`
3. Verifique senha no `appsettings.json`

### Erro: "Table doesn't exist" no PostgreSQL

**Causa**: Schema não foi criado

**Solução**:
Execute o script de criação:
```bash
psql -h 192.168.0.40 -U postgres -d nwconsultas -f Database/Scripts/CreateSchema.sql
```

### Erro: "Timeout expired" ao executar query

**Causa**: Query muito pesada ou banco lento

**Solução**:
1. Adicione filtros para limitar resultados
2. Aumente o timeout em `appsettings.json`:
```json
"QueryBuilder": {
    "QueryTimeout": 600
}
```
3. Otimize a query com índices no Firebird

### Nenhuma tabela aparece no Query Builder

**Causa**: String de conexão Firebird incorreta ou permissões

**Solução**:
1. Verifique o nome do database: deve ser exatamente "QUESTOR"
2. Teste com um cliente Firebird externo
3. Verifique logs da aplicação para detalhes do erro

## 7. Configurações Avançadas

### 7.1. Alterar Porta da Aplicação

Edite `Properties/launchSettings.json`:

```json
{
  "applicationUrl": "https://localhost:7001;http://localhost:7000"
}
```

### 7.2. Configurar Logging

Edite `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "NwConsultas": "Debug"
    }
  }
}
```

### 7.3. Habilitar HTTPS em Produção

Configure certificado SSL no servidor web (IIS/Nginx/Apache).

## 8. Próximos Passos

Após configuração bem-sucedida:

1. ✅ Criar queries de exemplo para usuários
2. ✅ Documentar tabelas e relacionamentos do Firebird
3. ✅ Treinar usuários finais
4. ✅ Configurar backup do PostgreSQL
5. ✅ Monitorar performance e logs

## 9. Suporte

Em caso de problemas não resolvidos por este guia:

1. Verifique os logs em: `bin/Debug/net8.0/` ou `bin/Release/net8.0/`
2. Abra uma issue no GitHub com detalhes do erro
3. Inclua versões de .NET, PostgreSQL e Firebird utilizadas

---

**Data de última atualização**: Janeiro 2026
