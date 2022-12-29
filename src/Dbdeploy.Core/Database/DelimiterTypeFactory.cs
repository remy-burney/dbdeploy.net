namespace Dbdeploy.Core.Database
{
    public static class DelimiterTypeFactory
    {
        public static IDelimiterType Create(string type)
        {
            switch ((type ?? string.Empty).ToUpperInvariant())
            {
                case "ROW":
                    return new RowDelimiter();

                case "NORMAL":
                default:
                    return new NormalDelimiter();
            }
        }
    }
}
