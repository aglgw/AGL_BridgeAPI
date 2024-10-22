using AGL.Api.ApplicationCore.Models.Queries;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AGL.Api.ApplicationCore.Extensions
{
    public static class LinqExtensions
    {
        private static PropertyInfo GetPropertyInfo(Type objType, string name)
        {
            var properties = objType.GetProperties();
            var matchedProperty = properties.FirstOrDefault(p => p.Name == name);
            if (matchedProperty == null)
                throw new ArgumentException("name");

            return matchedProperty;
        }

        private static LambdaExpression GetOrderExpression(Type objType, PropertyInfo pi)
        {
            var paramExpr = Expression.Parameter(objType);
            var propAccess = Expression.PropertyOrField(paramExpr, pi.Name);
            var expr = Expression.Lambda(propAccess, paramExpr);
            return expr;
        }

        private static Expression<Func<T, bool>> GetExpression<T>(string propertyName, string propertyValue)
        {
            var parameterExp = Expression.Parameter(typeof(T), "type");
            var propertyExp = Expression.Property(parameterExp, propertyName);
            MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var someValue = Expression.Constant(propertyValue, typeof(string));
            var containsMethodExp = Expression.Call(propertyExp, method, someValue);

            return Expression.Lambda<Func<T, bool>>(containsMethodExp, parameterExp);
        }

        private static List<T> WhereIn<T, TValue>(this IQueryable<T> source, IEnumerable<TValue> keys, Expression<Func<T, TValue>> selector)
        {
            MethodInfo method = null;
            foreach (MethodInfo tmp in typeof(Enumerable).GetMethods(
               BindingFlags.Public | BindingFlags.Static))
            {
                if (tmp.Name == "Contains" && tmp.IsGenericMethodDefinition
                                           && tmp.GetParameters().Length == 2)
                {
                    method = tmp.MakeGenericMethod(typeof(TValue));
                    break;
                }
            }
            if (method == null) throw new InvalidOperationException(
               "Unable to locate Contains");
            var row = Expression.Parameter(typeof(T), "row");
            var member = Expression.Invoke(selector, row);
            var values = Expression.Constant(keys, typeof(IEnumerable<TValue>));
            var predicate = Expression.Call(method, values, member);
            var lambda = Expression.Lambda<Func<T, bool>>(predicate, row);
            return source.Where(lambda).ToList();
        }

        public static IQueryable<T> WhereInFilter<T>(this IQueryable<T> source, List<QueryFilter> queryFilters) where T : class
        {
            Dictionary<string, Expression> searchExpressions = new Dictionary<string, Expression>();

            if (queryFilters == null || queryFilters.Count == 0)
            {
                return source;
            }

            foreach (var field in queryFilters.GroupBy(x => x.Field).Select(x => x.Key))
            {
                searchExpressions.Add(field.ToLower(), null);
            }

            Type elementType = typeof(T);
            ParameterExpression parameterExpression = Expression.Parameter(typeof(T));

            foreach (var filter in queryFilters)
            {
                if (string.IsNullOrEmpty(filter.Value))
                {
                    continue;
                }


                var prop = elementType.GetProperties().FirstOrDefault(x => x.Name.ToLower().Equals(filter.Field.ToLower()));
                var method = prop.PropertyType == typeof(string) ? "Contains" : "Equals";
                var value = prop.PropertyType == typeof(string) ? filter.Value : Convert.ChangeType(filter.Value, prop.PropertyType);
                MethodInfo propertyMethod = prop.PropertyType.GetMethod(method, new[] { prop.PropertyType });

                Expression whereExpression = Expression.Call(
                   Expression.Property(
                       parameterExpression,
                       prop
                   ),
                   propertyMethod,
                   Expression.Constant(value)
                );

                if (searchExpressions[filter.Field.ToLower()] == null)
                {
                    searchExpressions[filter.Field.ToLower()] = whereExpression;
                }
                else
                {
                    searchExpressions[filter.Field.ToLower()] = Expression.Or(searchExpressions[filter.Field.ToLower()], whereExpression);
                }
            }

            //Combine all the resultant expression nodes using &&
            Expression body = searchExpressions.Values
                .Aggregate(
                    (prev, current) => Expression.And(prev, current)
                );

            // Wrap the expression body in a compile-time-typed lambda expression
            Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(body, parameterExpression);

            // Because the lambda is compile-time-typed (albeit with a generic parameter), we can use it with the Where method
            return source.Where(lambda);
        }
        public static IEnumerable<T> WhereInFilter<T>(this IEnumerable<T> source, List<QueryFilter> queryFilters) where T : class
        {
            Dictionary<string, Expression> searchExpressions = new Dictionary<string, Expression>();

            if (queryFilters == null || queryFilters.Count == 0)
            {
                return source;
            }

            foreach (var field in queryFilters.GroupBy(x => x.Field).Select(x => x.Key))
            {
                searchExpressions.Add(field.ToLower(), null);
            }

            Type elementType = typeof(T);
            ParameterExpression parameterExpression = Expression.Parameter(typeof(T));

            foreach (var filter in queryFilters)
            {
                if (string.IsNullOrEmpty(filter.Value))
                {
                    continue;
                }


                var prop = elementType.GetProperties().FirstOrDefault(x => x.Name.ToLower().Equals(filter.Field.ToLower()));
                var method = prop.PropertyType == typeof(string) ? "Contains" : "Equals";
                var value = prop.PropertyType == typeof(string) ? filter.Value : Convert.ChangeType(filter.Value, prop.PropertyType);
                MethodInfo propertyMethod = prop.PropertyType.GetMethod(method, new[] { prop.PropertyType });

                Expression whereExpression = Expression.Call(
                   Expression.Property(
                       parameterExpression,
                       prop
                   ),
                   propertyMethod,
                   Expression.Constant(value)
                );

                if (searchExpressions[filter.Field] == null)
                {
                    searchExpressions[filter.Field] = whereExpression;
                }
                else
                {
                    searchExpressions[filter.Field] = Expression.Or(searchExpressions[filter.Field], whereExpression);
                }
            }

            //Combine all the resultant expression nodes using &&
            Expression body = searchExpressions.Values
                .Aggregate(
                    (prev, current) => Expression.And(prev, current)
                );

            // Wrap the expression body in a compile-time-typed lambda expression
            Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(body, parameterExpression);

            // Because the lambda is compile-time-typed (albeit with a generic parameter), we can use it with the Where method
            return source.AsQueryable().Where(lambda);
        }

        public static IQueryable<T> WhereIn<T>(this IQueryable<T> source, string propertyName, string term)
        {
            if (string.IsNullOrEmpty(term)) { return source; }

            Type elementType = typeof(T);

            if (!elementType.GetProperties().Any(x => x.Name.ToLower().Equals(propertyName?.ToLower()))) { return source; }

            PropertyInfo propertyInfo = elementType.GetProperties().FirstOrDefault(x => x.Name.ToLower().Equals(propertyName?.ToLower()));

            // Get the right overload of String.Contains
            MethodInfo containsMethod = propertyInfo.PropertyType.GetMethod("Contains", new[] { propertyInfo.PropertyType })!;

            ParameterExpression parameterExpression = Expression.Parameter(typeof(T));

            // Map each property to an expression tree node
            Expression containExpression = Expression.Call(                  // .Contains(...) 
                        Expression.Property(          // .PropertyName
                            parameterExpression,           // x 
                            propertyInfo
                        ),
                        containsMethod,
                        Expression.Constant(term)     // "term" 
                    );

            // Combine all the resultant expression nodes using ||
            //Expression body = expressions
            //    .Aggregate(
            //        (prev, current) => Expression.Or(prev, current)
            //    );

            // Wrap the expression body in a compile-time-typed lambda expression
            Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(containExpression, parameterExpression);

            // Because the lambda is compile-time-typed (albeit with a generic parameter), we can use it with the Where method
            return source.Where(lambda);
        }

        private static IEnumerable<T> OrderBy<T>(this IEnumerable<T> query, string name)
        {
            var propInfo = GetPropertyInfo(typeof(T), name);
            var expr = GetOrderExpression(typeof(T), propInfo);

            var method = typeof(Enumerable).GetMethods().FirstOrDefault(m => m.Name == "OrderBy" && m.GetParameters().Length == 2);
            var genericMethod = method.MakeGenericMethod(typeof(T), propInfo.PropertyType);
            return (IEnumerable<T>)genericMethod.Invoke(null, new object[] { query, expr.Compile() });
        }

        private static IQueryable<T> OrderBy<T>(this IQueryable<T> query, string name)
        {
            var propInfo = GetPropertyInfo(typeof(T), name);
            var expr = GetOrderExpression(typeof(T), propInfo);

            var method = typeof(Queryable).GetMethods().FirstOrDefault(m => m.Name == "OrderBy" && m.GetParameters().Length == 2);
            var genericMethod = method.MakeGenericMethod(typeof(T), propInfo.PropertyType);
            return (IQueryable<T>)genericMethod.Invoke(null, new object[] { query, expr });
        }

        private static IEnumerable<T> ThenBy<T>(this IEnumerable<T> query, string name)
        {
            var propInfo = GetPropertyInfo(typeof(T), name);
            var expr = GetOrderExpression(typeof(T), propInfo);

            var method = typeof(Enumerable).GetMethods().FirstOrDefault(m => m.Name == "ThenBy" && m.GetParameters().Length == 2);
            var genericMethod = method.MakeGenericMethod(typeof(T), propInfo.PropertyType);
            return (IEnumerable<T>)genericMethod.Invoke(null, new object[] { query, expr.Compile() });
        }

        private static IQueryable<T> ThenBy<T>(this IQueryable<T> query, string name)
        {
            var propInfo = GetPropertyInfo(typeof(T), name);
            var expr = GetOrderExpression(typeof(T), propInfo);

            var method = typeof(Queryable).GetMethods().FirstOrDefault(m => m.Name == "ThenBy" && m.GetParameters().Length == 2);
            var genericMethod = method.MakeGenericMethod(typeof(T), propInfo.PropertyType);
            return (IQueryable<T>)genericMethod.Invoke(null, new object[] { query, expr });
        }

        private static IEnumerable<T> OrderByDescending<T>(this IEnumerable<T> query, string name)
        {
            var propInfo = GetPropertyInfo(typeof(T), name);
            var expr = GetOrderExpression(typeof(T), propInfo);

            var method = typeof(Enumerable).GetMethods().FirstOrDefault(m => m.Name == "OrderByDescending" && m.GetParameters().Length == 2);
            var genericMethod = method.MakeGenericMethod(typeof(T), propInfo.PropertyType);
            return (IEnumerable<T>)genericMethod.Invoke(null, new object[] { query, expr.Compile() });
        }

        private static IQueryable<T> OrderByDescending<T>(this IQueryable<T> query, string name)
        {
            var propInfo = GetPropertyInfo(typeof(T), name);
            var expr = GetOrderExpression(typeof(T), propInfo);

            var method = typeof(Queryable).GetMethods().FirstOrDefault(m => m.Name == "OrderByDescending" && m.GetParameters().Length == 2);
            var genericMethod = method.MakeGenericMethod(typeof(T), propInfo.PropertyType);
            return (IQueryable<T>)genericMethod.Invoke(null, new object[] { query, expr });
        }

        private static IEnumerable<T> ThenByDescending<T>(this IEnumerable<T> query, string name)
        {
            var propInfo = GetPropertyInfo(typeof(T), name);
            var expr = GetOrderExpression(typeof(T), propInfo);

            var method = typeof(Enumerable).GetMethods().FirstOrDefault(m => m.Name == "ThenByDescending" && m.GetParameters().Length == 2);
            var genericMethod = method.MakeGenericMethod(typeof(T), propInfo.PropertyType);
            return (IEnumerable<T>)genericMethod.Invoke(null, new object[] { query, expr.Compile() });
        }

        private static IQueryable<T> ThenByDescending<T>(this IQueryable<T> query, string name)
        {
            var propInfo = GetPropertyInfo(typeof(T), name);
            var expr = GetOrderExpression(typeof(T), propInfo);

            var method = typeof(Queryable).GetMethods().FirstOrDefault(m => m.Name == "ThenByDescending" && m.GetParameters().Length == 2);
            var genericMethod = method.MakeGenericMethod(typeof(T), propInfo.PropertyType);
            return (IQueryable<T>)genericMethod.Invoke(null, new object[] { query, expr });
        }

        public static IQueryable<T> OrderByDynamic<T>(this IQueryable<T> source, string? sortKey, System.ComponentModel.ListSortDirection sortDirection)
        {
            if (!string.IsNullOrEmpty(sortKey))
            {
                if (sortDirection == ListSortDirection.Ascending)
                    return source.OrderBy(sortKey);
                else
                    return source.OrderByDescending(sortKey);
            }
            return source;
        }

        public static IEnumerable<T> OrderByDynamic<T>(this IEnumerable<T> source, string? sortKey, System.ComponentModel.ListSortDirection sortDirection)
        {
            if (!string.IsNullOrEmpty(sortKey))
            {
                if (sortDirection == ListSortDirection.Ascending)
                    return source.OrderBy(sortKey);
                else
                    return source.OrderByDescending(sortKey);
            }
            return source;
        }

        public static IQueryable<T> ThenByDynamic<T>(this IQueryable<T> source, string? sortKey, System.ComponentModel.ListSortDirection sortDirection)
        {
            if (!string.IsNullOrEmpty(sortKey))
            {
                if (sortDirection == ListSortDirection.Ascending)
                    return source.ThenBy(sortKey);
                else
                    return source.ThenByDescending(sortKey);
            }
            return source;
        }

        public static IEnumerable<T> ThenByDynamic<T>(this IEnumerable<T> source, string? sortKey, System.ComponentModel.ListSortDirection sortDirection)
        {
            if (!string.IsNullOrEmpty(sortKey))
            {
                if (sortDirection == ListSortDirection.Ascending)
                    return source.ThenBy(sortKey);
                else
                    return source.ThenByDescending(sortKey);
            }
            return source;
        }

        public static IQueryable<T> WhereInFilter2<T>(this IQueryable<T> source, List<QueryFilter> queryFilters) where T : class
        {
            Dictionary<string, Expression> searchExpressions = new Dictionary<string, Expression>();

            if (queryFilters == null || queryFilters.Count == 0)
            {
                return source;
            }

            foreach (var field in queryFilters.GroupBy(x => x.Field).Select(x => x.Key))
            {
                searchExpressions.Add(field, null);
            }

            Type elementType = typeof(T);
            ParameterExpression parameterExpression = Expression.Parameter(typeof(T));

            foreach (var filter in queryFilters)
            {
                if (string.IsNullOrEmpty(filter.Value))
                {
                    continue;
                }


                var prop = elementType.GetProperties().FirstOrDefault(x => x.Name.Equals(filter.Field));
                var method = prop.PropertyType == typeof(string) ? "Contains" : "Equals";
                var value = prop.PropertyType == typeof(string) ? filter.Value : Convert.ChangeType(filter.Value, prop.PropertyType);
                MethodInfo propertyMethod = prop.PropertyType.GetMethod(method, new[] { prop.PropertyType, typeof(StringComparison) });

                Expression whereExpression = Expression.Call(
                   Expression.Property(
                       parameterExpression,
                       prop
                   ),
                   propertyMethod,
                   Expression.Constant(value),
                   Expression.Constant(StringComparison.OrdinalIgnoreCase)
                );

                if (searchExpressions[filter.Field] == null)
                {
                    searchExpressions[filter.Field] = whereExpression;
                }
                else
                {
                    searchExpressions[filter.Field] = Expression.Or(searchExpressions[filter.Field], whereExpression);
                }
            }

            //Combine all the resultant expression nodes using &&
            Expression body = searchExpressions.Values
                .Aggregate(
                    (prev, current) => Expression.And(prev, current)
                );

            // Wrap the expression body in a compile-time-typed lambda expression
            Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(body, parameterExpression);

            // Because the lambda is compile-time-typed (albeit with a generic parameter), we can use it with the Where method
            return source.Where(lambda);
        }
        public static IEnumerable<T> WhereInFilter2<T>(this IEnumerable<T> source, List<QueryFilter> queryFilters) where T : class
        {
            Dictionary<string, Expression> searchExpressions = new Dictionary<string, Expression>();

            if (queryFilters == null || queryFilters.Count == 0)
            {
                return source;
            }

            foreach (var field in queryFilters.GroupBy(x => x.Field).Select(x => x.Key))
            {
                searchExpressions.Add(field, null);
            }

            Type elementType = typeof(T);
            ParameterExpression parameterExpression = Expression.Parameter(typeof(T));

            foreach (var filter in queryFilters)
            {
                if (string.IsNullOrEmpty(filter.Value))
                {
                    continue;
                }


                var prop = elementType.GetProperties().FirstOrDefault(x => x.Name.Equals(filter.Field));
                var method = prop.PropertyType == typeof(string) ? "Contains" : "Equals";
                var value = prop.PropertyType == typeof(string) ? filter.Value : Convert.ChangeType(filter.Value, prop.PropertyType);
                MethodInfo propertyMethod = prop.PropertyType.GetMethod(method, new[] { prop.PropertyType, typeof(StringComparison) });

                Expression whereExpression = Expression.Call(
                   Expression.Property(
                       parameterExpression,
                       prop
                   ),
                   propertyMethod,
                   Expression.Constant(value),
                   Expression.Constant(StringComparison.OrdinalIgnoreCase)
                );

                if (searchExpressions[filter.Field] == null)
                {
                    searchExpressions[filter.Field] = whereExpression;
                }
                else
                {
                    searchExpressions[filter.Field] = Expression.Or(searchExpressions[filter.Field], whereExpression);
                }
            }

            //Combine all the resultant expression nodes using &&
            Expression body = searchExpressions.Values
                .Aggregate(
                    (prev, current) => Expression.And(prev, current)
                );

            // Wrap the expression body in a compile-time-typed lambda expression
            Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(body, parameterExpression);

            // Because the lambda is compile-time-typed (albeit with a generic parameter), we can use it with the Where method
            return source.AsQueryable().Where(lambda);
        }


        public static IQueryable<T> WhereOrFilterT<T>(this IQueryable<T> source, List<QueryFilter> queryFilters) where T : class
        {
            Dictionary<string, Expression> searchExpressions = new Dictionary<string, Expression>();

            if (queryFilters == null || queryFilters.Count == 0)
            {
                return source;
            }

            foreach (var field in queryFilters.GroupBy(x => x.Field).Select(x => x.Key))
            {
                searchExpressions.Add(field, null);
            }

            Type elementType = typeof(T);
            ParameterExpression parameterExpression = Expression.Parameter(typeof(T));

            foreach (var filter in queryFilters)
            {
                if (string.IsNullOrEmpty(filter.Value))
                {
                    continue;
                }


                var prop = elementType.GetProperties().FirstOrDefault(x => x.Name.Equals(filter.Field));
                var method = prop.PropertyType == typeof(string) ? "Contains" : "Equals";
                var value = prop.PropertyType == typeof(string) ? filter.Value : Convert.ChangeType(filter.Value, prop.PropertyType);
                MethodInfo propertyMethod = prop.PropertyType.GetMethod(method, new[] { prop.PropertyType });

                Expression whereExpression = Expression.Call(
                   Expression.Property(
                       parameterExpression,
                       prop
                   ),
                   propertyMethod,
                   Expression.Constant(value)
                );

                if (searchExpressions[filter.Field] == null)
                {
                    searchExpressions[filter.Field] = whereExpression;
                }
                else
                {
                    searchExpressions[filter.Field] = Expression.Or(searchExpressions[filter.Field], whereExpression);
                }
            }

            //Combine all the resultant expression nodes using &&
            Expression body = searchExpressions.Values
                .Aggregate(
                    (prev, current) => Expression.Or(prev, current)
                );

            // Wrap the expression body in a compile-time-typed lambda expression
            Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(body, parameterExpression);

            // Because the lambda is compile-time-typed (albeit with a generic parameter), we can use it with the Where method
            return source.Where(lambda);
        }


        public static IQueryable<T> WhereOrFilter<T>(this IQueryable<T> source, List<QueryFilter> queryFilters) where T : class
        {
            Dictionary<string, Expression> searchExpressions = new Dictionary<string, Expression>();

            if (queryFilters == null || queryFilters.Count == 0)
            {
                return source;
            }

            foreach (var field in queryFilters.GroupBy(x => x.Field).Select(x => x.Key))
            {
                searchExpressions.Add(field, null);
            }

            Type elementType = typeof(T);
            ParameterExpression parameterExpression = Expression.Parameter(typeof(T));

            foreach (var filter in queryFilters)
            {
                if (string.IsNullOrEmpty(filter.Value))
                {
                    continue;
                }


                var prop = elementType.GetProperties().FirstOrDefault(x => x.Name.Equals(filter.Field));
                var method = prop.PropertyType == typeof(string) ? "Contains" : "Equals";
                var value = prop.PropertyType == typeof(string) ? filter.Value : Convert.ChangeType(filter.Value, prop.PropertyType);
                MethodInfo propertyMethod = prop.PropertyType.GetMethod(method, new[] { prop.PropertyType, typeof(StringComparison) });

                Expression whereExpression = Expression.Call(
                   Expression.Property(
                       parameterExpression,
                       prop
                   ),
                   propertyMethod,
                   Expression.Constant(value),
                   Expression.Constant(StringComparison.OrdinalIgnoreCase)
                );

                if (searchExpressions[filter.Field] == null)
                {
                    searchExpressions[filter.Field] = whereExpression;
                }
                else
                {
                    searchExpressions[filter.Field] = Expression.Or(searchExpressions[filter.Field], whereExpression);
                }
            }

            //Combine all the resultant expression nodes using &&
            Expression body = searchExpressions.Values
                .Aggregate(
                    (prev, current) => Expression.Or(prev, current)
                );

            // Wrap the expression body in a compile-time-typed lambda expression
            Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(body, parameterExpression);

            // Because the lambda is compile-time-typed (albeit with a generic parameter), we can use it with the Where method
            return source.Where(lambda);
        }
        public static IEnumerable<T> WhereOrFilter<T>(this IEnumerable<T> source, List<QueryFilter> queryFilters) where T : class
        {
            Dictionary<string, Expression> searchExpressions = new Dictionary<string, Expression>();

            if (queryFilters == null || queryFilters.Count == 0)
            {
                return source;
            }

            foreach (var field in queryFilters.GroupBy(x => x.Field).Select(x => x.Key))
            {
                searchExpressions.Add(field, null);
            }

            Type elementType = typeof(T);
            ParameterExpression parameterExpression = Expression.Parameter(typeof(T));

            foreach (var filter in queryFilters)
            {
                if (string.IsNullOrEmpty(filter.Value))
                {
                    continue;
                }


                var prop = elementType.GetProperties().FirstOrDefault(x => x.Name.Equals(filter.Field));
                var method = prop.PropertyType == typeof(string) ? "Contains" : "Equals";
                var value = prop.PropertyType == typeof(string) ? filter.Value : Convert.ChangeType(filter.Value, prop.PropertyType);
                MethodInfo propertyMethod = prop.PropertyType.GetMethod(method, new[] { prop.PropertyType, typeof(StringComparison) });

                Expression whereExpression = Expression.Call(
                   Expression.Property(
                       parameterExpression,
                       prop
                   ),
                   propertyMethod,
                   Expression.Constant(value),
                   Expression.Constant(StringComparison.OrdinalIgnoreCase)
                );

                if (searchExpressions[filter.Field] == null)
                {
                    searchExpressions[filter.Field] = whereExpression;
                }
                else
                {
                    searchExpressions[filter.Field] = Expression.Or(searchExpressions[filter.Field], whereExpression);
                }
            }

            //Combine all the resultant expression nodes using &&
            Expression body = searchExpressions.Values
                .Aggregate(
                    (prev, current) => Expression.Or(prev, current)
                );

            // Wrap the expression body in a compile-time-typed lambda expression
            Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(body, parameterExpression);

            // Because the lambda is compile-time-typed (albeit with a generic parameter), we can use it with the Where method
            return source.AsQueryable().Where(lambda);
        }
    }
}
