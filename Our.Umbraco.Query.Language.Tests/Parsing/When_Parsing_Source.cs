using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Our.Umbraco.Query.Language.Tests.Parsing
{
    [TestFixture]
    public class When_Parsing_Source : ParsingTest
    {
        [Test]
        public void Then_Has_Route_Node()
        {
            const string program = @"
                blogposts
                from 'home/blog'
            ";

            var tree = ParseTree(program);

            Assert.That(
                ((ContentNode)tree.Root.AstNode).Source.Route.AsString,
                Is.EqualTo("\"home/blog\"")
                );
        }
    }
}
