using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace Yuri.PageView
{
    /// <summary>
    /// 定义标准Yuri UI Frame接口
    /// </summary>
    interface YuriPage
    {
        /// <summary>
        /// 在执行uipage切换UI页面时被系统主动调用
        /// </summary>
        void PrepareOpen();

        /// <summary>
        /// 在离开uipage时调用，不会被系统主动调用
        /// </summary>
        void PrepareClose();
    }
}
