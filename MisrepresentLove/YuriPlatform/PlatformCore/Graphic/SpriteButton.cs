﻿using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Yuri.PlatformCore.Semaphore;
using Yuri.PlatformCore.VM;

namespace Yuri.PlatformCore.Graphic
{
    /// <summary>
    /// 精灵按钮类
    /// </summary>
    internal class SpriteButton
    {
        /// <summary>
        /// 构造精灵按钮
        /// </summary>
        /// <param name="bid">按钮ID</param>
        public SpriteButton(int bid)
        {
            this.Id = bid;
            this.Ntr = null;
            this.DisplayBinding = null;
            this.Enable = true;
            this.Eternal = false;
            this.X = this.Y = 0;
            this.Z = GlobalConfigContext.GAME_Z_BUTTON;
            this.IsMouseOn = this.IsMouseOver = false;
        }

        /// <summary>
        /// 获取或设置按钮编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 获取或设置按下时的中断
        /// </summary>
        public Interrupt Ntr { get; set; }

        /// <summary>
        /// 获取或设置绑定前端显示控件
        /// </summary>
        public Image DisplayBinding { get; set; }

        /// <summary>
        /// 获取或设置按钮是否有效
        /// </summary>
        public bool Enable { get; set; }

        /// <summary>
        /// 获取或设置按钮是否在点击后仍存留屏幕上
        /// </summary>
        public bool Eternal { get; set; }

        /// <summary>
        /// 获取或设置按钮的X坐标
        /// </summary>
        public double X
        {
            get
            {
                return this.DisplayBinding == null ? 0 : Canvas.GetLeft(this.DisplayBinding);
            }
            set
            {
                if (this.DisplayBinding != null)
                {
                    Canvas.SetLeft(this.DisplayBinding, value);
                }
            }
        }

        /// <summary>
        /// 获取或设置按钮的Y坐标
        /// </summary>
        public double Y
        {
            get
            {
                return this.DisplayBinding == null ? 0 : Canvas.GetTop(this.DisplayBinding);
            }
            set
            {
                if (this.DisplayBinding != null)
                {
                    Canvas.SetTop(this.DisplayBinding, value);
                }
            }
        }

        /// <summary>
        /// 获取或设置按钮的Z坐标
        /// </summary>
        public int Z
        {
            get
            {
                return this.DisplayBinding == null ? 0 : Panel.GetZIndex(this.DisplayBinding);
            }
            set
            {
                if (this.DisplayBinding != null)
                {
                    Panel.SetZIndex(this.DisplayBinding, value);
                }
            }
        }

        /// <summary>
        /// 获取或设置正常时的按钮图像
        /// </summary>
        public YuriSprite ImageNormal { get; set; }

        /// <summary>
        /// 获取或设置鼠标按下时的按钮图像
        /// </summary>
        public YuriSprite ImageMouseOn { get; set; }

        /// <summary>
        /// 获取或设置鼠标悬停时的按钮图像
        /// </summary>
        public YuriSprite ImageMouseOver { get; set; }

        /// <summary>
        /// 获取鼠标是否悬停在按钮上
        /// </summary>
        public bool IsMouseOver { get; private set; }

        /// <summary>
        /// 获取鼠标是否按下按钮
        /// </summary>
        public bool IsMouseOn { get; private set; }

        /// <summary>
        /// 获取或设置精灵动画锚点
        /// </summary>
        public SpriteAnchorType Anchor
        {
            get
            {
                return this.anchorType;
            }
            set
            {
                this.anchorType = value;
                this.InitAnimationRenderTransform();
            }
        }

        /// <summary>
        /// 获取精灵锚点相对精灵左上角的X坐标
        /// </summary>
        public double AnchorX
        {
            get
            {
                if (this.DisplayBinding == null)
                {
                    return 0;
                }
                return this.Anchor == SpriteAnchorType.Center ? this.DisplayBinding.Width / 2 : 0;
            }
        }

        /// <summary>
        /// 获取精灵锚点相对精灵左上角的Y坐标
        /// </summary>
        public double AnchorY
        {
            get
            {
                if (this.DisplayBinding == null)
                {
                    return 0;
                }
                return this.Anchor == SpriteAnchorType.Center ? this.DisplayBinding.Height / 2 : 0;
            }
        }

