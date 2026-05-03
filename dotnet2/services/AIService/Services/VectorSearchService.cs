using Pgvector;

namespace AIService.Services
{
    public record VectorSearchItem(string EntityType, string EntityName, string Description, double Score);

    public interface IVectorSearchService
    {
        Task StoreEmbeddingAsync(string entityType, string entityName, string description, float[] embedding);
        Task<List<VectorSearchItem>> SearchAsync(float[] queryEmbedding, int topK = 5);
    }

    public class VectorSearchService : IVectorSearchService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<VectorSearchService> _logger;

        public VectorSearchService(IConfiguration configuration, ILogger<VectorSearchService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private Npgsql.NpgsqlDataSource BuildDataSource()
        {
            var cs = _configuration.GetConnectionString("DefaultConnection")!;
            var dsBuilder = new Npgsql.NpgsqlDataSourceBuilder(cs);
            dsBuilder.UseVector();
            return dsBuilder.Build();
        }

        public async Task StoreEmbeddingAsync(string entityType, string entityName, string description, float[] embedding)
        {
            await using var ds = BuildDataSource();
            await using var conn = await ds.OpenConnectionAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO metadata_embeddings (entity_type, entity_name, description, embedding)
                VALUES ($1, $2, $3, $4)
                ON CONFLICT (entity_type, entity_name)
                DO UPDATE SET description = EXCLUDED.description,
                              embedding   = EXCLUDED.embedding,
                              updated_at  = CURRENT_TIMESTAMP";

            cmd.Parameters.AddWithValue(entityType);
            cmd.Parameters.AddWithValue(entityName);
            cmd.Parameters.AddWithValue(description);
            cmd.Parameters.AddWithValue(new Vector(embedding));

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<VectorSearchItem>> SearchAsync(float[] queryEmbedding, int topK = 5)
        {
            var results = new List<VectorSearchItem>();

            await using var ds = BuildDataSource();
            await using var conn = await ds.OpenConnectionAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT entity_type, entity_name, description,
                       1 - (embedding <=> $1) AS score
                FROM metadata_embeddings
                ORDER BY embedding <=> $1
                LIMIT $2";

            cmd.Parameters.AddWithValue(new Vector(queryEmbedding));
            cmd.Parameters.AddWithValue(topK);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                results.Add(new VectorSearchItem(
                    reader.GetString(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetDouble(3)));
            }

            return results;
        }
    }
}
