﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Yuri.PlatformCore.VM;

namespace Yuri.PlatformCore.Evaluator
{
    /// <summary>
    /// 逆波兰式求值器
    /// </summary>
    internal class PolishEvaluator : IEvaluator
    {
        /// <summary>
        /// 计算表达式的值
        /// </summary>
        /// <param name="expr">表达式字符串</param>
        /// <param name="ctx">求值上下文</param>
        /// <returns>计算结果的值（Double/字符串）</returns>
        public object Eval(string expr, EvaluatableContext ctx)
        {
            var calcList = PolishEvaluator.GetPolishItemList(expr, null, ctx);
            return PolishEvaluator.HandleEval(expr, calcList);
        }

        /// <summary>
        /// 计算表达式的真值
        /// </summary>
        /// <param name="polish">表达式字符串</param>
        /// <param name="ctx">求值上下文</param>
        /// <returns>表达式的真值</returns>
        public bool EvalBoolean(string polish, EvaluatableContext ctx)
        {
            return Convert.ToBoolean(this.Eval(polish, ctx));
        }

        /// <summary>
        /// 计算表达式
        /// </summary>
        /// <param name="polish">表达式的逆波兰式</param>
        /// <param name="vsm">关于哪个调用堆栈做动作</param>
        /// <returns>计算结果的值（Double/字符串）</returns>
        public static object Evaluate(string polish, StackMachine vsm)
        {
            var calcList = PolishEvaluator.GetPolishItemList(polish, vsm, null);
            return PolishEvaluator.HandleEval(polish, calcList);
        }

        /// <summary>
        /// 计算表达式的真值
        /// </summary>
        /// <param name="polish">表达式的逆波兰式</param>
        /// <param name="vsm">关于哪个调用堆栈做动作</param>
        /// <returns>表达式的真值</returns>
        public static bool EvaluateBoolean(string polish, StackMachine vsm)
        {
            return Convert.ToBoolean(PolishEvaluator.Evaluate(polish, vsm));
        }

