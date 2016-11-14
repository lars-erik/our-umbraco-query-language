using System;
using System.Linq;
using Irony.Parsing;
using NUnit.Framework;

namespace Our.Umbraco.Query.Language.Tests
{
    public class ParsingTest
    {
        protected static ParseTree ParseTree(string program)
        {
            var grammar = new UmbracoQueryGrammar();
            var languageData = new LanguageData(grammar);
            Assert.That(languageData.Errors, Is.Empty,
                String.Join(Environment.NewLine, languageData.Errors.Select(e => e.Message)));
            var parser = new Parser(languageData);
            var tree = parser.Parse(program);
            Assert.That(tree.HasErrors(), Is.False, String.Join(Environment.NewLine, tree.ParserMessages.Select(m => m.Message)));
            return tree;
        }
    }
}