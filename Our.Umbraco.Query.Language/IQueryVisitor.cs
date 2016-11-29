using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Interpreter.Ast;

namespace Our.Umbraco.Query.Language
{
    public interface IQueryVisitor
    {
        void Visit(IVisitable node);
        void Visit(ContentNode node);
        void Visit(OrderModifierNode node);
        void Visit(LimitModifierNode limitModifier);
    }
}
