using System.Collections;
using System.Data;

namespace Perigon.AspNetCore.Utils;

public static class Extensions
{
    extension<TSource>(IQueryable<TSource> source)
    {
        /// <summary>
        /// 不为空(字符串)时执行的条件
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="field">要判断的字段</param>
        /// <param name="expression">不为空时执行的条件</param>
        /// <returns></returns>
        public IQueryable<TSource> WhereNotNull(
            object? field,
            Expression<Func<TSource, bool>> expression
        )
        {
            return field switch
            {
                null => source,
                string str when str.IsEmpty() => source,
                ICollection collection when collection.Count == 0 => source,
                Array array when array.Length == 0 => source,
                _ => source.Where(expression),
            };
        }

        /// <summary>
        /// 构建Between表达式
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="propertyExpression"></param>
        /// <param name="minVal"></param>
        /// <param name="maxVal"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private IQueryable<TSource> Between<TValue>(
            Expression<Func<TSource, TValue>> propertyExpression,
            TValue minVal,
            TValue maxVal
        ) where TValue : struct
        {
            ParameterExpression parameter = propertyExpression.Parameters[0];
            MemberExpression? memberExpression = (
                propertyExpression.Body as MemberExpression
                ?? (propertyExpression.Body as UnaryExpression)?.Operand as MemberExpression)

                ?? throw new ArgumentException(
                    "Invalid property expression",
                    nameof(propertyExpression)
                );
            Type propertyType = memberExpression.Type;

            var minValObj = Convert.ChangeType(minVal, propertyType);
            var maxValObj = Convert.ChangeType(maxVal, propertyType);

            ConstantExpression minExpr = Expression.Constant(minValObj, propertyType);
            ConstantExpression maxExpr = Expression.Constant(maxValObj, propertyType);
            MemberExpression propertyAccess = Expression.MakeMemberAccess(parameter, memberExpression.Member);
            BinaryExpression leftExpr = Expression.GreaterThanOrEqual(propertyAccess, minExpr);
            BinaryExpression rightExpr = Expression.LessThanOrEqual(propertyAccess, maxExpr);
            BinaryExpression andExpr = Expression.AndAlso(leftExpr, rightExpr);
            var lambdaExpr = Expression.Lambda<Func<TSource, bool>>(andExpr, parameter);

            return source.Where(lambdaExpr);
        }
    }

    extension<T>(IQueryable<T> source)
    {
        /// <summary>
        /// 范围查询:int
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="propertyExpression"></param>
        /// <param name="minVal"></param>
        /// <param name="maxVal"></param>
        /// <returns></returns>
        public IQueryable<T> Between(
            Expression<Func<T, int>> propertyExpression,
            int minVal,
            int maxVal
)
        {
            return source.Between<T, int>(propertyExpression, minVal, maxVal);
        }

        /// <summary>
        /// 范围查询:long
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="propertyExpression"></param>
        /// <param name="minVal"></param>
        /// <param name="maxVal"></param>
        /// <returns></returns>
        public IQueryable<T> Between(
            Expression<Func<T, long>> propertyExpression,
            int minVal,
            int maxVal
        )
        {
            return source.Between<T, long>(propertyExpression, minVal, maxVal);
        }

        /// <summary>
        /// 范围查询:double
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="propertyExpression"></param>
        /// <param name="minVal"></param>
        /// <param name="maxVal"></param>
        /// <returns></returns>
        public IQueryable<T> Between(Expression<Func<T, double>> propertyExpression, int minVal, int maxVal, int a = 1)
        {
            return source.Between<T, double>(propertyExpression, minVal, maxVal);
        }

        /// <summary>
        /// 范围条件:DateOnly
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="propertyExpression"></param>
        /// <param name="minVal"></param>
        /// <param name="maxVal"></param>
        /// <returns></returns>
        public IQueryable<T> Between(
            Expression<Func<T, DateTimeOffset>> propertyExpression,
            DateOnly minVal,
            DateOnly maxVal
        )
        {
            var minValObj = minVal.ToDateTimeOffset();
            var maxValObj = maxVal.ToDateTimeOffset();
            return source.Between<T, DateTimeOffset>(propertyExpression, minValObj, maxValObj);
        }

        /// <summary>
        /// 范围查询:DateTimeOffset
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="propertyExpression"></param>
        /// <param name="minVal"></param>
        /// <param name="maxVal"></param>
        /// <returns></returns>
        public IQueryable<T> Between(
            Expression<Func<T, DateTimeOffset>> propertyExpression,
            DateTimeOffset minVal,
            DateTimeOffset maxVal
)
        {
            return source.Between<T, DateTimeOffset>(propertyExpression, minVal, maxVal);
        }
    }

    extension<T>(List<T> nodes) where T : ITreeNode<T>
    {
        /// <summary>
        /// 带有根节点的列表转成树型结构
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nodes"></param>
        /// <returns></returns>
        public List<T> BuildTree()
        {
            nodes.ForEach(n =>
            {
                n.Children = [];
            });
            var nodeDict = nodes.ToDictionary(n => n.Id);
            List<T> res = [];

            foreach (T node in nodes)
            {
                if (node.ParentId == null)
                {
                    res.Add(node);
                }
                else
                {
                    if (nodeDict.TryGetValue(node.ParentId.Value, out T? value))
                    {
                        value.Children ??= [];
                        value.Children!.Add(node);
                    }
                }
            }
            return res;
        }
    }
}
