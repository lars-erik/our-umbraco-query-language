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

            var number = new NumberLiteral("number");

            var query = new NonTerminal("query", typeof(QueryNode));
            var limitModifier = new NonTerminal("limitModifier", typeof(LimitModifierNode));
            var orderModifier = new NonTerminal("orderModifier", typeof(OrderModifierNode));

            var contentType = new NonTerminal("contentType");
            var content = new NonTerminal("content", typeof(ContentNode));

            content.Rule = contentType;
            contentType.Rule = contentTypeAlias;
            limitModifier.Rule = number;

            query.Rule = content | orderModifier + content |
                         limitModifier + content | orderModifier + limitModifier + content;
            orderModifier.Rule = ToTerm("latest");
            
            Root = query;

            MarkTransient(contentType);
        }
    }

    public class ContentNode : AstNode
    {
        public IdentifierNode Identifier { get; set; }

        public string ContentType
        {
            get { return Identifier.Symbol; }
        }

        public override void Init(Irony.Ast.AstContext context, Irony.Parsing.ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            Identifier = treeNode.ChildNodes[0].AstNode as IdentifierNode;
        }

        public void Visit(IQueryVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class QueryNode : AstNode
    {
        public OrderModifierNode OrderModifier { get; private set; }
        public LimitModifierNode LimitModifier { get; private set; }
        public ContentNode Source { get; private set; }
        
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            foreach (var child in treeNode.ChildNodes)
            {
                SetIfType<OrderModifierNode>(child, n => OrderModifier = n);
                SetIfType<LimitModifierNode>(child, n => LimitModifier = n);
                SetIfType<ContentNode>(child, n => Source = n);
            }
        }

        private void SetIfType<TType>(ParseTreeNode node, Action<TType> setter)
            where TType : AstNode
        {
            var astNode = node.AstNode as TType;
            if (astNode != null)
                setter(astNode);
        }

        public void Visit(IQueryVisitor visitor)
        {
            visitor.Visit(Source);
            if (OrderModifier != null)
                visitor.Visit(OrderModifier);
            if (LimitModifier != null)
                visitor.Visit(LimitModifier);
        }
    }

    public class LimitModifierNode : AstNode
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

    public class OrderModifierNode : AstNode
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
