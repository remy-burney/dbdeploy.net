using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Dbdeploy.Core.Exceptions;

namespace Dbdeploy.Core.Scripts
{
    public class DirectoryScanner
    {
        private readonly FilenameParser filenameParser;

        private readonly TextWriter infoTextWriter;

        private readonly Encoding encoding;

        public DirectoryScanner(TextWriter infoTextWriter, Encoding encoding)
        {
            filenameParser = new FilenameParser();
            
            this.infoTextWriter = infoTextWriter;
            this.encoding = encoding;
        }

        public List<ChangeScript> GetChangeScriptsForDirectory(DirectoryInfo directory)
        {
            if (directory == null)
                throw new ArgumentNullException("directory");

            try
            {
                infoTextWriter.WriteLine("Reading change scripts from directory '" + directory.FullName + "'...");
            }
            catch (IOException)
            {
                // ignore
            }

            List<ChangeScript> scripts = new List<ChangeScript>();

            foreach (FileInfo file in directory.GetFiles("*.*", SearchOption.AllDirectories))
            {
                if ((file.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                    continue;

                string filename = file.Name;

                try
                {
                    int scriptNumber = filenameParser.ExtractScriptNumberFromFilename(filename);

                    scripts.Add(new ChangeScript(file.Directory.Name, scriptNumber, file, encoding));
                }
                catch (UnrecognisedFilenameException)
                {
                    // ignore
                }
            }

            return scripts;
        }
    }
}