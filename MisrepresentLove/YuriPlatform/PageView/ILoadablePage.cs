using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yuri.PageView
{
    public interface ILoadablePage
    {
        void HandleResumeFromLoad();
    }
}
