using System.Collections.Generic;
using Yuri.YuriHalation.ScriptPackage;

namespace Yuri.YuriHalation.Command
{
    using HalaAttrList = List<KeyValuePair<string, KeyValuePair<ArgType, string>>>;

    /// <summary>
    /// 命令类：中断循环
    /// </summary>
    internal class BreakCommand : HalationSingleCommand
    {
        /// <summary>
        /// 中断循环
        /// </summary>
        /// <param name="line">命令的行</param>
        /// <param name="indent">对齐偏移</param>
        /// <param name="parent">所属的可运行包装</param>
        public BreakCommand(int line, int indent, RunnablePackage parent)
            : base(line, indent, parent)
        {
            base.Init(new HalaAttrList(), ActionPackageType.act_break);
        }
    }
}
