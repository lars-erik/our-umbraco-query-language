using System;
using System.Globalization;
using Irony.Parsing;
using Lucene.Net.Index;
using Lucene.Net.Search;
using NUnit.Framework;

namespace Our.Umbraco.Query.Language.Tests
{
    [TestFixture]
    public class When_Building_Lucene_Query : ParsingTest
    {
        [Test]
        public void Adds_Criteria_For_NodeType()
        {
            var queryString = "news";
            var tree = ParseTree(queryString);
            var visitor = new LuceneQueryParser();
            var query = visitor.Execute((IVisitable)tree.Root.AstNode);
            Assert.That(query.ToString(), Is.EqualTo("+__nodeType:news"));
        }

        [Test]
        public void Adds_Order_And_Limit()
        {
            var queryString = "latest 1 news";
            var tree = ParseTree(queryString);
            var visitor = new LuceneQueryParser();
            visitor.Execute((IVisitable)tree.Root.AstNode);
            
            Assert.That(visitor.Limit, Is.EqualTo(1));
            Assert.That(visitor.Sort.GetSort()[0].GetField(), Is.EqualTo("__created"));

        }
    }

    public class LuceneQueryParser : IQueryVisitor
    {
        private BooleanQuery query;

        public Sort Sort { get; private set; }

        public int Limit { get; private set; }

        public LuceneQueryParser()
        {
        }

        public Lucene.Net.Search.Query Execute(IVisitable node)
        {
            query = new Lucene.Net.Search.BooleanQuery();
            node.Visit(this);
            return query;
        }

        public void Visit(ContentNode node)
        {
            query.Add(new TermQuery(new Term("__nodeType", node.ContentType)), BooleanClause.Occur.MUST);
        }

        public void Visit(OrderModifierNode node)
        {
            Sort = new Sort(new SortField("__created", CultureInfo.InvariantCulture, true));
        }

        public void Visit(LimitModifierNode limitModifier)
        {
            Limit = limitModifier.Limit;
        }

        // TODO: Baseclass this
        void IQueryVisitor.Visit(IVisitable node)
        {
            node.Visit(this);
        }
    }
}
