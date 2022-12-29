namespace Net.Sf.Dbdeploy.Database
{
    using System.Collections.Generic;
    using System.Text;

    public class QueryStatementSplitter
    {
        private string delimiter = ";";

        private IDelimiterType delimiterType = new NormalDelimiter();

        private string lineEnding = Database.LineEnding.Platform;

        public QueryStatementSplitter()
        {
        }

        public string Delimiter
        {
            get { return delimiter; }
            set { delimiter = value; }
        }

        public IDelimiterType DelimiterType
        {
            get { return delimiterType; }
            set { delimiterType = value; }
        }

        public string LineEnding
        {
            get { return lineEnding; }
            set { lineEnding = value; }
        }

        public virtual ICollection<string> Split(string input)
        {
            var statements = new List<string>();
            var currentSql = new StringBuilder();

            string[] lines = input.Split("\r\n".ToCharArray());

            foreach (string line in lines)
            {
                string strippedLine = line.TrimEnd();

                if (string.IsNullOrEmpty(strippedLine))
                    continue;

                if (currentSql.Length != 0)
                {
                    currentSql.Append(lineEnding);
                }

                currentSql.Append(strippedLine);

                if (delimiterType.Matches(strippedLine, delimiter))
                {
                    statements.Add(currentSql.ToString(0, currentSql.Length - delimiter.Length));

                    // Clear StringBuilder
                    currentSql.Length = 0;
                }
            }

            if (currentSql.Length != 0)
            {
                statements.Add(currentSql.ToString());
            }

            return statements;
        }
    }
}