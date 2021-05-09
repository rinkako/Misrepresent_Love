﻿using System;
using System.Collections.Generic;
using Yuri.PlatformCore.Audio;
using Yuri.PlatformCore.Graphic;
using Yuri.Yuriri;

namespace Yuri.PlatformCore.VM
{
    /// <summary>
    /// 提供系统回滚的快照类
    /// </summary>
    internal sealed class RollbackableSnapshot
    {
        /// <summary>
        /// 字符串化方法
        /// </summary>
        /// <returns>该快照的唯一标识符</returns>
        public override string ToString()
        {
            return this.VMRef.StackName;
        }

        /// <summary>
        /// 调用堆栈的拷贝
        /// </summary>
        public StackMachine VMRef
        {
            get;
            set;
        }

        /// <summary>
        /// 屏幕状态的拷贝
        /// </summary>
        public ScreenManager ScreenStateRef
        {
            get;
            set;
        }

        /// <summary>
        /// 信号绑定字典的拷贝
        /// </summary>
        public Dictionary<string, List<Tuple<string, string>>> SemaphoreDict
        {
            get;
            set;
        }

        /// <summary>
        /// 全局变量上下文的拷贝
        /// </summary>
        public GlobalContextDAO globalDao
        {
            get;
            set;
        }

        /// <summary>
        /// 场景上下文的拷贝
        /// </summary>
        public SceneContextDAO sceneDao
        {
            get;
            set;
        }

        /// <summary>
        /// 重现动作的拷贝
        /// </summary>
        public SceneAction ReactionRef
        {
            get;
            set;
        }
        
        /// <summary>
        /// 音乐状态的拷贝
        /// </summary>
        public MusicianDescriptor MusicRef
        {
            get;
            set;
        }

        /// <summary>
        /// 回顾状态的拷贝
        /// </summary>
        public BacklogHolder BacklogRef
        {
            get;
            set;
        }

        /// <summary>
        /// 是否分支的拷贝
        /// </summary>
        public bool IsBranchingRefer
        {
            get;
            set;
        }

        /// <summary>
        /// 获取或设置当前是否允许右键菜单
        /// </summary>
        public bool EnableRClickRef
        {
            get;
            set;
        }

        /// <summary>
        /// 获取或设置正在演出的章节名
        /// 该字段仅用在存档标记，和调用堆栈无关
        /// </summary>
        public string PerformingChapterRef
        {
            get;
            set;
        }

        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime TimeStamp
        {
            get;
            set;
        }
    }
}
