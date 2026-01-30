// Query Builder JavaScript
// Estado global da query em construção
let queryBuilder = {
    tables: [],
    selectedColumns: [],
    joins: [],
    filters: [],
    aliases: []
};

// Helper para escapar HTML e prevenir XSS
function escapeHtml(unsafe) {
    if (unsafe === null || unsafe === undefined) {
        return '';
    }
    return String(unsafe)
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}

// Helper para criar ID seguro a partir de string
function makeSafeId(str) {
    if (!str) return '';
    return String(str).replace(/[^a-zA-Z0-9_-]/g, '_');
}

// Selecionartabela
function selectTable(tableName) {
    // Verificar se já foi selecionada
    if (queryBuilder.tables.some(t => t.TableName === tableName)) {
        alert('Tabela já selecionada!');
        return;
    }

    // Adicionar à lista de tabelas selecionadas
    queryBuilder.tables.push({
        TableName: tableName,
        TableAlias: null,
        Columns: []
    });

    // Atualizar UI
    updateSelectedTablesUI();

    // Carregar colunas da tabela
    loadTableColumns(tableName);
}

// Atualizar UI de tabelas selecionadas
function updateSelectedTablesUI() {
    const container = document.getElementById('selectedTables');
    
    if (queryBuilder.tables.length === 0) {
        container.innerHTML = '<p class="text-muted">Nenhuma tabela selecionada</p>';
        return;
    }

    let html = '<div class="list-group">';
    queryBuilder.tables.forEach((table, index) => {
        html += `
            <div class="list-group-item d-flex justify-content-between align-items-center">
                <span><i class="fas fa-table"></i> ${table.TableName}</span>
                <button class="btn btn-sm btn-danger" onclick="removeTable(${index})">
                    <i class="fas fa-times"></i>
                </button>
            </div>
        `;
    });
    html += '</div>';
    
    container.innerHTML = html;
}

// Remover tabela
function removeTable(index) {
    const tableName = queryBuilder.tables[index].TableName;
    
    // Remover tabela
    queryBuilder.tables.splice(index, 1);
    
    // Remover colunas relacionadas
    queryBuilder.selectedColumns = queryBuilder.selectedColumns.filter(c => c.TableName !== tableName);
    
    // Remover JOINs relacionados
    queryBuilder.joins = queryBuilder.joins.filter(j => 
        j.LeftTable !== tableName && j.RightTable !== tableName
    );
    
    // Remover filtros relacionados
    queryBuilder.filters = queryBuilder.filters.filter(f => f.TableName !== tableName);
    
    // Atualizar UI
    updateSelectedTablesUI();
    updateColumnsUI();
}

// Carregar colunas de uma tabela
async function loadTableColumns(tableName) {
    const spinner = document.getElementById('columnsLoadingSpinner');
    const container = document.getElementById('columnsContainer');
    
    try {
        console.log('[QueryBuilder] Carregando colunas da tabela:', tableName);
        
        // Mostrar spinner
        if (spinner) spinner.classList.remove('d-none');
        if (container) container.classList.add('d-none');
        
        const url = `/QueryBuilder/GetTableColumns?tableName=${encodeURIComponent(tableName)}`;
        console.log('[QueryBuilder] URL da requisição:', url);
        
        const response = await fetch(url);
        console.log('[QueryBuilder] Response status:', response.status);
        
        if (!response.ok) {
            const errorText = await response.text();
            console.error('[QueryBuilder] Erro HTTP:', response.status, errorText);
            throw new Error(`Erro HTTP ${response.status}: ${errorText}`);
        }
        
        const columns = await response.json();
        console.log('[QueryBuilder] Colunas recebidas:', columns);
        
        // Validar se é array
        if (!Array.isArray(columns)) {
            console.error('[QueryBuilder] Resposta não é um array:', columns);
            throw new Error('Formato de resposta inválido - esperado array de colunas');
        }
        
        // Validar se array não está vazio
        if (columns.length === 0) {
            console.warn('[QueryBuilder] Tabela não possui colunas ou está vazia');
            alert(`A tabela "${tableName}" não possui colunas visíveis ou está vazia.`);
            // Não remover a tabela neste caso, apenas avisar
            const table = queryBuilder.tables.find(t => t.TableName === tableName);
            if (table) {
                table.Columns = [];
            }
            updateColumnsUI();
            return;
        }
        
        // Armazenar colunas na tabela
        const table = queryBuilder.tables.find(t => t.TableName === tableName);
        if (table) {
            table.Columns = columns;
            console.log(`[QueryBuilder] ${columns.length} colunas armazenadas para ${tableName}`);
        } else {
            console.error('[QueryBuilder] Tabela não encontrada no estado:', tableName);
            throw new Error('Tabela não encontrada no estado do builder');
        }
        
        // Atualizar UI de colunas
        updateColumnsUI();
        console.log('[QueryBuilder] UI de colunas atualizada');
        
    } catch (error) {
        console.error('[QueryBuilder] Erro ao carregar colunas:', error);
        
        // Mensagem simples e amigável para o usuário
        alert(`Erro ao carregar colunas da tabela "${tableName}". Por favor, tente novamente ou verifique os logs do console para mais detalhes.`);
        
        // Remover tabela da lista apenas para erros críticos (não para tabela vazia)
        if (error.message && !error.message.includes('vazia')) {
            const tableIndex = queryBuilder.tables.findIndex(t => t.TableName === tableName);
            if (tableIndex !== -1) {
                queryBuilder.tables.splice(tableIndex, 1);
                updateSelectedTablesUI();
            }
        }
    } finally {
        // Esconder spinner
        if (spinner) spinner.classList.add('d-none');
        if (container) container.classList.remove('d-none');
    }
}

