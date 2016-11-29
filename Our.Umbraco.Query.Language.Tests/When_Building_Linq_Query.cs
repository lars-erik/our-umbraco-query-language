using System;
using System.Collections.Generic;
using System.Linq;
using Irony.Interpreter.Ast;
using Moq;
using NUnit.Framework;

namespace Our.Umbraco.Query.Language.Tests
{
    [TestFixture]
    public class When_Building_Linq_Query : ParsingTest
    {
        [Test]
        public void Gets_Content_Of_Type_From_Cache()
        {
            var program = "news";
            var tree = ParseTree(program);
            var cache = Mock.Of<IContentCache>();

            var visitor = new PublishedCacheQueryVisitor(cache);
            var result = visitor.Execute((IVisitable)tree.Root.AstNode);

            Mock.Get(cache).Verify(c => c.GetContentByXPath("//news"));
        }

        [Test]
        public void Orders_By_CreateDate_Descending_When_Latest()
        {
            var program = "latest news";
            var tree = ParseTree(program);
            var cache = Mock.Of<IContentCache>();
            Mock.Get(cache).Setup(c => c.GetContentByXPath("//news")).Returns(new List<IPublishedContent>
            {
                Mock.Of<IPublishedContent>(c => c.CreateDate == DateTime.Today),
                Mock.Of<IPublishedContent>(c => c.CreateDate == DateTime.Today.AddDays(1))
            });

            var visitor = new PublishedCacheQueryVisitor(cache);
            var result = visitor.Execute((IVisitable)tree.Root.AstNode);

            Assert.That(result.First(), Has.Property("CreateDate").EqualTo(DateTime.Today.AddDays(1)));
        }

        [Test]
        public void Limits_Ordered_Result()
        {
            var program = "latest 1 article";
            var tree = ParseTree(program);
            var cache = Mock.Of<IContentCache>();
            Mock.Get(cache).Setup(c => c.GetContentByXPath("//article")).Returns(new List<IPublishedContent>
            {
                Mock.Of<IPublishedContent>(c => c.CreateDate == DateTime.Today),
                Mock.Of<IPublishedContent>(c => c.CreateDate == DateTime.Today.AddDays(1))
            });

            var visitor = new PublishedCacheQueryVisitor(cache);
            var result = visitor.Execute((IVisitable)tree.Root.AstNode);

            Assert.That(result.Single(), Has.Property("CreateDate").EqualTo(DateTime.Today.AddDays(1)));
        }
    }

    public class PublishedCacheQueryVisitor : IQueryVisitor
    {
        private readonly IContentCache cache;
        private IQueryable<IPublishedContent> current;
        private int? limit;

        public PublishedCacheQueryVisitor(IContentCache cache)
        {
            this.cache = cache;
        }

        public IQueryable<IPublishedContent> Execute(IVisitable queryNode)
        {
            queryNode.Visit(this);

            if (limit != null)
                current = current.Take(limit.Value);

            return current;
        }

        void IQueryVisitor.Visit(IVisitable node)
        {
            node.Visit(this);
        }

        void IQueryVisitor.Visit(ContentNode contentNode)
        {
            current = cache.GetContentByXPath("//" + contentNode.ContentType).AsQueryable();
        }

        void IQueryVisitor.Visit(LimitModifierNode limitNode)
        {
            limit = limitNode.Limit;
        }

        void IQueryVisitor.Visit(OrderModifierNode orderedNode)
        {
            current = current.OrderByDescending(c => c.CreateDate);
        }
    }

    public interface IContentCache
    {
        IEnumerable<IPublishedContent> GetContentByXPath(string xPath);
    }

    public interface IPublishedContent
    {
        DateTime CreateDate { get; set; }
    }
}
