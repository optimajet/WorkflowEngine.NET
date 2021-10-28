using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace OptimaJet.Workflow.MongoDB
{
    public interface IOrderBy
    {
        dynamic Expression { get; }
    }
    
    public class OrderBy<TSource,TKey> : IOrderBy
    {
        private readonly Expression<Func<TSource, TKey>> expression;
	
        public OrderBy(Expression<Func<TSource, TKey>> expression)
        {
            this.expression = expression;
        }

        public dynamic Expression => this.expression;
    }
}