// Atualizar UI de colunas
function updateColumnsUI() {
    const container = document.getElementById('columnsContainer');
    
    if (!container) {
        console.error('[QueryBuilder] Elemento columnsContainer não encontrado');
        return;
    }
    
    if (queryBuilder.tables.length === 0) {
        container.innerHTML = '<p class="text-muted">Selecione uma tabela para visualizar suas colunas</p>';
        return;
    }

    let html = '';
    queryBuilder.tables.forEach(table => {
        const tableNameEscaped = escapeHtml(table.TableName);
        html += `<h6 class="mt-3"><i class="fas fa-table"></i> ${tableNameEscaped}</h6>`;
        html += '<div class="row">';
        
        if (table.Columns && Array.isArray(table.Columns) && table.Columns.length > 0) {
            table.Columns.forEach(column => {
                // Validar propriedades da coluna (case-insensitive)
                const columnName = column.ColumnName || column.columnName;
                const dataType = column.DataType || column.dataType || 'Unknown';
                
                // Pular colunas sem nome
                if (!columnName) {
                    console.warn('[QueryBuilder] Coluna sem nome detectada:', column);
                    return;
                }
                
                const isSelected = queryBuilder.selectedColumns.some(
                    c => c.TableName === table.TableName && c.ColumnName === columnName
                );
                
                // Criar ID seguro para o checkbox
                const safeId = `col-${makeSafeId(table.TableName)}-${makeSafeId(columnName)}`;
                
                // Escapar valores para HTML
                const columnNameEscaped = escapeHtml(columnName);
                const dataTypeEscaped = escapeHtml(dataType);
                
                // Criar elemento usando DOM para evitar XSS
                const colDiv = document.createElement('div');
                colDiv.className = 'col-md-6 mb-2';
                colDiv.innerHTML = `
                    <div class="form-check">
                        <input class="form-check-input column-checkbox" type="checkbox" 
                               ${isSelected ? 'checked' : ''}
                               id="${safeId}"
                               data-table="${tableNameEscaped}"
                               data-column="${columnNameEscaped}">
                        <label class="form-check-label" for="${safeId}">
                            ${columnNameEscaped} <small class="text-muted">(${dataTypeEscaped})</small>
                        </label>
                    </div>
                `;
                
                if (isSelected) {
                    const aliasInput = document.createElement('input');
                    aliasInput.type = 'text';
                    aliasInput.className = 'form-control form-control-sm mt-1 column-alias';
                    aliasInput.placeholder = 'Alias (opcional)';
                    aliasInput.value = getColumnAlias(table.TableName, columnName);
                    aliasInput.dataset.table = table.TableName;
                    aliasInput.dataset.column = columnName;
                    colDiv.appendChild(aliasInput);
                }
                
                html += colDiv.outerHTML;
            });
        } else {
            html += '<p class="text-muted">Nenhuma coluna disponível ou ainda carregando...</p>';
            console.warn('[QueryBuilder] Tabela sem colunas:', table);
        }
        
        html += '</div>';
    });
    
    container.innerHTML = html;
    
    // Adicionar event listeners após inserir o HTML
    document.querySelectorAll('.column-checkbox').forEach(checkbox => {
        checkbox.addEventListener('change', function() {
            const tableName = this.dataset.table;
            const columnName = this.dataset.column;
            toggleColumn(tableName, columnName, this.checked);
        });
    });
    
    document.querySelectorAll('.column-alias').forEach(input => {
        input.addEventListener('change', function() {
            const tableName = this.dataset.table;
            const columnName = this.dataset.column;
            setColumnAlias(tableName, columnName, this.value);
        });
    });
}

