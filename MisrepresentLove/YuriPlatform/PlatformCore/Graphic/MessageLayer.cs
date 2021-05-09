﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Yuri.PlatformCore.Graphic
{
    /// <summary>
    /// 文字层类：为游戏提供在某个区域描绘文字的服务
    /// </summary>
    internal class MessageLayer
    {
        /// <summary>
        /// 将文字层恢复初始状态
        /// </summary>
        public void Reset()
        {
            this.Text = String.Empty;
            this.X = this.Y = 0;
            this.Z = GlobalConfigContext.GAME_Z_MESSAGELAYER + Id;
            this.Opacity = 1.0f;
            this.Visibility = Visibility.Visible;
            this.Padding = GlobalConfigContext.GAME_MESSAGELAYER_PADDING;
            this.StyleReset();
        }

        /// <summary>
        /// 恢复默认文字层的样式
        /// </summary>
        public void StyleReset()
        {
            this.FontColor = GlobalConfigContext.GAME_FONT_COLOR;
            this.FontSize = GlobalConfigContext.GAME_FONT_FONTSIZE;
            this.FontName = GlobalConfigContext.GAME_FONT_NAME;
            this.LineHeight = GlobalConfigContext.GAME_FONT_LINEHEIGHT;
        }

        /// <summary>
        /// 获取或设置文字层id
        /// </summary>
        public int Id
        {
            get;
            set;
        }

        /// <summary>
        /// 获取或设置文字层的文本
        /// </summary>
        public string Text
        {
            get
            {
                return this.DisplayBinding.Text;
            }
            set
            {
                this.DisplayBinding.Text = value;
            }
        }

        /// <summary>
        /// 获取或设置文字层是否可见
        /// </summary>
        public Visibility Visibility
        {
            get
            {
                return this.DisplayBinding.Visibility;
            }
            set
            {
                this.DisplayBinding.Visibility = value;
            }
        }

        /// <summary>
        /// 设置文字层字体
        /// </summary>
        public string FontName
        {
            set
            {
                if (value == "default")
                {
                    this.DisplayBinding.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "Resources/#Source Han Serif CN SemiBold");
                }
                else
                {
                    this.DisplayBinding.FontFamily = new FontFamily(value);
                }
            }
        }

        /// <summary>
        /// 获取或设置文字层字号
        /// </summary>
        public double FontSize
        {
            get
            {
                this.DisplayBinding.FontStyle = new FontStyle();
                return this.DisplayBinding.FontSize;
            }
            set
            {
                this.DisplayBinding.FontSize = value;
            }
        }

        /// <summary>
        /// 获取或设置文字层的纯色颜色
        /// </summary>
        public Color FontColor
        {
            get
            {
                return ((SolidColorBrush)this.DisplayBinding.Foreground).Color;
            }
            set
            {
                this.DisplayBinding.Foreground = new SolidColorBrush(value);
            }
        }

        /// <summary>
        /// 获取或设置行距
        /// </summary>
        public double LineHeight
        {
            get
            {
                return this.DisplayBinding.LineHeight;
            }
            set
            {
                this.DisplayBinding.LineHeight = value;
            }
        }

        /// <summary>
        /// 获取或设置文字层透明度
        /// </summary>
        public double Opacity
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
        /// 获取或设置文字层X坐标
        /// </summary>
        public double X
        {
            get
            {
                return Canvas.GetLeft(this.DisplayBinding);
            }
            set
            {
                Canvas.SetLeft(this.DisplayBinding, value);
            }
        }

        /// <summary>
        /// 获取或设置文字层Y坐标
        /// </summary>
        public double Y
        {
            get
            {
                return Canvas.GetTop(this.DisplayBinding);
            }
            set
            {
                Canvas.SetTop(this.DisplayBinding, value);
            }
        }

        /// <summary>
        /// 获取或设置文字层深度坐标
        /// </summary>
        public int Z
        {
            get
            {
                return Panel.GetZIndex(this.DisplayBinding);
            }
            set
            {
                Panel.SetZIndex(this.DisplayBinding, value);
            }
        }

        /// <summary>
        /// 获取或设置文字层高度
        /// </summary>
        public double Height
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
        /// 获取或设置文字层宽度
        /// </summary>
        public double Width
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
        /// 获取或设置文字层边距
        /// </summary>
        public Thickness Padding
        {
            get
            {
                return this.DisplayBinding.Padding;
            }
            set
            {
                this.DisplayBinding.Padding = value;
            }
        }

        /// <summary>
        /// 获取或设置文字层文本水平对齐属性
        /// </summary>
        public TextAlignment TextHorizontalAlignment
        {
            get
            {
                return this.DisplayBinding.TextAlignment;
            }
            set
            {
                this.DisplayBinding.TextAlignment = value;
            }
        }

        /// <summary>
        /// 获取或设置文字层在窗体中水平对齐属性
        /// </summary>
        public HorizontalAlignment HorizontalAlignment
        {
            get
            {
                return this.DisplayBinding.HorizontalAlignment;
            }
            set
            {
                this.DisplayBinding.HorizontalAlignment = value;
            }
        }

        /// <summary>
        /// 获取或设置文字层在窗体中竖直对齐属性
        /// </summary>
        public VerticalAlignment VerticalAlignment
        {
            get
            {
                return this.DisplayBinding.VerticalAlignment;
            }
            set
            {
                this.DisplayBinding.VerticalAlignment = value;
            }
        }

        /// <summary>
        /// 获取或设置文字层的阴影状态
        /// </summary>
        public bool FontShadow
        {
            get;
            set;
        }

        /// <summary>
        /// 获取或设置文字层的主文本块
        /// </summary>
        public TextBlock DisplayBinding
        {
            get;
            set;
        }

        /// <summary>
        /// 获取或设置文字层的背景精灵
        /// </summary>
        public YuriSprite BackgroundSprite
        {
            get
            {
                return this.bgSprite;
            }
            set
            {
                this.bgSprite = value;
            }
        }

        /// <summary>
        /// 文字层背景精灵
        /// </summary>
        private YuriSprite bgSprite = null;
    }
}
