﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Navigation;
using Yuri.PageView;
using Yuri.Utils;
using Yuri.Yuriri;

namespace Yuri.PlatformCore.Graphic
{
    /// <summary>
    /// WPF页面导航器类：为游戏窗体提供对前端页的通用接口
    /// </summary>
    internal static class ViewPageManager
    {
        /// <summary>
        /// 在页面管理器中注册一个页面
        /// </summary>
        /// <param name="pageId">页面唯一标识符</param>
        /// <param name="pageRef">页引用</param>
        /// <returns>是否发生了覆盖</returns>
        public static bool RegisterPage(string pageId, Page pageRef)
        {
            LogUtils.LogLine("Register Page: " + pageId, "ViewPage Manager", LogLevel.Important);
            bool rFlag = ViewPageManager.pageDict.ContainsKey(pageId);
            ViewPageManager.pageDict[pageId] = pageRef;
            return rFlag;
        }

        /// <summary>
        /// 通过页面的唯一标识符获取页面的引用
        /// </summary>
        /// <param name="pageId">页面唯一标识符</param>
        /// <returns>页引用</returns>
        public static Page RetrievePage(string pageId)
        {
            return ViewPageManager.pageDict.ContainsKey(pageId) ? ViewPageManager.pageDict[pageId] : null;
        }

        /// <summary>
        /// 清空管理器中所有的WPF页引用
        /// </summary>
        public static void Clear()
        {
            ViewPageManager.pageDict.Clear();
        }

        /// <summary>
        /// 初始化第一个页面
        /// </summary>
        /// <param name="fp">首页引用</param>
        public static void InitFirstPage(Page fp)
        {
            ViewPageManager.PageCallStack.Push(fp);
        }

        /// <summary>
        /// 导航到目标页面
        /// </summary>
        /// <param name="toPageName">目标页面在页管理器里的唯一标识符</param>
        public static void NavigateTo(string toPageName)
        {
            // 不在主舞台就不处理调用堆栈
            if ((ViewPageManager.CurrentPage is PageView.Stage2D || ViewPageManager.CurrentPage is PageView.Stage3D) &&
                toPageName != GlobalConfigContext.FirstViewPage)
            {
                Director.PauseUpdateContext();
            }
            var rp = ViewPageManager.RetrievePage(toPageName);
            try
            {
                if (rp != null && ViewPageManager.CurrentPage != null)
                {
                    NavigationService.GetNavigationService(ViewPageManager.CurrentPage)?.Navigate(rp);
                    ViewPageManager.PageCallStack.Push(rp);
                }
                else
                {
                    LogUtils.LogLine(string.Format("Cannot find page: {0}, Navigation service ignored.", toPageName),
                        "ViewPageManager", LogLevel.Error);
                    Director.GetInstance().GetMainRender().Shutdown();
                }
            }
            catch (Exception ex)
            {
                LogUtils.LogLine(string.Format("Cannot find page: {0}, Navigation service ignored. {1}", toPageName, ex),
                        "ViewPageManager", LogLevel.Error);
                Director.GetInstance().GetMainRender().Shutdown();
            }
            // 如果目标页是主舞台就恢复处理调用堆栈
            if (toPageName == GlobalConfigContext.FirstViewPage)
            {
                Director.ResumeUpdateContext();
            }
        }

        /// <summary>
        /// 返回导航前的页面
        /// </summary>
        public static void GoBack()
        {
            try
            {
                if (ViewPageManager.CurrentPage != null && ViewPageManager.CurrentPage.NavigationService != null &&
                ViewPageManager.CurrentPage.NavigationService.CanGoBack)
                {
                    ViewPageManager.CurrentPage.NavigationService.GoBack();
                    ViewPageManager.PageCallStack.Pop();
                }
                else
                {
                    LogUtils.LogLine(string.Format("Cannot go back from page: {0}, Navigation service ignored.", ViewPageManager.CurrentPage?.Name),
                        "ViewPageManager", LogLevel.Error);
                    Director.GetInstance().GetMainRender().Shutdown();
                }
                if (ViewPageManager.CurrentPage is PageView.Stage3D || ViewPageManager.CurrentPage is PageView.Stage2D)
                {
                    Director.ResumeUpdateContext();
                }
            }
            catch (Exception ex)
            {
                LogUtils.LogLine(string.Format("Cannot go back from page: {0}, Navigation service ignored. {1}", ViewPageManager.CurrentPage?.Name, ex),
                        "ViewPageManager", LogLevel.Error);
                Director.GetInstance().GetMainRender().Shutdown();
            }    
        }