// Helper para obter alias atual
function getColumnAlias(tableName, columnName) {
    const alias = queryBuilder.aliases.find(
        a => a.TableName === tableName && a.ColumnName === columnName
    );
    return alias ? alias.Alias : '';
}

// Toggle seleção de coluna
function toggleColumn(tableName, columnName, isSelected) {
    if (isSelected) {
        // Adicionar coluna
        const table = queryBuilder.tables.find(t => t.TableName === tableName);
        const column = table?.Columns.find(c => c.ColumnName === columnName);
        
        if (column) {
            queryBuilder.selectedColumns.push({
                TableName: tableName,
                ColumnName: columnName,
                DataType: column.DataType,
                IsNullable: column.IsNullable,
                IsSelected: true
            });
        }
    } else {
        // Remover coluna
        queryBuilder.selectedColumns = queryBuilder.selectedColumns.filter(
            c => !(c.TableName === tableName && c.ColumnName === columnName)
        );
        
        // Remover alias
        queryBuilder.aliases = queryBuilder.aliases.filter(
            a => !(a.TableName === tableName && a.ColumnName === columnName)
        );
    }
    
    updateColumnsUI();
}

// Definir alias de coluna
function setColumnAlias(tableName, columnName, alias) {
    // Remover alias existente
    queryBuilder.aliases = queryBuilder.aliases.filter(
        a => !(a.TableName === tableName && a.ColumnName === columnName)
    );
    
    // Adicionar novo alias se não vazio
    if (alias && alias.trim()) {
        queryBuilder.aliases.push({
            TableName: tableName,
            ColumnName: columnName,
            Alias: alias.trim()
        });
    }
}

// Adicionar JOIN
function addJoin() {
    if (queryBuilder.tables.length < 2) {
        alert('Selecione pelo menos 2 tabelas para criar um JOIN');
        return;
    }

    const container = document.getElementById('joinsContainer');
    const index = queryBuilder.joins.length;
    
    let html = `
        <div class="card mb-2" id="join-${index}">
            <div class="card-body">
                <div class="row">
                    <div class="col-md-3">
                        <label>Tabela Esquerda</label>
                        <select class="form-select form-select-sm" id="join-${index}-left-table">
                            ${queryBuilder.tables.map(t => `<option value="${t.TableName}">${t.TableName}</option>`).join('')}
                        </select>
                    </div>
                    <div class="col-md-3">
                        <label>Coluna Esquerda</label>
                        <input type="text" class="form-control form-control-sm" id="join-${index}-left-column">
                    </div>
                    <div class="col-md-2">
                        <label>Tipo JOIN</label>
                        <select class="form-select form-select-sm" id="join-${index}-type">
                            <option value="0">INNER</option>
                            <option value="1">LEFT</option>
                            <option value="2">RIGHT</option>
                            <option value="3">FULL</option>
                        </select>
                    </div>
                    <div class="col-md-3">
                        <label>Tabela Direita</label>
                        <select class="form-select form-select-sm" id="join-${index}-right-table">
                            ${queryBuilder.tables.map(t => `<option value="${t.TableName}">${t.TableName}</option>`).join('')}
                        </select>
                    </div>
                    <div class="col-md-3">
                        <label>Coluna Direita</label>
                        <input type="text" class="form-control form-control-sm" id="join-${index}-right-column">
                    </div>
                    <div class="col-md-1">
                        <label>&nbsp;</label>
                        <button class="btn btn-sm btn-danger w-100" onclick="removeJoin(${index})">
                            <i class="fas fa-times"></i>
                        </button>
                    </div>
                </div>
            </div>
        </div>
    `;
    
    if (queryBuilder.joins.length === 0) {
        container.innerHTML = html;
    } else {
        container.insertAdjacentHTML('beforeend', html);
    }
    
    queryBuilder.joins.push({
        LeftTable: '',
        LeftColumn: '',
        RightTable: '',
        RightColumn: '',
        JoinType: 0
    });
}

// Remover JOIN
function removeJoin(index) {
    queryBuilder.joins.splice(index, 1);
    document.getElementById(`join-${index}`).remove();
    
    if (queryBuilder.joins.length === 0) {
        document.getElementById('joinsContainer').innerHTML = '<p class="text-muted">Nenhum JOIN configurado</p>';
    }
}

