namespace Dbdeploy.Powershell
{
    using System;
    using System.IO;
    using System.Text;

    public class LambdaTextWriter : TextWriter
    {
        private readonly Action<string> writer;
        private UnicodeEncoding encoding;

        public override Encoding Encoding
        {
            get
            {
                if (encoding == null)
                {
                    encoding = new UnicodeEncoding(false, false);
                }
                return encoding;
            }
        }

        public LambdaTextWriter(Action<string> writer)
        {
            this.writer = writer;
        }


        public LambdaTextWriter(IFormatProvider formatProvider, Action<string> writer)
            : base(formatProvider)
        {
            this.writer = writer;
        }

        public override void Write(string value)
        {
            writer(value);
        }

        public override void Write(char[] buffer, int index, int count)
        {
            Write(new string(buffer, index, count));
        }
    }
}