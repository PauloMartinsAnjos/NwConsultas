-- Script para criar o schema do banco PostgreSQL para NwConsultas
-- Database: nwconsultas
-- Host: 192.168.0.40

-- Criar o banco de dados (executar como superuser)
-- CREATE DATABASE nwconsultas;
-- \c nwconsultas;

-- Queries salvas
CREATE TABLE IF NOT EXISTS saved_queries (
    id SERIAL PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    description TEXT,
    query_json JSONB NOT NULL,
    sql_generated TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    created_by VARCHAR(100) DEFAULT 'system',
    is_active BOOLEAN DEFAULT TRUE
);

-- Histórico de execuções
CREATE TABLE IF NOT EXISTS query_executions (
    id SERIAL PRIMARY KEY,
    saved_query_id INTEGER REFERENCES saved_queries(id) ON DELETE CASCADE,
    executed_at TIMESTAMP DEFAULT NOW(),
    execution_time_ms INTEGER,
    rows_returned INTEGER,
    executed_by VARCHAR(100) DEFAULT 'system',
    success BOOLEAN DEFAULT TRUE,
    error_message TEXT
);

-- Exportações
CREATE TABLE IF NOT EXISTS query_exports (
    id SERIAL PRIMARY KEY,
    saved_query_id INTEGER REFERENCES saved_queries(id) ON DELETE SET NULL,
    export_format VARCHAR(20) NOT NULL,
    file_name VARCHAR(255),
    exported_at TIMESTAMP DEFAULT NOW(),
    exported_by VARCHAR(100) DEFAULT 'system',
    row_count INTEGER
);

-- Índices para performance
CREATE INDEX IF NOT EXISTS idx_saved_queries_name ON saved_queries(name);
CREATE INDEX IF NOT EXISTS idx_query_executions_query_id ON query_executions(saved_query_id);
CREATE INDEX IF NOT EXISTS idx_query_executions_date ON query_executions(executed_at DESC);

-- Comentários nas tabelas
COMMENT ON TABLE saved_queries IS 'Armazena queries construídas visualmente pelo usuário';
COMMENT ON TABLE query_executions IS 'Histórico de execuções de queries';
COMMENT ON TABLE query_exports IS 'Histórico de exportações de resultados';
