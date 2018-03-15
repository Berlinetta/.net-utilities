namespace DALUtility.Specifications
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using DALUtility.Data;

    public class SearchSpecification<T> : Specification<T>
    {
        private List<SearchCondition> conditions = new List<SearchCondition>();
        private String advancedCriteria;

        public SearchSpecification(List<SearchCondition> conditions, String advancedCriteria = default(String))
        {
            this.conditions = conditions;
            this.advancedCriteria = advancedCriteria;
        }

        public override Expression<Func<T, bool>> GetExpression()
        {
            ParameterExpression left = Expression.Parameter(typeof(T), "c");
            Expression result = Expression.Constant(true);
            if (String.IsNullOrEmpty(advancedCriteria))
                result = this.InitExpressionByNormal(left, result);
            else
            {
                result = this.InitExpressionByAdvance(left, result);
            }
            Expression<Func<T, bool>> finalExpression
                = Expression.Lambda<Func<T, bool>>(result, new ParameterExpression[] { left });
            return finalExpression;
        }

        private void SetRealValue(SearchCondition src, Expression dest)
        {
            Type srcType = src.Value1.GetType();
            if (srcType != dest.Type)
            {
                if (dest.Type == typeof(DateTime?) || dest.Type == typeof(DateTime))
                {
                    src.Value1 = new DateTime(Int64.Parse(src.Value1.ToString()), DateTimeKind.Utc);
                    if (src.Value2 != null)
                        src.Value2 = new DateTime(Int64.Parse(src.Value2.ToString()), DateTimeKind.Utc);
                }
                else
                {
                    TypeConverter tc = TypeDescriptor.GetConverter(dest.Type);
                    src.Value1 = tc.ConvertFromString(src.Value1.ToString());
                    if (src.Value2 != null)
                        src.Value2 = tc.ConvertFromString(src.Value2.ToString());
                }
            }
        }

        private Expression SetFilterOperate(FilterOperate filterOperate, Expression expression, SearchCondition condition)
        {
            Expression result = null;
            var value1 = condition.Value1 == null ? null : Expression.Constant(condition.Value1, expression.Type);
            var value2 = condition.Value2 == null ? null : Expression.Constant(condition.Value2, expression.Type);
            switch (filterOperate)
            {
                case FilterOperate.Equals:
                    result = Expression.Equal(expression, value1);
                    break;

                case FilterOperate.NotEquals:
                    result = Expression.NotEqual(expression, value1);
                    break;

                case FilterOperate.GreaterThan:
                    result = Expression.GreaterThan(expression, value1);
                    break;

                case FilterOperate.GreaterThanOrEquals:
                    result = Expression.GreaterThanOrEqual(expression, value1);
                    break;

                case FilterOperate.LessThan:
                    result = Expression.LessThan(expression, value1);
                    break;

                case FilterOperate.LessThanOrEquals:
                    result = Expression.LessThanOrEqual(expression, value1);
                    break;

                case FilterOperate.Contains:
                    result = Expression.Call(expression,
                        typeof(string).GetMethod("Contains", new Type[] { typeof(string) }),
                        value1);
                    break;

                case FilterOperate.Between:
                    result = Expression.And(Expression.GreaterThanOrEqual(expression, value1), Expression.LessThan(expression, value2));
                    break;

                default:
                    break;
            }
            return result;
        }

        private Expression SetRelatedOperate(Expression left, Expression right, RelatedOperate operate)
        {
            Expression result = null;
            switch (operate)
            {
                case RelatedOperate.None:
                    result = Expression.And(left, right);
                    break;

                case RelatedOperate.And:
                    result = Expression.And(left, right);
                    break;

                case RelatedOperate.Or:
                    result = Expression.Or(left, right);
                    break;

                default:
                    break;
            }
            return result;
        }

        private Expression InitExpressionByAdvance(ParameterExpression left, Expression result)
        {
            var postfixAdvance = this.ChangeToPostfix(advancedCriteria);
            var expression = this.JointExpression(postfixAdvance, left);
            expression = SetRelatedOperate(result, expression, RelatedOperate.And);
            return expression;
        }

        private Expression InitExpressionByNormal(ParameterExpression left, Expression result)
        {
            foreach (var condition in conditions)
            {
                Expression right = left;
                condition.ColumnName.Split('.').ToList().ForEach(item => right = Expression.Property(right, item));
                this.SetRealValue(condition, right);
                var body = SetFilterOperate(condition.FilterOperate, right, condition);
                result = SetRelatedOperate(result, body, condition.RelatedOperate);
            }
            return result;
        }

        private String ChangeToPostfix(String expression)
        {
            expression = expression.ToLower().Replace(" ", "").Replace("and", "&").Replace("or", "|");
            Stack<Char> operators = new Stack<Char>();
            StringBuilder result = new StringBuilder();
            for (Int32 i = 0; i < expression.Length; i++)
            {
                Char character = expression[i];
                if (Char.IsWhiteSpace(character))
                    continue;
                switch (character)
                {
                    case '&':
                        while (operators.Count > 0)
                        {
                            Char c = operators.Pop();
                            if (c == '(' || c == '|')
                            {
                                operators.Push(c);
                                break;
                            }
                            {
                                result.Append(c);
                            }
                        }
                        operators.Push(character);
                        break;

                    case '|':
                        while (operators.Count > 0)
                        {
                            Char c = operators.Pop();
                            if (c == '(')
                            {
                                operators.Push(c);
                                break;
                            }
                            else
                            {
                                result.Append(c);
                            }
                        }
                        operators.Push(character);
                        break;

                    case '(':
                        operators.Push(character);
                        break;

                    case ')':
                        while (operators.Count > 0)
                        {
                            Char c = operators.Pop();
                            if (c == '(')
                            {
                                break;
                            }
                            else
                            {
                                result.Append(c);
                            }
                        }
                        break;

                    default:
                        result.Append(character);
                        break;
                }
            }
            while (operators.Count > 0)
            {
                result.Append(operators.Pop()); //pop All Operator
            }
            return result.ToString();
        }

        private Expression JointExpression(String expression, ParameterExpression left)
        {
            Stack<Expression> results = new Stack<Expression>();
            Expression tmp1;
            Expression tmp2;
            for (Int32 i = 0; i < expression.Length; i++)
            {
                Char character = expression[i];
                if (Char.IsWhiteSpace(character))
                    continue;
                switch (character)
                {
                    case '&':
                        tmp2 = results.Pop();
                        tmp1 = results.Pop();
                        Expression AndExpression = SetRelatedOperate(tmp1, tmp2, RelatedOperate.And);
                        results.Push(AndExpression);
                        break;

                    case '|':
                        tmp2 = results.Pop();
                        tmp1 = results.Pop();
                        Expression OrExpression = SetRelatedOperate(tmp1, tmp2, RelatedOperate.Or);
                        results.Push(OrExpression);
                        break;

                    default:
                        var order = Convert.ToInt32(character.ToString());
                        var condition = this.conditions.Find(item => item.Order == order);
                        Expression right = left;
                        condition.ColumnName.Split('.').ToList().ForEach(item => right = Expression.Property(right, item));
                        this.SetRealValue(condition, right);
                        var body = SetFilterOperate(condition.FilterOperate, right, condition);
                        results.Push(body);
                        break;
                }
            }
            return results.Peek();
        }
    }
}