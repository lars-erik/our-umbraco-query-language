using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Our.Umbraco.Query.Language
{
    public interface IQueryVisitor
    {
        void Visit(QueryNode node);
        void Visit(ContentNode node);
        void Visit(OrderModifierNode node);
        void Visit(LimitModifierNode limitModifier);
    }
}
