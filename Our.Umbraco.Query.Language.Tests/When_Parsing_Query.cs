using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Interpreter.Ast;
using Irony.Parsing;
using NUnit.Framework;

namespace Our.Umbraco.Query.Language.Tests
{
    [TestFixture]
    public class When_Parsing_Query : ParsingTest
    {
        [Test]
        public void ContentType_Only_Parses_Source_Node()
        {
            var program = "news";
            var tree = ParseTree(program);
            Assert.That(tree.Root.AstNode, 
                Is.InstanceOf<QueryNode>() &
                Has.Property("Source").InstanceOf<ContentNode>()
            );
        }

        [Test]
        public void With_Latest_Modifier_Parses_Order_Node()
        {
            var program = "latest news";
            var tree = ParseTree(program);
            Assert.That(tree.Root.AstNode,
                Has.Property("OrderModifier").InstanceOf<OrderModifierNode>()
                );
        }

        [Test]
        public void With_Count_Modifier_Parses_Source_Node()
        {
            var program = "5 news";
            var tree = ParseTree(program);
            Assert.That(tree.Root.AstNode,
                Has.Property("Source").InstanceOf<ContentNode>() &
                Has.Property("LimitModifier").InstanceOf<LimitModifierNode>()
                );
        }

        [Test]
        public void With_Latest_And_Count_Modifiers_Parses_Source_And_Order()
        {
            var program = "latest 5 news";
            var tree = ParseTree(program);
            Assert.That(tree.Root.AstNode,
                Has.Property("LimitModifier").InstanceOf<LimitModifierNode>() &
                Has.Property("OrderModifier").InstanceOf<OrderModifierNode>()
                );
        }
    }
}
