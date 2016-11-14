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
            var limitModifier = new NonTerminal("limitModifier");
            var orderModifier = new NonTerminal("orderModifier", typeof(OrderModifierNode));

            var contentType = new NonTerminal("contentType");
            var limitedContent = new NonTerminal("limitedContentType", typeof(LimitedContentNode));

            contentType.Rule = contentTypeAlias;
            limitedContent.Rule = contentType | limitModifier + contentType;
            limitModifier.Rule = number;

            query.Rule = limitedContent | orderModifier + limitedContent;
            orderModifier.Rule = ToTerm("latest");
            
            Root = query;

            MarkTransient(contentType, limitModifier);
        }
    }

    public class QueryNode : AstNode
    {
        public OrderModifierNode OrderModifier { get; private set; }
        public LimitedContentNode Source { get; private set; }

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            foreach (var child in treeNode.ChildNodes)
            {
                SetIfType<OrderModifierNode>(child, n => OrderModifier = n);
                SetIfType<LimitedContentNode>(child, n => Source = n);
            }
        }

        private void SetIfType<TType>(ParseTreeNode node, Action<TType> setter)
            where TType : AstNode
        {
            var astNode = node.AstNode as TType;
            if (astNode != null)
                setter(astNode);
        }
    }

    public class OrderModifierNode : AstNode
    {
    }

    public class LimitedContentNode : AstNode
    {
        public LiteralValueNode Limit { get; private set; }
        public IdentifierNode Source { get; private set; }

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            Limit = treeNode.ChildNodes[0].AstNode as LiteralValueNode;

            Source = Limit == null 
                ? treeNode.ChildNodes[0].AstNode as IdentifierNode
                : treeNode.ChildNodes[1].AstNode as IdentifierNode;
        }
    }
}
