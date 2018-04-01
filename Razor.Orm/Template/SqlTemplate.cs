﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Razor.Orm.Template
{
    public abstract class SqlTemplate<TModel> : ISqlTemplate
    {
        protected TModel Model { get; private set; }
        private ILogger logger;
        private SqlWriter sqlWriter;

        private int parametersIndex;
        private IList<SqlParameter> parameters;

        public SqlTemplate()
        {
            logger = this.CreateLogger();
        }

        public SmartBuilder Smart { get; private set; }

        public SqlTemplateResult Process(object model)
        {
            sqlWriter = new SqlWriter();
            Smart = new SmartBuilder(sqlWriter);
            parametersIndex = 0;
            parameters = new List<SqlParameter>();

            if (model != null)
            {
                var modelType = model.GetType();
                var typeInfo = modelType.GetTypeInfo();

                if (typeInfo.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Any() && modelType.FullName.Contains("AnonymousType"))
                {
                    IDictionary<string, object> expando = new ExpandoObject();
                    foreach (var propertyDescriptor in typeInfo.GetProperties())
                    {
                        var obj = propertyDescriptor.GetValue(model);
                        expando.Add(propertyDescriptor.Name, obj);
                    }

                    Model = (TModel)expando;
                }
                else
                {
                    Model = (TModel)model;
                } 
            }

            Execute();

            var sql = sqlWriter.ToString();
            var sqlParameters = parameters.ToArray();

            if (logger != null)
            {
                logger.LogInformation(sql);
                logger.LogInformation(string.Join('\n', sqlParameters.Select(e => $"{e.ParameterName} -> {e.Value}")));
            }

            return new SqlTemplateResult(sql, sqlParameters);
        }

        protected virtual void Write(object value)
        {
            sqlWriter.Write(value);
        }

        protected string ParseString(string value)
        {
            return $"'{value}'";
        }

        protected virtual void Write(string value)
        {
            sqlWriter.Write(ParseString(value));
        }

        protected virtual void Write(EscapeString value)
        {
            sqlWriter.Write(value);
        }

        protected virtual void WriteLiteral(string value)
        {
            sqlWriter.Write(value);
        }

        protected abstract void Execute();

        public EscapeString Par(object value)
        {
            var key = string.Format("@p{0}", parametersIndex);
            parametersIndex++;
            parameters.Add(new SqlParameter(key, value));
            return new EscapeString(key);
        }

        public EscapeString InParams(params string[] values)
        {
            return In(values);
        }

        public EscapeString InParams<T>(params T[] values)
        {
            return PrivateIn(values);
        }

        public EscapeString In(string[] values)
        {
            return PrivateIn(values.Select(e => ParseString(e)));
        }

        public EscapeString In<T>(T[] values)
        {
            return PrivateIn(values);
        }

        private EscapeString PrivateIn<T>(IEnumerable<T> values)
        {
            return new EscapeString($"in ({string.Join(',', values)})");
        }
    }

    public abstract class SqlTemplate<TModel, TResult> : SqlTemplate<TModel>
    {
        private Hashtable asBinds = new Hashtable();

        public EscapeString As<T>(Expression<Func<TResult, T>> expression)
        {
            return new EscapeString(GetAsBind(expression));
        }

        private string GetAsBind<T>(Expression<Func<TResult, T>> expression)
        {
            if (asBinds.ContainsKey(expression))
            {
                return (string)asBinds[expression];
            }
            else
            {
                var stringBuilder = new StringBuilder();
                Stringify(stringBuilder, expression.Body);
                stringBuilder.Insert(0, "as '");
                stringBuilder.Append("'");
                var result = stringBuilder.ToString();
                asBinds.Add(expression, result);
                return result;
            }
        }

        private void Stringify(StringBuilder stringBuilder, Expression expression)
        {
            if (expression is MemberExpression memberExpression)
            {
                stringBuilder.Insert(0, memberExpression.Member.Name);

                if (memberExpression.Expression.NodeType == ExpressionType.MemberAccess)
                {
                    stringBuilder.Insert(0, "_");
                    Stringify(stringBuilder, memberExpression.Expression);
                }
            }
        }
    }
}
