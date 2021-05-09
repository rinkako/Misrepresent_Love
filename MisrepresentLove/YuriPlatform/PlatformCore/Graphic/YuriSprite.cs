﻿using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using Yuri.PlatformCore.VM;
using Yuri.Utils;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Yuri.PlatformCore.Graphic
{
    /// <summary>
    /// 精灵类：为图形资源提供展示、用户互动和动画效果的类
    /// </summary>
    internal class YuriSprite
    {
        private System.Windows.Forms.Timer autoAnimationTimer = null;

        private MemoryStream memoryStreamRef = null;

        /// <summary>
        /// 初始化精灵对象，它只能被执行一次
        /// </summary>
        /// <param name="resName">资源名称</param>
        /// <param name="resType">资源类型</param>
        /// <param name="ms">材质的内存流</param>
        /// <param name="cutrect">材质切割矩形</param>
        public void Init(string resName, ResourceType resType, MemoryStream ms, Int32Rect? cutrect = null)
        {
            this.memoryStreamRef = ms;
            if (!this.IsInit)
            {
                this.ResourceName = resName;
                this.ResourceType = resType;
                var highPerformanceFlag = PersistContextDAO.Fetch("system_config_performance_set")?.ToString() == "Disable";
                if (highPerformanceFlag || resType != ResourceType.Stand || resName.StartsWith("@") || !resName.Contains("_@"))
                {   
                    this.SpriteBitmapImage = new BitmapImage();
                    this.SpriteBitmapImage.BeginInit();
                    this.SpriteBitmapImage.StreamSource = ms;
                    if (cutrect != null)
                    {
                        this.CutRect = (Int32Rect)cutrect;
                        this.SpriteBitmapImage.SourceRect = this.CutRect;
                    }
                    this.SpriteBitmapImage.EndInit();
                    this.IsInit = true;
                    // 加载Mask
                    var rm = ResourceManager.GetInstance();
                    var maskPath = GlobalConfigContext.PictureMaskPrefix + resName;
                    this.IsMaskTriggerable = rm.IsResourceExist(maskPath, ResourceType.Pictures);
                    if (this.IsMaskTriggerable)
                    {
                        this.MaskSprite = rm.GetPicture(maskPath, ResourceManager.FullImageRect);
                    }
                    this.IsAutoAnimateable = false;
                }
                // 如果是立绘，就看是否需要加载帧动画
                else
                {
                    this.IsAutoAnimateable = true;
                    var splitor = resName.LastIndexOf(".");
                    var prefix = resName.Substring(0, splitor);
                    var postfix = resName.Substring(splitor, resName.Length - splitor);
                    this.AutoAnimationCount = 4;
                    this.animationImageList = new List<BitmapImage>();
                    var rm = ResourceManager.GetInstance();
                    for (int i = 1; i <= this.AutoAnimationCount; i++)
                    {
                        var actualName = $"@{prefix}@{i}{postfix}";
                        var tSprite = rm.GetCharacterStand(actualName, ResourceManager.FullImageRect);
                        this.animationImageList.Add(tSprite.SpriteBitmapImage);
                    }
                    this.autoAnimationTimer = new System.Windows.Forms.Timer
                    {
                        Interval = 5000
                    };
                    this.autoAnimationTimer.Tick += AutoAnimationTimer_Tick;
                    this.autoAnimationTimer.Start();
                    this.SpriteBitmapImage = this.animationImageList[0];
                    this.IsInit = true;

                    // fast wink when loaded
                    try
                    {
                        this.AutoAnimationTimer_Tick(null, null);
                    } catch (Exception e)
                    {
                        LogUtils.LogLine("fast wink failed: " + e.ToString(), "YuriSprite", LogLevel.Warning);
                    }
                }
            }
            else
            {
                LogUtils.LogLine($"Sprite Init again: {resName}", "MySprite", LogLevel.Error);
            }
        }

        private int winkAnimationCounter = 0;
        private readonly object winkingLock = new object();

        private void AutoAnimationTimer_Tick(object sender, EventArgs e)
        {
            lock (this.winkingLock)
            {
                var randomDelayTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(new Random().Next(0, 3000)) };
                randomDelayTimer.Tick += (_rsender, _re) =>
                {
                    
                    try
                    {
                        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(70) };
                        timer.Start();
                        timer.Tick += (_sender, _e) =>
                        {
                            try
                            {
                                if (winkAnimationCounter >= this.AutoAnimationCount)
                                {
                                    winkAnimationCounter = 0;
                                    ((Image)this.DisplayBinding).Source = this.animationImageList[0];
                                    timer.Stop();
                                }
                            ((Image)this.DisplayBinding).Source = this.animationImageList[winkAnimationCounter];
                                winkAnimationCounter++;
                            }
                            catch (Exception ee)
                            {
                                timer.IsEnabled = false;
                                return;
                            }
                        };
                        randomDelayTimer.Stop();
                    } catch (Exception outEx)
                    {
                        randomDelayTimer.IsEnabled = false;
                        return;
                    }
                };
                randomDelayTimer.Start();
            }
        }

        /// <summary>
        /// 初始化精灵对象，它只能被执行一次
        /// </summary>
        /// <param name="resName">资源名称</param>
        /// <param name="resType">资源类型</param>
        /// <param name="uri">材质的路径</param>
        /// <param name="cutrect">材质切割矩形</param>
        public void Init(string resName, ResourceType resType, Uri uri, Int32Rect? cutrect = null)
        {
            if (!this.IsInit)
            {
                this.ResourceName = resName;
                this.ResourceType = resType;
                this.SpriteBitmapImage = new BitmapImage();
                this.SpriteBitmapImage.BeginInit();
                this.SpriteBitmapImage.UriSource = uri;
                if (cutrect != null)
                {
                    this.CutRect = (Int32Rect)cutrect;
                    this.SpriteBitmapImage.SourceRect = this.CutRect;
                }
                this.SpriteBitmapImage.EndInit();
                this.IsInit = true;
            }
            else
            {
                LogUtils.LogLine($"Sprite Init again: {resName}", "MySprite", LogLevel.Error);
            }
        }

        public void OnDispose()
        {
            lock (this.winkingLock)
            {
                this.DisplayBinding = null;
                this.memoryStreamRef = null;
                if (this.IsAutoAnimateable && this.autoAnimationTimer != null)
                {
                    this.autoAnimationTimer.Enabled = false;
                    this.autoAnimationTimer.Stop();
                }
            }
        }

        /// <summary>
        /// 获取一个相对于左上角的像素点的颜色
        /// </summary>
        /// <param name="X">检测点X坐标</param>
        /// <param name="Y">检测点Y坐标</param>
        /// <returns>一个ARGB描述的Color实例</returns>
        public Color GetPixelColor(double X, double Y)
        {
            Color c = Color.FromArgb(Byte.MaxValue, 0, 0, 0);
            if (this.SpriteBitmapImage != null)
            {
                try
                {
                    CroppedBitmap cb = new CroppedBitmap(this.SpriteBitmapImage, new Int32Rect((int)X, (int)Y, 1, 1));
                    byte[] pixels = new byte[4];
                    cb.CopyPixels(pixels, 4, 0);
                    c = Color.FromArgb(pixels[3], pixels[2], pixels[1], pixels[0]);
                }
                catch (Exception) { }
            }
            return c;
        }

        /// <summary>
        /// 判断一个相对于左上角的像素点是否全透明
        /// </summary>
        /// <param name="X">检测点X坐标</param>
        /// <param name="Y">检测点Y坐标</param>
        /// <param name="threshold">透明度阈值</param>
        /// <returns>该点是否不超过透明阈值</returns>
        public bool IsEmptyRegion(double X, double Y, int threshold = 0)
        {
            return this.GetPixelColor(X, Y).A <= threshold;
        }

        /// <summary>
        /// 初始化精灵的动画依赖
        /// </summary>
        public void InitAnimationRenderTransform()
        {
            TransformGroup aniGroup = new TransformGroup();
            TranslateTransform XYTransformer = new TranslateTransform();
            ScaleTransform ScaleTransformer = new ScaleTransform();
            ScaleTransformer.CenterX = this.AnchorX;
            ScaleTransformer.CenterY = this.AnchorY;
            RotateTransform RotateTransformer = new RotateTransform();
            RotateTransformer.CenterX = this.AnchorX;
            RotateTransformer.CenterY = this.AnchorY;
            aniGroup.Children.Add(XYTransformer);
            aniGroup.Children.Add(ScaleTransformer);
            aniGroup.Children.Add(RotateTransformer);
            this.DisplayBinding.RenderTransform = aniGroup;
            this.TranslateTransformer = XYTransformer;
            this.RotateTransformer = RotateTransformer;
            this.ScaleTransformer = ScaleTransformer;
        }

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
        /// 获取或设置纹理切割矩形
        /// </summary>
        public Int32Rect CutRect { get; set; }

        /// <summary>
        /// 获取或设置纹理源
        /// </summary>
        public BitmapImage SpriteBitmapImage { get; set; }

        /// <summary>
        /// 获取或设置前端显示控件
        /// </summary>
        public FrameworkElement DisplayBinding
        {
            get
            {
                return this.viewBinding;
            }
            set
            {
                this.viewBinding = value;
            }
        }

        /// <summary>
        /// 获取或设置前端显示控件的X值
        /// </summary>
        public double DisplayX
        {
            get
            {
                return Canvas.GetLeft(this.DisplayBinding);
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
        /// 获取或设置前端显示控件的Y值
        /// </summary>
        public double DisplayY
        {
            get
            {
                return Canvas.GetTop(this.DisplayBinding);
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
        /// 获取或设置前端显示控件的Z值
        /// </summary>
        public int DisplayZ
        {
            get
            {
                return Canvas.GetZIndex(this.DisplayBinding);
            }
            set
            {
                Canvas.SetZIndex(this.DisplayBinding, value);
            }
        }

        /// <summary>
        /// 获取或设置前端显示控件的透明度
        /// </summary>
        public double DisplayOpacity
        {
            get
            {
                return this.DisplayBinding.Opacity;
            }
            set
            {
                this.DisplayBinding.Opacity = value;
            }
        }

        /// <summary>
        /// 获取或设置前端显示控件的宽度
        /// </summary>
        public double DisplayWidth
        {
            get
            {
                return this.DisplayBinding.Width;
            }
            set
            {
                this.DisplayBinding.Width = value;
            }
        }

        /// <summary>
        /// 获取或设置前端显示控件的高度
        /// </summary>
        public double DisplayHeight
        {
            get
            {
                return this.DisplayBinding.Height;
            }
            set
            {
                this.DisplayBinding.Height = value;
            }
        }

        /// <summary>
        /// 获取源图片的宽度
        /// </summary>
        public double ImageWidth => this.SpriteBitmapImage.Width;

        /// <summary>
        /// 获取源图片的高度
        /// </summary>
        public double ImageHeight => this.SpriteBitmapImage.Height;

        /// <summary>
        /// 获取当前精灵是否被绑定到Image前端对象上
        /// </summary>
        public bool IsDisplaying => this.DisplayBinding != null;

        /// <summary>
        /// 获取精灵是否被缩放
        /// </summary>
        public bool IsScaling => this.Descriptor.ScaleX != 1 || this.Descriptor.ScaleY != 1;

        /// <summary>
        /// 获取精灵的资源类型
        /// </summary>
        public ResourceType ResourceType
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取精灵的资源名
        /// </summary>
        public string ResourceName
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取是否自动动画立绘
        /// </summary>
        public bool IsAutoAnimateable
        {
            get;
            private set;
        }

        /// <summary>
        /// 立绘动画张数
        /// </summary>
        public int AutoAnimationCount
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取或设置正在进行的动画数量
        /// </summary>
        public int AnimateCount
        {
            get
            {
                return this.animateCounter;
            }
            set
            {
                this.animateCounter = Math.Max(0, value);
            }
        }

        /// <summary>
        /// 获取精灵是否已经初始化
        /// </summary>
        public bool IsInit
        {
            get;
            private set;
        }

        /// <summary>
        /// 是否有Mask图样用于触发点击中断
        /// </summary>
        public bool IsMaskTriggerable
        {
            get;
            private set;
        }

        /// <summary>
        /// Mask图样精灵
        /// </summary>
        public YuriSprite MaskSprite {
            get;
            private set;
        }

        /// <summary>
        /// 获取或设置精灵的描述子
        /// </summary>
        public SpriteDescriptor Descriptor
        {
            get;
            set;
        }

        /// <summary>
        /// 获取或设置绑定的平移变换器
        /// </summary>
        public TranslateTransform TranslateTransformer
        {
            get;
            set;
        }

        /// <summary>
        /// 获取或设置绑定的缩放变换器
        /// </summary>
        public ScaleTransform ScaleTransformer
        {
            get;
            set;
        }

        /// <summary>
        /// 获取或设置绑定的旋转变换器
        /// </summary>
        public RotateTransform RotateTransformer
        {
            get;
            set;
        }

        /// <summary>
        /// 获取或设置背景层实际显示控件的引用
        /// </summary>
        public FrameworkElement AnimationElement
        {
            get;
            set;
        }

        private List<BitmapImage> animationImageList = null;

        /// <summary>
        /// 提供精灵按钮鼠标松开时的处理函数
        /// </summary>
        /// <param name="sender">触发者</param>
        /// <param name="e">鼠标参数</param>
        public void MouseDownHandler(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            if (!YuriSprite.RenderRef.IsShowingDialog && !YuriSprite.RenderRef.IsBranching 
                && e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                this.isMousePressing = true;
            }
        }

        /// <summary>
        /// 提供精灵按钮鼠标松开时的处理函数
        /// </summary>
        /// <param name="sender">触发者</param>
        /// <param name="e">鼠标参数</param>
        public void MouseUpHandler(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (this.MaskSprite != null && this.isMousePressing && 
                Director.RunMana.GameState(Director.RunMana.CallStack) == StackMachineState.WaitUser)
            {
                // 计算点击在Mask上的颜色
                var pos = e.GetPosition(this.DisplayBinding);
                var maskColor = this.MaskSprite.GetPixelColor(pos.X, pos.Y).ToString();
                if (maskColor != "#FFFFFFFF")
                {
                    if (!GlobalConfigContext.MaskSpirit_ColorMapDict.ContainsKey(maskColor))
                    {
                        return;
                    }
                    var mappedId = GlobalConfigContext.MaskSpirit_ColorMapDict[maskColor];
                    // 根据Mask的颜色映射构造跳转中断
                    this.Ntr = new Interrupt()
                    {
                        Detail = "SpriteMaskInterrupt",
                        InterruptSA = null,
                        Type = InterruptType.ButtonJump,
                        InterruptFuncSign = string.Empty,
                        ReturnTarget = $"Mask_{this.Descriptor.Id}_{mappedId}",
                        PureInterrupt = false,
                        ExitWait = false
                    };
                    LogUtils.LogLine("Trigger mask event " + maskColor, "YuriSprite", LogLevel.Important);
                    Director.GetInstance().SubmitInterrupt(Director.RunMana.CallStack, this.Ntr);
                }
            }
            this.isMousePressing = false;
        }

        /// <summary>
        /// 是否鼠标正在按下
        /// </summary>
        private bool isMousePressing = false;

        /// <summary>
        /// 精灵动画状态
        /// </summary>
        private int animateCounter = 0;

        /// <summary>
        /// 精灵动画锚点类型
        /// </summary>
        private SpriteAnchorType anchorType = SpriteAnchorType.Center;

        /// <summary>
        /// 前端控件绑定
        /// </summary>
        private FrameworkElement viewBinding = null;

        /// <summary>
        /// 获取或设置按下时的中断，仅MASK时用
        /// </summary>
        public Interrupt Ntr { get; set; }

        /// <summary>
        /// 生成这个按钮的堆栈，仅MASK时用
        /// </summary>
        public StackMachine InterruptVSM { get; set; } = null;

        /// <summary>
        /// 渲染器引用
        /// </summary>
        private static UpdateRender RenderRef = Director.GetInstance().GetMainRender();
    }

    /// <summary>
    /// 枚举：精灵的动画锚点
    /// </summary>
    internal enum SpriteAnchorType
    {
        LeftTop,
        Center
    }
}
