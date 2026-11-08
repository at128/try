

using Microsoft.Extensions.Configuration;

namespace practise.Infrastructure.Data;

public class ShardResolver
{
    private readonly List<ShardMapEntry> _shardMap;

    public ShardResolver(IConfiguration config)
    {
        _shardMap = config.GetSection("ShardMap").GetChildren()
            .Select(s => new ShardMapEntry
            {
                ShardId = s["ShardId"] ?? string.Empty,
                ConnectionString = s["ConnectionString"] ?? string.Empty
            })
            .ToList();
    }

    public string GetConnectionStringForKey(DateTime dateTime)
    {
        var shard = _shardMap.FirstOrDefault(s => dateTime >= DateTime.Parse(s.ShardId) && dateTime < DateTime.Parse(s.ShardId).AddDays(1));
        if(shard == null)
        {
            throw new Exception($"No shard found for the given date: {dateTime}");
        }
        return shard.ConnectionString;
    }
}

public class ShardMapEntry
{
    public string ShardId { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
}