        /// <summary>
        /// 初始化精灵的动画依赖
        /// </summary>
        public void InitAnimationRenderTransform()
        {
            TransformGroup aniGroup = new TransformGroup();
            TranslateTransform XYTransformer = new TranslateTransform();
            ScaleTransform ScaleTransformer = new ScaleTransform
            {
                CenterX = this.AnchorX,
                CenterY = this.AnchorY
            };
            RotateTransform RotateTransformer = new RotateTransform
            {
                CenterX = this.AnchorX,
                CenterY = this.AnchorY
            };
            aniGroup.Children.Add(XYTransformer);
            aniGroup.Children.Add(ScaleTransformer);
            aniGroup.Children.Add(RotateTransformer);
            this.DisplayBinding.RenderTransform = aniGroup;
        }

        /// <summary>
        /// 提供精灵按钮在鼠标移动时的处理函数
        /// </summary>
        /// <param name="sender">触发者</param>
        /// <param name="e">鼠标参数</param>
        public void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (this.Enable && this.DisplayBinding != null)
            {
                var pPos = e.GetPosition(this.DisplayBinding);
                var eFlag = this.ImageNormal.IsEmptyRegion(pPos.X, pPos.Y, 0);
                this.DisplayBinding.IsHitTestVisible = !eFlag;
                if (eFlag != this.lastPixelHitFlag)
                {
                    if (!eFlag)
                    {
                        BitmapImage myBitmapImage = this.ImageMouseOver.SpriteBitmapImage;
                        this.DisplayBinding.Width = myBitmapImage.PixelWidth;
                        this.DisplayBinding.Height = myBitmapImage.PixelHeight;
                        this.DisplayBinding.Source = myBitmapImage;
                    }
                    else
                    {
                        BitmapImage myBitmapImage = this.ImageNormal.SpriteBitmapImage;
                        this.DisplayBinding.Width = myBitmapImage.PixelWidth;
                        this.DisplayBinding.Height = myBitmapImage.PixelHeight;
                        this.DisplayBinding.Source = myBitmapImage;
                    }
                    this.lastPixelHitFlag = eFlag;
                }
            }
        }

        /// <summary>
        /// 提供精灵按钮鼠标离开时的处理函数
        /// </summary>
        /// <param name="sender">触发者</param>
        /// <param name="e">鼠标参数</param>
        public void MouseLeaveHandler(object sender, MouseEventArgs e)
        {
            if (this.Enable)
            {
                this.IsMouseOver = this.IsMouseOn = false;
                SemaphoreDispatcher.Deactivate($"System_Button_Over_{this.Id}");
                if (this.DisplayBinding != null && (this.ImageMouseOver != null || this.ImageMouseOn != null))
                {
                    BitmapImage myBitmapImage = this.ImageNormal.SpriteBitmapImage;
                    //this.ImageNormal.DisplayBinding = this.DisplayBinding;
                    this.DisplayBinding.Width = myBitmapImage.PixelWidth;
                    this.DisplayBinding.Height = myBitmapImage.PixelHeight;
                    this.DisplayBinding.Source = myBitmapImage;
                }
            }
        }

        /// <summary>
        /// 提供精灵按钮鼠标移入时的处理函数
        /// </summary>
        /// <param name="sender">触发者</param>
        /// <param name="e">鼠标参数</param>
        public void MouseEnterHandler(object sender, MouseEventArgs e)
        {
            if (this.Enable)
            {
                this.IsMouseOver = true;
                SemaphoreDispatcher.Activate($"System_Button_Over_{this.Id}");
                if (this.DisplayBinding != null && this.ImageMouseOver != null)
                {
                    BitmapImage myBitmapImage = this.ImageMouseOver.SpriteBitmapImage;
                    //this.ImageNormal.DisplayBinding = null;
                    //this.ImageMouseOn.DisplayBinding = null;
                    //this.ImageMouseOver.DisplayBinding = this.DisplayBinding;
                    this.DisplayBinding.Width = myBitmapImage.PixelWidth;
                    this.DisplayBinding.Height = myBitmapImage.PixelHeight;
                    this.DisplayBinding.Source = myBitmapImage;
                }
            }
        }

