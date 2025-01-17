﻿using System;
using System.Collections.Generic;
using System.Linq;
using Dbdeploy.Core.Database;
using NUnit.Framework;

namespace Test.Dbdeploy.Database
{
    [TestFixture]
    public class QueryStatementSplitterTest
    {
        private QueryStatementSplitter splitter;

        [SetUp]
        public void SetUp() 
        {
            splitter = new QueryStatementSplitter();
        }

        [Test]
        public void ShouldNotSplitStatementsThatHaveNoDelimter() 
        {
            List<string> result = new List<string>(splitter.Split("SELECT 1"));
        
            Assert.Contains("SELECT 1", result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ShouldIgnoreSemicolonsInTheMiddleOfALine() 
        {
            List<string> result = new List<string>(splitter.Split("SELECT ';'"));
        
            Assert.Contains("SELECT ';'", result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ShouldSplitStatementsOnASemicolonAtTheEndOfALine() 
        {
            List<string> result = new List<string>(splitter.Split("SELECT 1;\nSELECT 2;"));
        
            Assert.Contains("SELECT 1", result);
            Assert.Contains("SELECT 2", result);

            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void ShouldSplitStatementsOnASemicolonAtTheEndOfALineEvenWithWindowsLineEndings() 
        {
            List<string> result = new List<string>(splitter.Split("SELECT 1;\r\nSELECT 2;"));

            Assert.Contains("SELECT 1", result);
            Assert.Contains("SELECT 2", result);

            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void ShouldSplitStatementsOnASemicolonAtTheEndOfALineIgnoringWhitespace() 
        {
            List<string> result = new List<string>(splitter.Split("SELECT 1;  \nSELECT 2;  "));
        
            Assert.Contains("SELECT 1", result);
            Assert.Contains("SELECT 2", result);

            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void ShouldLeaveLineBreaksAlone() 
        {
            Assert.Contains("SELECT" + Environment.NewLine + "1", splitter.Split("SELECT\n1").ToList());
            Assert.Contains("SELECT" + Environment.NewLine + "1", splitter.Split("SELECT\r\n1").ToList());
        }

        [Test]
        public void ShouldSupportRowStyleTerminators() 
        {
            splitter.Delimiter = "/";
            splitter.DelimiterType = new RowDelimiter();

            List<String> result = splitter.Split("SHOULD IGNORE /\nAT THE END OF A LINE\n/\nSELECT BLAH FROM DUAL").ToList();

            Assert.Contains("SHOULD IGNORE /" + Environment.NewLine + "AT THE END OF A LINE" + Environment.NewLine, result);
            Assert.Contains("SELECT BLAH FROM DUAL", result);
        
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void ShouldSupportDefinedNewLineCharacters() 
        {
            splitter.LineEnding = LineEnding.CrLf;
            Assert.Contains("SELECT\r\n1", splitter.Split("SELECT\n1").ToList());
            Assert.Contains("SELECT\r\n1", splitter.Split("SELECT\r\n1").ToList());

            splitter.LineEnding = LineEnding.Cr;
            Assert.Contains("SELECT\r1", splitter.Split("SELECT\n1").ToList());
            Assert.Contains("SELECT\r1", splitter.Split("SELECT\r\n1").ToList());

            splitter.LineEnding = LineEnding.Lf;
            Assert.Contains("SELECT\n1", splitter.Split("SELECT\n1").ToList());
            Assert.Contains("SELECT\n1", splitter.Split("SELECT\r\n1").ToList());

            splitter.LineEnding = LineEnding.Platform;
            Assert.Contains("SELECT" + Environment.NewLine + "1", splitter.Split("SELECT\n1").ToList());
            Assert.Contains("SELECT" + Environment.NewLine + "1", splitter.Split("SELECT\r\n1").ToList());
        }
    }
}