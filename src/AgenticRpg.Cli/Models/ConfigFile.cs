namespace AgenticRpg.Cli.Models
{
    public class ConfigFile
    {
        public required string BearerToken { get; init; }
        public required string ModelId { get; init; }
        public required string RegionName { get; init; }
    }
}
