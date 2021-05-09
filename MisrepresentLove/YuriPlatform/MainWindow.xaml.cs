﻿using log4net;
using System;
using System.Windows;
using System.Windows.Input;
using Yuri.PageView;
using Yuri.PlatformCore;
using Yuri.PlatformCore.Graphic;
using Yuri.PlatformCore.Semaphore;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace Yuri
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(MainWindow));

        /// <summary>
        /// 导演类的引用
        /// </summary>
        private readonly Director world = Director.GetInstance();

        /// <summary>
        /// Alt键正在被按下的标记
        /// </summary>
        private static bool altDown = false;

        /// <summary>
        /// 是否初始化完毕
        /// </summary>
        private static bool initFlag = false;

        /// <summary>
        /// 构造器
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            ViewManager.SetWindowReference(this);
            this.Title = GlobalConfigContext.GAME_TITLE_NAME;
            this.Width = GlobalConfigContext.GAME_VIEWPORT_WIDTH;
            this.Height = GlobalConfigContext.GAME_VIEWPORT_ACTUALHEIGHT;
            this.mainCanvas.Width = GlobalConfigContext.GAME_WINDOW_WIDTH;
            this.mainCanvas.Height = GlobalConfigContext.GAME_WINDOW_HEIGHT;
            this.ResizeMode = GlobalConfigContext.GAME_WINDOW_RESIZEABLE ? ResizeMode.CanResize : ResizeMode.NoResize;
            // 加载主页面
            this.mainFrame.Width = GlobalConfigContext.GAME_WINDOW_WIDTH;
            this.mainFrame.Height = GlobalConfigContext.GAME_WINDOW_HEIGHT;
            ViewPageManager.RegisterPage("SplashPage", new SplashPage());
            ViewPageManager.RegisterPage("LHPage", new LHPage());
            ViewPageManager.RegisterPage("LHStartPage", new LHStartPage());
            this.maskFrame.Width = GlobalConfigContext.GAME_WINDOW_WIDTH;
            this.maskFrame.Height = GlobalConfigContext.GAME_WINDOW_HEIGHT;
            this.uiFrame.Width = GlobalConfigContext.GAME_WINDOW_WIDTH;
            this.uiFrame.Height = GlobalConfigContext.GAME_WINDOW_HEIGHT;
            
            ViewManager.MaskFrameRef = this.maskFrame;
            InputMethod.SetIsInputMethodEnabled(this, false);
            this.mainFrame.Content = ViewPageManager.RetrievePage("SplashPage");

            if (GlobalConfigContext.GAME_WINDOW_FULLSCREEN)
            {
                Director.IsFullScreen = true;
                this.FullScreenTransform();
            }
            logger.Info("main window loaded");
        }

        /// <summary>
        /// 强制跳转到主舞台
        /// </summary>
        public void GoToMainStage()
        {
            if (MainWindow.initFlag == false)
            {
                if (GlobalConfigContext.GAME_IS3D)
                {
                    this.world.SetStagePageReference(new Stage3D());
                }
                else
                {
                    this.world.SetStagePageReference(new Stage2D());
                }
                // 预注册保存和读取页面
                //ViewPageManager.RegisterPage("SavePage", new SLPage(isSave: true));
                //ViewPageManager.RegisterPage("LoadPage", new SLPage(isSave: false));
                MainWindow.initFlag = true;
            }
            this.mainFrame.Content = ViewPageManager.RetrievePage(GlobalConfigContext.FirstViewPage);
            Director.ResumeUpdateContext();
        }
        
        #region 窗体监听事件
        /// <summary>
        /// 事件：窗体关闭
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!ViewPageManager.IsAtMainStage())
            {
                SplashPage.LoadingExitFlag = true;
                Director.CollapseWorld();
                return;
            }
            if (SemaphoreDispatcher.CountBinding("System_PreviewShutdown") > 0)
            {
                e.Cancel = true;
                SemaphoreDispatcher.Activate("System_PreviewShutdown");
            }
            else
            {
                Director.CollapseWorld();
            }
        }

        /// <summary>
        /// 事件：键盘即将按下按钮
        /// </summary>
        private void window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.SystemKey == Key.LeftAlt || e.SystemKey == Key.RightAlt)
            {
                MainWindow.altDown = true;
            }
            else if (e.SystemKey == Key.F4 && MainWindow.altDown)
            {
                this.world.GetMainRender().Shutdown();
            }
            else if (e.SystemKey == Key.Enter && MainWindow.altDown && this.IsKeyAltWindowSizeEnabled)
            {
                if (Director.IsFullScreen)
                {
                    Director.IsFullScreen = !Director.IsFullScreen;
                    this.WindowScreenTransform();
                }
                else
                {
                    Director.IsFullScreen = !Director.IsFullScreen;
                    this.FullScreenTransform();
                }
            }
            SemaphoreDispatcher.Activate($"System_Key_{e.Key}");
        }

        /// <summary>
        /// 事件：键盘即将松开按钮
        /// </summary>
        private void window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.SystemKey == Key.LeftAlt || e.SystemKey == Key.RightAlt)
            {
                MainWindow.altDown = false;
            }
            SemaphoreDispatcher.Deactivate($"System_Key_{e.Key}");
        }
        
        /// <summary>
        /// 事件：窗口尺寸模式改变
        /// </summary>
        private void window_StateChanged(object sender, EventArgs e)
        {
            //if (this.WindowState == WindowState.Maximized)
            //{
            //    this.FullScreenTransform();
            //    Director.IsFullScreen = true;
            //}
        }

        /// <summary>
        /// 事件：键盘按下按钮
        /// </summary>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            this.world.UpdateKeyboard(e, true);
        }

        /// <summary>
        /// 事件：键盘松开按钮
        /// </summary>
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            this.world.UpdateKeyboard(e, false);
        }
        #endregion

        #region 辅助函数
        /// <summary>
        /// 切换到全屏模式
        /// </summary>
        public void FullScreenTransform()
        {
            this.WindowState = WindowState.Normal;
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;
            this.Topmost = true;
            this.Left = 0.0;
            this.Top = 0.0;
            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;
        }

        /// <summary>
        /// 切换到窗口模式
        /// </summary>
        public void WindowScreenTransform(int? providedWidth = null, int? providedHeight = null)
        {
            this.WindowState = WindowState.Normal;
            this.WindowStyle = WindowStyle.SingleBorderWindow;
            this.ResizeMode = ResizeMode.CanResize;
            this.Topmost = false;
            this.Left = 50.0;
            this.Top = 50.0;
            this.Width = providedWidth ?? GlobalConfigContext.GAME_WINDOW_WIDTH;
            this.Height = providedHeight ?? GlobalConfigContext.GAME_WINDOW_ACTUALHEIGHT;
        }

        public bool IsKeyAltWindowSizeEnabled { get; set; } = true;
        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Director.RunMana.IsAutoplaying = true;
            Director.RunMana.ExitUserWait();
            Director.RunMana.AutoPlayWait();
        }
    }
}
