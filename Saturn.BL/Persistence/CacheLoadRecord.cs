namespace Saturn.BL.Persistence
{
    public record CacheLoadRecord
    {
        public CacheLoadRecord(string typeName, string fileName)
        {
            TypeName = typeName;
            FileName = fileName;
        }

        public string TypeName { get; }
        public string FileName { get; }
    }
}