        /// <summary>
        /// 显示一个用户自定义UI页到UIFrame
        /// </summary>
        /// <param name="uiPageName">页面的名字</param>
        /// <returns>页面是否存在</returns>
        public static bool ShowUIPage(string uiPageName)
        {
            try
            {
                var up = ViewPageManager.RetrievePage(uiPageName);
                if (up == null)
                {
                    if (ViewPageManager.typeDict.ContainsKey(uiPageName))
                    {
                        var pageType = ViewPageManager.typeDict[uiPageName];
                        var pageObj = (Page)Activator.CreateInstance(pageType);
                        ViewPageManager.RegisterPage(uiPageName, pageObj);
                        ViewManager.mWnd.uiFrame.Visibility = System.Windows.Visibility.Visible;
                        ViewManager.mWnd.uiFrame.Content = pageObj;
                        return true;
                    }
                    return false;
                }
                ViewManager.mWnd.uiFrame.Visibility = System.Windows.Visibility.Visible;
                ViewManager.mWnd.uiFrame.Content = up;
                (up as YuriPage).PrepareOpen();
                return true;
            }
            catch (Exception ex)
            {
                LogUtils.LogLine("Show UI Page in uiframe failed. " + ex, "ViewPageManager", LogLevel.Error);
                return false;
            }
        }

        /// <summary>
        /// 退出UIFrame
        /// </summary>
        public static void CollapseUIPage()
        {
            ViewManager.mWnd.uiFrame.Content = null;
            ViewManager.mWnd.uiFrame.Visibility = System.Windows.Visibility.Hidden;
        }
        
        /// <summary>
        /// 获取当前是否位于主舞台页面
        /// </summary>
        /// <returns>是否在主舞台</returns>
        public static bool IsAtMainStage()
        {
            return ViewPageManager.PageCallStack.Count > 0 && 
                (ViewPageManager.PageCallStack.Peek() is PageView.Stage3D || ViewPageManager.PageCallStack.Peek() is PageView.Stage2D);
        }

        public static bool ApplyPreviewRenderAction(SceneAction action)
        {
            bool skipFlag = false;
            foreach (var cp in ViewPageManager.PageCallStack)
            {
                if (cp is RenderablePage)
                {
                    skipFlag = ((RenderablePage)cp).PreviewSceneAction(action);
                }
            }
            if (ViewManager.mWnd?.uiFrame?.Content != null && ViewManager.mWnd?.uiFrame?.Content is RenderablePage)
            {
                skipFlag = ((RenderablePage)ViewManager.mWnd.uiFrame.Content).PreviewSceneAction(action);
            }
            return skipFlag;
        }

        public static void ApplyRenderAction(SceneAction action)
        {
            foreach (var cp in ViewPageManager.PageCallStack)
            {
                if (cp is RenderablePage)
                {
                    ((RenderablePage)cp).OnSceneActionDone(action);
                }
            }
            if (ViewManager.mWnd?.uiFrame?.Content != null && ViewManager.mWnd?.uiFrame?.Content is RenderablePage)
            {
                ((RenderablePage)ViewManager.mWnd.uiFrame.Content).OnSceneActionDone(action);
            }
        }

        /// <summary>
        /// 静态构造函数
        /// </summary>
        static ViewPageManager()
        {
            var typeVec = from t in YuririWorld.YuririReflector.YuriTypeVector
                where t.FullName.StartsWith("Yuri.PageView.") && !t.FullName.Contains("<>")
                select t;
            foreach (var t in typeVec)
            {
                ViewPageManager.typeDict.Add(t.Name, t);
            }
        }

        /// <summary>
        /// 获取当前呈现在屏幕上的页面
        /// </summary>
        public static Page CurrentPage => ViewPageManager.PageCallStack.Count > 0 ? ViewPageManager.PageCallStack.Peek() : null;

        /// <summary>
        /// 页面转移栈
        /// </summary>
        private static readonly Stack<Page> PageCallStack = new Stack<Page>();

        /// <summary>
        /// 前端页引用字典
        /// </summary>
        private static readonly Dictionary<string, Page> pageDict = new Dictionary<string, Page>();

        /// <summary>
        /// 前端页类型字典
        /// </summary>
        private static readonly Dictionary<string, Type> typeDict = new Dictionary<string, Type>();
    }
}
