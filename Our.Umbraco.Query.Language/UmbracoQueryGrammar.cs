using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Ast;
using Irony.Interpreter;
using Irony.Interpreter.Ast;
using Irony.Parsing;

namespace Our.Umbraco.Query.Language
{
    public class UmbracoQueryGrammar : InterpretedLanguageGrammar
    {
        public UmbracoQueryGrammar() : base(false)
        {
            var contentTypeAlias = new IdentifierTerminal("contentType");
            var route = new StringLiteral("route", "'");

            var number = new NumberLiteral("number");

            var query = new NonTerminal("query");
            var limitModifier = new NonTerminal("limitModifier", typeof(LimitModifierNode));
            var orderModifier = new NonTerminal("orderModifier", typeof(OrderModifierNode));

            var contentType = new NonTerminal("contentType");
            var content = new NonTerminal("content", typeof(ContentNode));
            var limitedContent = new NonTerminal("limitedContent", typeof(LimitedContentNode));
            var orderedContent = new NonTerminal("orderedContent", typeof(OrderedContentNode));

            var source = new NonTerminal("source", typeof(SourceNode));

            orderedContent.Rule = orderModifier + content | orderModifier + limitedContent;
            limitedContent.Rule = limitModifier + content;

            source.Rule = "from" + route;

            content.Rule = contentType | contentType + source;
            contentType.Rule = contentTypeAlias;
            limitModifier.Rule = number;

            query.Rule = content | limitedContent | orderedContent;

            orderModifier.Rule = ToTerm("latest");
            
            Root = query;

            MarkTransient(contentType, query);
        }
    }

    public interface IVisitable
    {
        void Visit(IQueryVisitor visitor);
    }

    public class SourceNode : AstNode, IVisitable
    {
        public LiteralValueNode Route { get; set; }

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            Route = ((LiteralValueNode)treeNode.ChildNodes.Last().AstNode);
        }

        public void Visit(IQueryVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class OrderedContentNode : AstNode, IVisitable
    {
        public OrderModifierNode OrderModifier { get; set; }
        public IVisitable Content { get; set; }

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            OrderModifier = (OrderModifierNode)treeNode.ChildNodes[0].AstNode;
            Content = (IVisitable)treeNode.ChildNodes[1].AstNode;
        }

        public void Visit(IQueryVisitor visitor)
        {
            visitor.Visit(Content);
            visitor.Visit(OrderModifier);
        }
    }

    public class LimitedContentNode : AstNode, IVisitable
    {
        public LimitModifierNode LimitModifier { get; set; }
        public IVisitable Content { get; set; }

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            LimitModifier = (LimitModifierNode)treeNode.ChildNodes[0].AstNode;
            Content = (IVisitable)treeNode.ChildNodes[1].AstNode;
        }

        public void Visit(IQueryVisitor visitor)
        {
            visitor.Visit(Content);
            visitor.Visit(LimitModifier);
        }
    }

    public class ContentNode : AstNode, IVisitable
    {
        public IdentifierNode Identifier { get; set; }
        public SourceNode Source { get; set; }

        public string ContentType
        {
            get { return Identifier.Symbol; }
        }

        public override void Init(Irony.Ast.AstContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            Identifier = treeNode.ChildNodes[0].AstNode as IdentifierNode;
            if (treeNode.ChildNodes.Count > 1)
                Source = (SourceNode)treeNode.ChildNodes[1].AstNode;
        }

        public void Visit(IQueryVisitor visitor)
        {
            visitor.Visit(this);
            visitor.Visit(Source);
        }
    }



    //public class QueryNode : AstNode, IVisitable
    //{
    //    public IVisitable Query { get; private set; }
        
    //    public override void Init(AstContext context, ParseTreeNode treeNode)
    //    {
    //        base.Init(context, treeNode);

    //        Query = 
    //    }

    //    private void SetIfType<TType>(ParseTreeNode node, Action<TType> setter)
    //        where TType : AstNode
    //    {
    //        var astNode = node.AstNode as TType;
    //        if (astNode != null)
    //            setter(astNode);
    //    }

    //    public void Visit(IQueryVisitor visitor)
    //    {
    //        visitor.Visit(Source);
    //        if (OrderModifier != null)
    //            visitor.Visit(OrderModifier);
    //        if (LimitModifier != null)
    //            visitor.Visit(LimitModifier);
    //    }
    //}

    public class LimitModifierNode : AstNode, IVisitable
    {
        public int Limit { get; private set; }

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            var limitLiteral = (LiteralValueNode)treeNode.ChildNodes[0].AstNode;
            Limit = (int) limitLiteral.Value;
        }

        public void Visit(IQueryVisitor visitor)
        {
            visitor.Visit((LimitModifierNode) this);
        }
    }

    public class OrderModifierNode : AstNode, IVisitable
    {
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
        }

        public void Visit(IQueryVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