// Adicionar filtro
function addFilter() {
    if (queryBuilder.selectedColumns.length === 0) {
        alert('Selecione pelo menos uma coluna antes de adicionar filtros');
        return;
    }

    const container = document.getElementById('filtersContainer');
    const index = queryBuilder.filters.length;
    
    let html = `
        <div class="card mb-2" id="filter-${index}">
            <div class="card-body">
                <div class="row">
                    <div class="col-md-3">
                        <label>Coluna</label>
                        <select class="form-select form-select-sm" id="filter-${index}-column">
                            ${queryBuilder.selectedColumns.map(c => 
                                `<option value="${c.TableName}|${c.ColumnName}">${c.TableName}.${c.ColumnName}</option>`
                            ).join('')}
                        </select>
                    </div>
                    <div class="col-md-2">
                        <label>Operador</label>
                        <select class="form-select form-select-sm" id="filter-${index}-operator">
                            <option value="0">=</option>
                            <option value="1">!=</option>
                            <option value="2">&gt;</option>
                            <option value="3">&lt;</option>
                            <option value="4">&gt;=</option>
                            <option value="5">&lt;=</option>
                            <option value="6">LIKE</option>
                            <option value="7">IN</option>
                            <option value="8">BETWEEN</option>
                        </select>
                    </div>
                    <div class="col-md-3">
                        <label>Valor</label>
                        <input type="text" class="form-control form-control-sm" id="filter-${index}-value">
                    </div>
                    <div class="col-md-2">
                        <label>Operador Lógico</label>
                        <select class="form-select form-select-sm" id="filter-${index}-logic">
                            <option value="0">AND</option>
                            <option value="1">OR</option>
                        </select>
                    </div>
                    <div class="col-md-2">
                        <label>&nbsp;</label>
                        <button class="btn btn-sm btn-danger w-100" onclick="removeFilter(${index})">
                            <i class="fas fa-times"></i>
                        </button>
                    </div>
                </div>
            </div>
        </div>
    `;
    
    if (queryBuilder.filters.length === 0) {
        container.innerHTML = html;
    } else {
        container.insertAdjacentHTML('beforeend', html);
    }
    
    queryBuilder.filters.push({
        Column: '',
        TableName: '',
        Operator: 0,
        Value: '',
        LogicalOperator: 0
    });
}

// Remover filtro
function removeFilter(index) {
    queryBuilder.filters.splice(index, 1);
    document.getElementById(`filter-${index}`).remove();
    
    if (queryBuilder.filters.length === 0) {
        document.getElementById('filtersContainer').innerHTML = '<p class="text-muted">Nenhum filtro configurado</p>';
    }
}

// Coletar dados dos JOINs
function collectJoinsData() {
    queryBuilder.joins.forEach((join, index) => {
        const leftTable = document.getElementById(`join-${index}-left-table`)?.value || '';
        const leftColumn = document.getElementById(`join-${index}-left-column`)?.value || '';
        const rightTable = document.getElementById(`join-${index}-right-table`)?.value || '';
        const rightColumn = document.getElementById(`join-${index}-right-column`)?.value || '';
        const joinType = parseInt(document.getElementById(`join-${index}-type`)?.value || '0');
        
        join.LeftTable = leftTable;
        join.LeftColumn = leftColumn;
        join.RightTable = rightTable;
        join.RightColumn = rightColumn;
        join.JoinType = joinType;
    });
}

// Coletar dados dos filtros
function collectFiltersData() {
    queryBuilder.filters.forEach((filter, index) => {
        const columnValue = document.getElementById(`filter-${index}-column`)?.value || '';
        const [tableName, columnName] = columnValue.split('|');
        const operator = parseInt(document.getElementById(`filter-${index}-operator`)?.value || '0');
        const value = document.getElementById(`filter-${index}-value`)?.value || '';
        const logic = parseInt(document.getElementById(`filter-${index}-logic`)?.value || '0');
        
        filter.TableName = tableName || '';
        filter.Column = columnName || '';
        filter.Operator = operator;
        filter.Value = value;
        filter.LogicalOperator = logic;
    });
}

