namespace Dbdeploy.Core.Database
{
    public interface IDelimiterType
    {
        bool Matches(string line, string delimiter);
    }
}