        /// <summary>
        /// 执行实际的运算操作
        /// </summary>
        /// <param name="polish">要计算的逆波兰式</param>
        /// <param name="calcList">逆波兰计算项向量</param>
        /// <returns>逆波兰式折叠结果</returns>
        private static object HandleEval(string polish, List<PolishItem> calcList)
        {
            if (calcList.Count == 0)
            {
                return null;
            }
            var calcStack = new Stack<PolishItem>();
            foreach (PolishItem poi in calcList)
            {
                // 操作数压栈
                if (poi.ItemType < PolishItemType.CAL_PLUS)
                {
                    calcStack.Push(poi);
                    continue;
                }
                // 下面开始是运算符
                if (poi.ItemType == PolishItemType.CAL_NOT && calcStack.Count >= 1)
                {
                    PolishItem peeker = calcStack.Peek();
                    switch (peeker.ItemType)
                    {
                        case PolishItemType.CONSTANT:
                        case PolishItemType.VAR_NUM:
                        {
                            calcStack.Pop();
                            double notres = Math.Abs(peeker.Number) < 1e-15 ? 1.0 : 0.0;
                            PolishItem np = new PolishItem()
                            {
                                Number = notres,
                                Reference = notres
                            };
                            calcStack.Push(np);
                            continue;
                        }
                        case PolishItemType.STRING:
                        case PolishItemType.VAR_STRING:
                        {
                            calcStack.Pop();
                            double notres = peeker.Cluster == String.Empty ? 1.0 : 0.0;

                            PolishItem np = new PolishItem()
                            {
                                Number = notres,
                                Reference = notres
                            };
                            calcStack.Push(np);
                            continue;
                        }
                    }
                }
                if (calcStack.Count >= 2)
                {
                    PolishItem operand2 = calcStack.Pop();
                    PolishItem operand1 = calcStack.Pop();
                    if (PolishItem.IsOperatable(operand1, operand2, poi))
                    {
                        PolishItem newPoi;
                        double tempDouble;
                        switch (poi.ItemType)
                        {
                            case PolishItemType.CAL_PLUS:
                                if (operand1.Reference is string || operand2.Reference is string)
                                {
                                    var tempString = Convert.ToString(operand1.Reference) + Convert.ToString(operand2.Reference);
                                    newPoi = new PolishItem()
                                    {
                                        Cluster = tempString,
                                        Reference = tempString
                                    };
                                    calcStack.Push(newPoi);
                                }
                                else
                                {
                                    tempDouble = Convert.ToDouble(operand1.Reference) + Convert.ToDouble(operand2.Reference);
                                    newPoi = new PolishItem()
                                    {
                                        Number = tempDouble,
                                        Reference = tempDouble
                                    };
                                    calcStack.Push(newPoi);
                                }
                                break;
                            case PolishItemType.CAL_MINUS:
                                if (operand1.Reference is ValueType)
                                {
                                    tempDouble = Convert.ToDouble(operand1.Reference) - Convert.ToDouble(operand2.Reference);
                                    newPoi = new PolishItem()
                                    {
                                        Number = tempDouble,
                                        Reference = tempDouble
                                    };
                                    calcStack.Push(newPoi);
                                }
                                else
                                {
                                    throw new Exception("字符串异常操作：减法");
                                }
                                break;
                            case PolishItemType.CAL_MULTI:
                                if (operand1.Reference is ValueType)
                                {
                                    tempDouble = Convert.ToDouble(operand1.Reference) * Convert.ToDouble(operand2.Reference);
                                    newPoi = new PolishItem()
                                    {
                                        Number = tempDouble,
                                        Reference = tempDouble
                                    };
                                    calcStack.Push(newPoi);
                                }
                                else
                                {
                                    throw new Exception("字符串异常操作：乘法");
                                }
                                break;
                            case PolishItemType.CAL_DIV:
                                if (operand1.Reference is ValueType)
                                {
                                    if (Math.Abs((double)operand2.Reference) < 0)
                                    {
                                        throw new Exception("除零错误");
                                    }
                                    tempDouble = Convert.ToDouble(operand1.Reference) / Convert.ToDouble(operand2.Reference);
                                    newPoi = new PolishItem()
                                    {
                                        Number = tempDouble,
                                        Reference = tempDouble
                                    };
                                    calcStack.Push(newPoi);
                                }
                                else
                                {
                                    throw new Exception("字符串异常操作：除法");
                                }
                                break;
                            case PolishItemType.CAL_ANDAND:
                                if (operand1.Reference is ValueType)
                                {
                                    tempDouble = (Math.Abs(Convert.ToDouble(operand1.Reference)) > 0 && Math.Abs(Convert.ToDouble(operand2.Reference)) > 0) ? 1 : 0;
                                    newPoi = new PolishItem()
                                    {
                                        Number = tempDouble,
                                        Reference = tempDouble
                                    };
                                    calcStack.Push(newPoi);
                                }
                                else
                                {
                                    throw new Exception("字符串异常操作：&&");
                                }
                                break;
                            case PolishItemType.CAL_OROR:
                                if (operand1.Reference is ValueType)
                                {
                                    tempDouble = (Math.Abs(Convert.ToDouble(operand1.Reference)) > 0 || Math.Abs(Convert.ToDouble(operand2.Reference)) > 0) ? 1 : 0;
                                    newPoi = new PolishItem()
                                    {
                                        Number = tempDouble,
                                        Reference = tempDouble
                                    };
                                    calcStack.Push(newPoi);
                                }
                                else
                                {
                                    throw new Exception("字符串异常操作：||");
                                }
                                break;
                            case PolishItemType.CAL_EQUAL:
                                if (operand1.Reference is ValueType)
                                {
                                    tempDouble = Convert.ToDouble(operand1.Reference) == Convert.ToDouble(operand2.Reference) ? 1 : 0;
                                    newPoi = new PolishItem()
                                    {
                                        Number = tempDouble,
                                        Reference = tempDouble
                                    };
                                    calcStack.Push(newPoi);
                                }
                                else
                                {
                                    tempDouble = (string)operand1.Reference == (string)operand2.Reference ? 1 : 0;
                                    newPoi = new PolishItem()
                                    {
                                        Number = tempDouble,
                                        Reference = tempDouble
                                    };
                                    calcStack.Push(newPoi);
                                }
                                break;
                            case PolishItemType.CAL_NOTEQUAL:
                                if (operand1.Reference is ValueType)
                                {
                                    tempDouble = Convert.ToDouble(operand1.Reference) != Convert.ToDouble(operand2.Reference) ? 1 : 0;
                                    newPoi = new PolishItem()
                                    {
                                        Number = tempDouble,
                                        Reference = tempDouble
                                    };
                                    calcStack.Push(newPoi);
                                }
                                else
                                {
                                    tempDouble = (string)operand1.Reference != (string)operand2.Reference ? 1 : 0;
                                    newPoi = new PolishItem()
                                    {
                                        Number = tempDouble,
                                        Reference = tempDouble
                                    };
                                    calcStack.Push(newPoi);
                                }
                                break;
                            case PolishItemType.CAL_BIG:
                                if (operand1.Reference is ValueType)
                                {
                                    tempDouble = Convert.ToDouble(operand1.Reference) > Convert.ToDouble(operand2.Reference) ? 1 : 0;
                                    newPoi = new PolishItem()
                                    {
                                        Number = tempDouble,
                                        Reference = tempDouble
                                    };
                                    calcStack.Push(newPoi);
                                }
                                else
                                {
                                    tempDouble = String.Compare((string)operand1.Reference, (string)operand2.Reference) > 0 ? 1 : 0;
                                    newPoi = new PolishItem()
                                    {
                                        Number = tempDouble,
                                        Reference = tempDouble
                                    };
                                    calcStack.Push(newPoi);
                                }
                                break;
                            case PolishItemType.CAL_BIGEQUAL:
                                if (operand1.Reference is ValueType)
                                {
                                    tempDouble = Convert.ToDouble(operand1.Reference) >= Convert.ToDouble(operand2.Reference) ? 1 : 0;
                                    newPoi = new PolishItem()
                                    {
                                        Number = tempDouble,
                                        Reference = tempDouble
                                    };
                                    calcStack.Push(newPoi);
                                }
                                else
                                {
                                    tempDouble = String.Compare((string)operand1.Reference, (string)operand2.Reference) >= 0 ? 1 : 0;
                                    newPoi = new PolishItem()
                                    {
                                        Number = tempDouble,
                                        Reference = tempDouble
                                    };
                                    calcStack.Push(newPoi);
                                }
                                break;
                            case PolishItemType.CAL_SMALL:
                                if (operand1.Reference is ValueType)
                                {
                                    tempDouble = Convert.ToDouble(operand1.Reference) < Convert.ToDouble(operand2.Reference) ? 1 : 0;
                                    newPoi = new PolishItem()
                                    {
                                        Number = tempDouble,
                                        Reference = tempDouble
                                    };
                                    calcStack.Push(newPoi);
                                }
                                else
                                {
                                    tempDouble = String.Compare((string)operand1.Reference, (string)operand2.Reference) < 0 ? 1 : 0;
                                    newPoi = new PolishItem()
                                    {
                                        Number = tempDouble,
                                        Reference = tempDouble
                                    };
                                    calcStack.Push(newPoi);
                                }
                                break;
                            case PolishItemType.CAL_SMALLEQUAL:
                                if (operand1.Reference is ValueType)
                                {
                                    tempDouble = Convert.ToDouble(operand1.Reference) <= Convert.ToDouble(operand2.Reference) ? 1 : 0;
                                    newPoi = new PolishItem()
                                    {
                                        Number = tempDouble,
                                        Reference = tempDouble
                                    };
                                    calcStack.Push(newPoi);
                                }
                                else
                                {
                                    tempDouble = String.Compare((string)operand1.Reference, (string)operand2.Reference) <= 0 ? 1 : 0;
                                    newPoi = new PolishItem()
                                    {
                                        Number = tempDouble,
                                        Reference = tempDouble
                                    };
                                    calcStack.Push(newPoi);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            if (calcStack.Count != 1)
            {
                Utils.LogUtils.LogLine("求值器无法计算逆波兰式：" + polish, "PolishEvaluator", Utils.LogLevel.Error);
                throw new Exception("表达式有错误");
            }
            return calcStack.Peek().Reference;
        }

        /// <summary>
        /// 将逆波兰式转化为可计算的项
        /// </summary>
        /// <param name="polish">逆波兰式字符串</param>
        /// <param name="vsm">关于哪个调用堆栈做动作</param>
        /// <param name="ctx">求值上下文</param>
        /// <returns>可计算项目向量</returns>
        private static List<PolishItem> GetPolishItemList(string polish, StackMachine vsm, EvaluatableContext ctx)
        {
            var resVec = new List<PolishItem>();
            var polishItem = polish.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string item in polishItem)
            {
                PolishItem poi = null;
                Regex floatRegEx = new Regex(@"^(\-|\+)?\d+(\.\d+)?$");
                // 常数项
                if (floatRegEx.IsMatch(item))
                {
                    double numitem = Convert.ToDouble(item);
                    poi = new PolishItem()
                    {
                        Number = numitem,
                        Cluster = null,
                        ItemType = PolishItemType.CONSTANT,
                        Reference = numitem
                    };
                }
                // 字符串
                else if (item.StartsWith("\"") && item.EndsWith("\""))
                {
                    string trimItem = item.Substring(1, item.Length - 2);
                    poi = new PolishItem()
                    {
                        Number = 0.0f,
                        Cluster = trimItem,
                        ItemType = PolishItemType.STRING,
                        Reference = trimItem
                    };
                }
                // 变量时
                else if ((item.StartsWith("&") || item.StartsWith("$") || item.StartsWith("%")) && item.Length > 1)
                {
                    object varRef = ctx != null ? ctx.Fetch(item.Substring(1)) : Director.RunMana.Fetch(item, vsm);
                    if (varRef is ValueType)
                    {
                        poi = new PolishItem()
                        {
                            Number = Convert.ToDouble(varRef),
                            Cluster = null,
                            ItemType = PolishItemType.VAR_NUM,
                            Reference = varRef
                        };
                    }
                    else
                    {
                        poi = new PolishItem()
                        {
                            Number = 0.0f,
                            Cluster = Convert.ToString(varRef),
                            ItemType = PolishItemType.VAR_STRING,
                            Reference = varRef
                        };
                    }
                }
                else switch (item)
                {
                    case "+":
                        poi = new PolishItem()
                        {
                            Number = 0.0f,
                            Cluster = null,
                            ItemType = PolishItemType.CAL_PLUS,
                            Reference = null
                        };
                        break;
                    case "-":
                        poi = new PolishItem()
                        {
                            Number = 0.0f,
                            Cluster = null,
                            ItemType = PolishItemType.CAL_MINUS,
                            Reference = null
                        };
                        break;
                    case "*":
                        poi = new PolishItem()
                        {
                            Number = 0.0f,
                            Cluster = null,
                            ItemType = PolishItemType.CAL_MULTI,
                            Reference = null
                        };
                        break;
                    case "/":
                        poi = new PolishItem()
                        {
                            Number = 0.0f,
                            Cluster = null,
                            ItemType = PolishItemType.CAL_DIV,
                            Reference = null
                        };
                        break;
                    case "!":
                        poi = new PolishItem()
                        {
                            Number = 0.0f,
                            Cluster = null,
                            ItemType = PolishItemType.CAL_NOT,
                            Reference = null
                        };
                        break;
                    case "&&":
                        poi = new PolishItem()
                        {
                            Number = 0.0f,
                            Cluster = null,
                            ItemType = PolishItemType.CAL_ANDAND,
                            Reference = null
                        };
                        break;
                    case "||":
                        poi = new PolishItem()
                        {
                            Number = 0.0f,
                            Cluster = null,
                            ItemType = PolishItemType.CAL_OROR,
                            Reference = null
                        };
                        break;
                    case "<>":
                        poi = new PolishItem()
                        {
                            Number = 0.0f,
                            Cluster = null,
                            ItemType = PolishItemType.CAL_NOTEQUAL,
                            Reference = null
                        };
                        break;
                    case "==":
                        poi = new PolishItem()
                        {
                            Number = 0.0f,
                            Cluster = null,
                            ItemType = PolishItemType.CAL_EQUAL,
                            Reference = null
                        };
                        break;
                    case ">":
                        poi = new PolishItem()
                        {
                            Number = 0.0f,
                            Cluster = null,
                            ItemType = PolishItemType.CAL_BIG,
                            Reference = null
                        };
                        break;
                    case "<":
                        poi = new PolishItem()
                        {
                            Number = 0.0f,
                            Cluster = null,
                            ItemType = PolishItemType.CAL_SMALL,
                            Reference = null
                        };
                        break;
                    case ">=":
                        poi = new PolishItem()
                        {
                            Number = 0.0f,
                            Cluster = null,
                            ItemType = PolishItemType.CAL_BIGEQUAL,
                            Reference = null
                        };
                        break;
                    case "<=":
                        poi = new PolishItem()
                        {
                            Number = 0.0f,
                            Cluster = null,
                            ItemType = PolishItemType.CAL_SMALLEQUAL,
                            Reference = null
                        };
                        break;
                }
                if (poi != null)
                {
                    resVec.Add(poi);
                }
            }
            return resVec;
        }
    }
}