        /// <summary>
        /// 提供精灵按钮鼠标按下时的处理函数
        /// </summary>
        /// <param name="sender">触发者</param>
        /// <param name="e">鼠标参数</param>
        public void MouseOnHandler(object sender, MouseEventArgs e)
        {
            if (this.Enable && e.LeftButton == MouseButtonState.Pressed)
            {
                Director.IsButtonClicking = true;
                this.IsMouseOn = true;
                if (this.DisplayBinding != null && this.ImageMouseOn != null)
                {
                    BitmapImage myBitmapImage = this.ImageMouseOn.SpriteBitmapImage;
                    //this.ImageNormal.DisplayBinding = null;
                    //this.ImageMouseOn.DisplayBinding = this.DisplayBinding;
                    //this.ImageMouseOver.DisplayBinding = null;
                    this.DisplayBinding.Width = myBitmapImage.PixelWidth;
                    this.DisplayBinding.Height = myBitmapImage.PixelHeight;
                    this.DisplayBinding.Source = myBitmapImage;
                }
            }
        }

        /// <summary>
        /// 提供精灵按钮鼠标松开时的处理函数
        /// </summary>
        /// <param name="sender">触发者</param>
        /// <param name="e">鼠标参数</param>
        public void MouseUpHandler(object sender, MouseEventArgs e)
        {
            Director.IsButtonClicking = false;
            if (this.Enable)
            {
                if (this.IsMouseOn)
                {
                    // 标记为非回滚
                    RollbackManager.IsRollingBack = false;
                    if (this.DisplayBinding != null && this.IsMouseOver && this.ImageMouseOver != null)
                    {
                        BitmapImage myBitmapImage2 = this.ImageMouseOver.SpriteBitmapImage;
                        //this.ImageNormal.DisplayBinding = null;
                        //this.ImageMouseOn.DisplayBinding = null;
                        //this.ImageMouseOver.DisplayBinding = this.DisplayBinding;
                        this.DisplayBinding.Width = myBitmapImage2.PixelWidth;
                        this.DisplayBinding.Height = myBitmapImage2.PixelHeight;
                        this.DisplayBinding.Source = myBitmapImage2;
                        // 向运行时环境提交中断
                        Director.GetInstance().SubmitInterrupt(this.InterruptVSM ?? Director.RunMana.CallStack, this.Ntr);
                        // 移除按钮
                        if (!this.Eternal)
                        {
                            this.Enable = false;
                            Director.GetInstance().RemoveButton(this.Id);
                        }
                        return;
                    }
                    BitmapImage myBitmapImage = this.ImageNormal.SpriteBitmapImage;
                    //this.ImageNormal.DisplayBinding = this.DisplayBinding;
                    //this.ImageMouseOn.DisplayBinding = null;
                    //this.ImageMouseOver.DisplayBinding = null;
                    if (this.DisplayBinding != null)
                    {
                        this.DisplayBinding.Width = myBitmapImage.PixelWidth;
                        this.DisplayBinding.Height = myBitmapImage.PixelHeight;
                        this.DisplayBinding.Source = myBitmapImage;
                    }
                    // 向运行时环境提交中断
                    Director.GetInstance().SubmitInterrupt(this.InterruptVSM ?? Director.RunMana.CallStack, this.Ntr);
                    // 移除按钮
                    if (!this.Eternal)
                    {
                        this.Enable = false;
                        Director.GetInstance().RemoveButton(this.Id);
                    }
                }
                // 松开按钮
                this.IsMouseOn = false;
            }
        }

        /// <summary>
        /// 生成这个按钮的堆栈
        /// </summary>
        public StackMachine InterruptVSM { get; set; } = null;

        /// <summary>
        /// 上一次移动指针之前是否可点击
        /// </summary>
        private bool lastPixelHitFlag = false;

        /// <summary>
        /// 动画锚点类型
        /// </summary>
        private SpriteAnchorType anchorType = SpriteAnchorType.Center;
    }
}
