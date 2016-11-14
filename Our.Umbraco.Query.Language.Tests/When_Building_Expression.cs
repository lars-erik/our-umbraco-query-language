using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Irony.Interpreter.Ast;
using Moq;
using NUnit.Framework;

namespace Our.Umbraco.Query.Language.Tests
{
    [TestFixture]
    public class When_Building_Expression : ParsingTest
    {
        [Test]
        public void Gets_Content_Of_Type_From_Cache()
        {
            var program = "news";
            var tree = ParseTree(program);
            var cache = Mock.Of<IContentCache>();

            var visitor = new PublishedCacheQueryVisitor(cache);
            var call = visitor.Visit((QueryNode)tree.Root.AstNode);

            call();

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
            var call = visitor.Visit((QueryNode)tree.Root.AstNode);

            var result = call();

            Assert.That(result.First(), Has.Property("CreateDate").EqualTo(DateTime.Today.AddDays(1)));
        }

        [Test]
        public void Limits_Ordered_Result()
        {
            var program = "latest 1 news";
            var tree = ParseTree(program);
            var cache = Mock.Of<IContentCache>();
            Mock.Get(cache).Setup(c => c.GetContentByXPath("//news")).Returns(new List<IPublishedContent>
            {
                Mock.Of<IPublishedContent>(c => c.CreateDate == DateTime.Today),
                Mock.Of<IPublishedContent>(c => c.CreateDate == DateTime.Today.AddDays(1))
            });

            var visitor = new PublishedCacheQueryVisitor(cache);
            var call = visitor.Visit((QueryNode)tree.Root.AstNode);

            var result = call();

            Assert.That(result.Single(), Has.Property("CreateDate").EqualTo(DateTime.Today.AddDays(1)));
        }
    }

    public class PublishedCacheQueryVisitor
    {
        private readonly IContentCache cache;
        //private readonly ParameterExpression cacheExpr;
        //private static readonly Type CacheType = typeof (IContentCache);
        //private static readonly MethodInfo ContentByXPathMethod = CacheType.GetMethod("GetContentByXPath");

        public PublishedCacheQueryVisitor(IContentCache cache)
        {
            this.cache = cache;
            //cacheExpr = Expression.Parameter(typeof(IContentCache), "cache");
        }

        #warning This should probably not be a contract. For lucene, a query?
        public Func<IEnumerable<IPublishedContent>> Visit(QueryNode queryNode)
        {
            var limit = queryNode.Source.Limit;
            var expression = Visit(queryNode.Source);
            if (queryNode.OrderModifier != null)
            {
                expression = Visit(queryNode.OrderModifier, expression);
            }
            if (limit != null)
            {
                expression = Limit(limit, expression);
            }

            return expression;
        }

        private Func<IEnumerable<IPublishedContent>> Visit(LimitedContentNode limitedNode)
        {
            Func<IEnumerable<IPublishedContent>> accessor = () =>
                cache.GetContentByXPath("//" + limitedNode.Source.Symbol);

            return accessor;
        }

        private Func<IEnumerable<IPublishedContent>> Visit(OrderModifierNode orderedNode, Func<IEnumerable<IPublishedContent>> original)
        {
            Func<IEnumerable<IPublishedContent>> sorter = () => original().OrderByDescending(c => c.CreateDate);

            return sorter;
        }

        private Func<IEnumerable<IPublishedContent>> Limit(LiteralValueNode limit, Func<IEnumerable<IPublishedContent>> expression)
        {
            return () => expression().Take(Convert.ToInt32(limit.Value));
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
