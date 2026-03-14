using System.Linq.Expressions;

namespace psi25_project.Utils
{
    public class ObjectValidator<T> where T : class, new()
    {
        public T CreateDefault()
        {
            return new T();
        }
        public bool ValidatePropertyNotNull<TProperty>(T obj, Expression<Func<T, TProperty>> selector)
        {
            var compiled = selector.Compile();
            return compiled(obj) != null;
        }
    }
}