// Gerar SQL
async function generateSql() {
    // Coletar dados dos formulários
    collectJoinsData();
    collectFiltersData();

    const queryDefinition = {
        Tables: queryBuilder.tables,
        SelectedColumns: queryBuilder.selectedColumns,
        Joins: queryBuilder.joins,
        Filters: queryBuilder.filters,
        Aliases: queryBuilder.aliases
    };

    try {
        const response = await fetch('/QueryBuilder/GenerateSql', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(queryDefinition)
        });

        const result = await response.json();
        
        if (response.ok) {
            document.getElementById('sqlPreview').innerHTML = '<code>' + result.sql + '</code>';
            // Mudar para aba SQL
            const sqlTab = new bootstrap.Tab(document.getElementById('sql-tab'));
            sqlTab.show();
        } else {
            alert('Erro ao gerar SQL: ' + (result.errors?.join(', ') || result.error));
        }
    } catch (error) {
        console.error('Erro:', error);
        alert('Erro ao gerar SQL');
    }
}

// Executar query
async function executeQuery() {
    const sql = document.getElementById('sqlPreview').innerText;
    
    if (!sql || sql.includes('Nenhuma query')) {
        alert('Gere o SQL primeiro!');
        return;
    }

    try {
        const response = await fetch('/QueryBuilder/Execute', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                sql: sql,
                savedQueryId: savedQueryId > 0 ? savedQueryId : null,
                queryName: savedQueryName || null
            })
        });

        if (response.ok) {
            // Redirecionar para página de resultados
            window.location.href = '/QueryBuilder/Execute';
        } else {
            alert('Erro ao executar query');
        }
    } catch (error) {
        console.error('Erro:', error);
        alert('Erro ao executar query');
    }
}

// Salvar query
function saveQuery() {
    const sql = document.getElementById('sqlPreview').innerText;
    
    if (!sql || sql.includes('Nenhuma query')) {
        alert('Gere o SQL primeiro!');
        return;
    }

    // Abrir modal
    const modal = new bootstrap.Modal(document.getElementById('saveQueryModal'));
    
    // Preencher com dados existentes se for edição
    if (savedQueryName) {
        document.getElementById('queryName').value = savedQueryName;
        document.getElementById('queryDescription').value = savedQueryDescription;
    }
    
    modal.show();
}

// Confirmar salvamento
async function confirmSave() {
    const name = document.getElementById('queryName').value;
    const description = document.getElementById('queryDescription').value;
    const sql = document.getElementById('sqlPreview').innerText;

    if (!name) {
        alert('Nome da query é obrigatório!');
        return;
    }

    // Coletar dados finais
    collectJoinsData();
    collectFiltersData();

    const queryDefinition = {
        Tables: queryBuilder.tables,
        SelectedColumns: queryBuilder.selectedColumns,
        Joins: queryBuilder.joins,
        Filters: queryBuilder.filters,
        Aliases: queryBuilder.aliases
    };

    try {
        const response = await fetch('/QueryBuilder/Save', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                queryId: savedQueryId > 0 ? savedQueryId : null,
                name: name,
                description: description,
                queryDefinition: queryDefinition,
                generatedSql: sql
            })
        });

        const result = await response.json();
        
        if (response.ok) {
            alert('Query salva com sucesso!');
            const modal = bootstrap.Modal.getInstance(document.getElementById('saveQueryModal'));
            modal.hide();
            
            // Redirecionar para lista de queries salvas
            window.location.href = '/SavedQueries';
        } else {
            alert('Erro ao salvar query: ' + result.error);
        }
    } catch (error) {
        console.error('Erro:', error);
        alert('Erro ao salvar query');
    }
}

// Limpar tudo
function clearAll() {
    if (confirm('Tem certeza que deseja limpar tudo?')) {
        queryBuilder = {
            tables: [],
            selectedColumns: [],
            joins: [],
            filters: [],
            aliases: []
        };
        
        updateSelectedTablesUI();
        updateColumnsUI();
        document.getElementById('joinsContainer').innerHTML = '<p class="text-muted">Nenhum JOIN configurado</p>';
        document.getElementById('filtersContainer').innerHTML = '<p class="text-muted">Nenhum filtro configurado</p>';
        document.getElementById('sqlPreview').innerHTML = '<code>-- Nenhuma query construída ainda</code>';
    }
}

// Busca de tabelas
document.addEventListener('DOMContentLoaded', function() {
    const tableSearch = document.getElementById('tableSearch');
    if (tableSearch) {
        tableSearch.addEventListener('input', function(e) {
            const searchTerm = e.target.value.toLowerCase();
            const tableItems = document.querySelectorAll('.table-item');
            
            tableItems.forEach(item => {
                const tableName = item.querySelector('button').textContent.toLowerCase();
                if (tableName.includes(searchTerm)) {
                    item.style.display = '';
                } else {
                    item.style.display = 'none';
                }
            });
        });
    }
});
