using System.Text;

namespace AIService.Services
{
    public record ColumnInfo(string ColumnName, string DataType, string? Description);
    public record TableSchema(string TableName, List<ColumnInfo> Columns);

    public interface ISchemaService
    {
        Task<List<TableSchema>> GetAllSchemasAsync();
        Task<string> BuildSchemaContextAsync();
    }

    public class SchemaService : ISchemaService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SchemaService> _logger;

        public SchemaService(IConfiguration configuration, ILogger<SchemaService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<List<TableSchema>> GetAllSchemasAsync()
        {
            var schemas = new List<TableSchema>();
            try
            {
                var cs = _configuration.GetConnectionString("DefaultConnection")!;
                await using var conn = new Npgsql.NpgsqlConnection(cs);
                await conn.OpenAsync();

                // Fetch user tables
                var tables = new List<string>();
                await using (var cmd = new Npgsql.NpgsqlCommand(
                    "SELECT table_name FROM information_schema.tables " +
                    "WHERE table_schema = 'public' AND table_type = 'BASE TABLE' " +
                    "ORDER BY table_name", conn))
                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                        tables.Add(reader.GetString(0));
                }

                // Fetch columns for each table
                foreach (var table in tables)
                {
                    var columns = new List<ColumnInfo>();
                    await using var colCmd = new Npgsql.NpgsqlCommand(@"
                        SELECT c.column_name, c.data_type, pgd.description
                        FROM information_schema.columns c
                        LEFT JOIN pg_catalog.pg_statio_all_tables st
                               ON st.relname = c.table_name AND st.schemaname = 'public'
                        LEFT JOIN pg_catalog.pg_description pgd
                               ON pgd.objoid = st.relid AND pgd.objsubid = c.ordinal_position
                        WHERE c.table_name = @t AND c.table_schema = 'public'
                        ORDER BY c.ordinal_position", conn);
                    colCmd.Parameters.AddWithValue("t", table);

                    await using var colReader = await colCmd.ExecuteReaderAsync();
                    while (await colReader.ReadAsync())
                    {
                        columns.Add(new ColumnInfo(
                            colReader.GetString(0),
                            colReader.GetString(1),
                            colReader.IsDBNull(2) ? null : colReader.GetString(2)));
                    }

                    schemas.Add(new TableSchema(table, columns));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching database schema");
            }

            return schemas;
        }

        public async Task<string> BuildSchemaContextAsync()
        {
            var schemas = await GetAllSchemasAsync();
            var sb = new StringBuilder();
            sb.AppendLine("PostgreSQL schema (public):");
            sb.AppendLine();

            foreach (var table in schemas)
            {
                sb.AppendLine($"Table: {table.TableName}");
                foreach (var col in table.Columns)
                {
                    var desc = col.Description is not null ? $"  -- {col.Description}" : "";
                    sb.AppendLine($"  {col.ColumnName} {col.DataType}{desc}");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
