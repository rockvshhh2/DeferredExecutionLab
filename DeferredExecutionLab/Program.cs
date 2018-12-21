using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DeferredExecutionLab
{
    class Program
    {
        static void Main(string[] args)
        {
            ClassA a = new ClassA {
                Id = 1,
                Name = "david",
                Age = 30,
                Address = "台北市"
            };

            ClassB b = new ClassB {
                Id = 1,
                Name = "peter"
            };

            // 功能 : 比對兩個物件，指定的屬性有沒有一樣
            var comparison = a.ComparisonFor(b).ComparisonFilterProps(new List<string> { nameof(ClassA.Id), nameof(ClassA.Name), nameof(ClassA.Age) }).ToComparison();

            var flow_1 = a.ComparisonFor(b);
            var flow_2 = flow_1.ComparisonFilterProps(new List<string> { nameof(ClassA.Id), nameof(ClassA.Name), nameof(ClassA.Age) });
            var flow_3 = flow_2.ToComparison();
        }
    }

    class ClassA
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Age { get; set; }

        public string Address { get; set; }
    }

    class ClassB
    {
        public int Id { get; set; }

        public string Name { get; set; }

    }

    static class ObjectComparisonExpansion
    {

        public static ObjectComparison ComparisonFor(this object source, object comparisonTarget)
        {
            ObjectComparison objectComparison = new ObjectComparison();

            #region Expression Init

            Expression sourceObj = Expression.Constant(source);

            var method = typeof(ObjectComparisonExpansion).GetMethods().FirstOrDefault(x => x.Name == (nameof(ObjectComparisonExpansion.ComparisonFor)));

            Expression targetObj = Expression.Constant(comparisonTarget);

            objectComparison.ExcuteFlow = Expression.Call(method, sourceObj, targetObj);

            #endregion

            return objectComparison;
        }

        public static ObjectComparison ComparisonFilterProps(this ObjectComparison source, IEnumerable<string> filterProps)
        {
            #region Expression Init

            var method = typeof(ObjectComparisonExpansion).GetMethods().FirstOrDefault(x => x.Name == (nameof(ObjectComparisonExpansion.ComparisonFilterProps)));

            Expression target = Expression.Constant(filterProps);

            source.ExcuteFlow = Expression.Call(method, source.ExcuteFlow, target);

            #endregion

            return source;
        }

        public static List<string> ToComparison(this ObjectComparison source)
        {
            List<string> datas = new List<string>();

            #region Init

            object sourceObj = null;
            object targetObj = null;
            List<string> filterProps = new List<string>();

            #endregion

            #region Scan ExcuteFlow

            scanExpression(source.ExcuteFlow);

            void scanExpression(Expression expor, string method = "", int argIndex = -1)
            {
                switch (expor)
                {
                    case MethodCallExpression mce:

                        for (int i = 0; i < mce.Arguments.Count; i++)
                        {
                            Expression arg = mce.Arguments[i];

                            if (arg is MethodCallExpression)
                            {
                                scanExpression(arg);
                            }
                            else if (arg is ConstantExpression)
                            {
                                scanExpression(arg, method: mce.Method.Name, argIndex: i);
                            }
                        }                        

                        break;

                    case ConstantExpression ce:

                        if (method == "ComparisonFor")
                        {
                            if (argIndex == 0)
                            {
                                sourceObj = ce.Value;
                            }
                            else if (argIndex == 1)
                            {
                                targetObj = ce.Value;
                            }
                        }
                        else if(method == "ComparisonFilterProps")
                        {
                            if (argIndex == 1)
                            {
                                filterProps = ce.Value as List<string>;
                            }
                        }

                        break;

                    default:
                        break;
                }
            }

            #endregion

            #region Comparison

            var sourceProps = sourceObj.GetType().GetProperties();
            var targetProps = targetObj.GetType().GetProperties();

            foreach (var filter in filterProps)
            {
                var sourceProp = sourceProps.FirstOrDefault(x => x.Name == filter);
                var targetProp = targetProps.FirstOrDefault(x => x.Name == filter);

                if (sourceProp != null && targetProp != null)
                {
                    var sourceValue = sourceProp.GetValue(sourceObj);
                    var tatgetValue = targetProp.GetValue(targetObj);

                    if (sourceProp.PropertyType == typeof(int))
                    {
                        if (((int)sourceValue) != ((int)tatgetValue))
                        {
                            datas.Add(filter);
                        }
                    }
                    else if (sourceProp.PropertyType == typeof(string))
                    {
                        if (((string)sourceValue) != ((string)tatgetValue))
                        {
                            datas.Add(filter);
                        }
                    }
                }
                else
                {
                    datas.Add(filter);
                }
            }

            #endregion

            return datas;
        }
    }

    class ObjectComparison
    {
        public Expression ExcuteFlow { get; set; }
    }
}
