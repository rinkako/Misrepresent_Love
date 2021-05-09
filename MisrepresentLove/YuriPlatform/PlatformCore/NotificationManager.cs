﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Yuri.PlatformCore.Graphic;

namespace Yuri.PlatformCore
{
    /// <summary>
    /// 系统级通知管理器
    /// </summary>
    internal static class NotificationManager
    {
        /// <summary>
        /// 发布一个成就通知到通知队列中并处理队列
        /// </summary>
        /// <param name="label">通知的标题</param>
        /// <param name="detail">通知的具体内容</param>
        /// <param name="icoFilename">通知的图标</param>
        public static void Notify(string label, string detail, string icoFilename = "", int pendingMs = 5000)
        {
            var nti = new NotifyMessageItem()
            {
                Label = label,
                Detail = detail,
                PendingMs = pendingMs,
                IconFilename = icoFilename
            };
            lock (NotificationManager.pendingMessages)
            {  
                NotificationManager.pendingMessages.Enqueue(nti);
            }
            NotificationManager.HandleNotificationQueue();
        }

        /// <summary>
        /// 发布一个系统级通知
        /// </summary>
        /// <param name="msg">通知内容</param>
        /// <param name="delayMS">显示的毫秒数</param>
        public static void SystemMessageNotify(string msg, int delayMS)
        {
            if (msgTimer.IsEnabled)
            {
                msgTimer.Stop();
            }
            NotificationManager.msgUI.Content = msg ?? String.Empty;
            NotificationManager.msgUI.Visibility = Visibility.Visible;
            msgTimer.Interval = TimeSpan.FromMilliseconds(delayMS);
            msgTimer.Tick += delegate
            {
                NotificationManager.msgUI.Visibility = Visibility.Hidden;
                msgTimer.Stop();
            };
            msgTimer.Start();
        }

        /// <summary>
        /// 初始化通知管理器
        /// </summary>
        public static void Init()
        {
            var view = ViewManager.mWnd;
            NotificationManager.msgUI = view.BO_MessageLabel;
            NotificationManager.BoxUI = view.BO_Information;
            NotificationManager.IcoUI = view.BO_Information_Image;
            NotificationManager.labelUI = view.BO_Information_Name;
            NotificationManager.detailUI = view.BO_Information_Detail;
            TransformGroup aniGroup = new TransformGroup();
            TranslateTransform XYTransformer = new TranslateTransform();
            aniGroup.Children.Add(XYTransformer);
            NotificationManager.BoxUI.RenderTransform = aniGroup;
            var sp = ResourceManager.GetInstance().GetPicture("NotificationBox.png", ResourceManager.FullImageRect);
            NotificationManager.BoxUI.Background = new ImageBrush(sp.SpriteBitmapImage);
        }

        /// <summary>
        /// 处理通知队列
        /// </summary>
        private static void HandleNotificationQueue()
        {
            // 正在播放通知就不要打断
            if (NotificationManager.IsNotificating)
            {
                return;
            }
            // 更新信息
            NotifyMessageItem nti;
            lock (NotificationManager.pendingMessages)
            {
                if (!NotificationManager.pendingMessages.Any())
                {
                    return;
                }
                nti = NotificationManager.pendingMessages.Dequeue();
            }
            NotificationManager.labelUI.Text = nti.Label;
            NotificationManager.detailUI.Text = nti.Detail;
            if (nti.IconFilename != String.Empty)
            {
                var icoRes = ResourceManager.GetInstance().GetPicture(nti.IconFilename, ResourceManager.FullImageRect);
                NotificationManager.IcoUI.Source = icoRes?.SpriteBitmapImage;
                NotificationManager.IcoUI.Visibility = Visibility.Visible;
            }
            else
            {
                NotificationManager.IcoUI.Visibility = Visibility.Hidden;
            }
            // 执行动画
            NotificationManager.IsNotificating = true;
            NotificationManager.ApplyNotificationAnimation(nti);
        }

        /// <summary>
        /// 执行通知动画
        /// </summary>
        private static void ApplyNotificationAnimation(NotifyMessageItem nti)
        {
            Storyboard story = new Storyboard();
            var beginRight = Canvas.GetRight(NotificationManager.BoxUI);
            var toRight = NotificationManager.DeltaBoxRight;
            DoubleAnimationUsingKeyFrames daukf_translate = new DoubleAnimationUsingKeyFrames();
            EasingDoubleKeyFrame k1_translate = new EasingDoubleKeyFrame(toRight, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(AnimationTimeMS)));
            EasingDoubleKeyFrame k2_translate = new EasingDoubleKeyFrame(toRight, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(AnimationTimeMS + nti.PendingMs)));
            EasingDoubleKeyFrame k3_translate = new EasingDoubleKeyFrame(beginRight, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(AnimationTimeMS * 2 + nti.PendingMs)));
            EasingDoubleKeyFrame k4_translate = new EasingDoubleKeyFrame(beginRight, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(AnimationTimeMS * 3 + nti.PendingMs)));
            k1_translate.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };
            k3_translate.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseIn };
            daukf_translate.KeyFrames.Add(k1_translate);
            daukf_translate.KeyFrames.Add(k2_translate);
            daukf_translate.KeyFrames.Add(k3_translate);
            daukf_translate.KeyFrames.Add(k4_translate);
            Storyboard.SetTarget(daukf_translate, NotificationManager.BoxUI);
            Storyboard.SetTargetProperty(daukf_translate, new PropertyPath(Canvas.RightProperty));
            DoubleAnimationUsingKeyFrames daukf_opacity = new DoubleAnimationUsingKeyFrames();
            EasingDoubleKeyFrame k1_opacity = new EasingDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(AnimationTimeMS)));
            EasingDoubleKeyFrame k2_opacity = new EasingDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(AnimationTimeMS + nti.PendingMs)));
            EasingDoubleKeyFrame k3_opacity = new EasingDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(AnimationTimeMS * 2 + nti.PendingMs)));
            EasingDoubleKeyFrame k4_opacity = new EasingDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(AnimationTimeMS * 3 + nti.PendingMs)));
            daukf_opacity.KeyFrames.Add(k1_opacity);
            daukf_opacity.KeyFrames.Add(k2_opacity);
            daukf_opacity.KeyFrames.Add(k3_opacity);
            daukf_opacity.KeyFrames.Add(k4_opacity);
            Storyboard.SetTarget(daukf_opacity, NotificationManager.BoxUI);
            Storyboard.SetTargetProperty(daukf_opacity, new PropertyPath(Canvas.OpacityProperty));
            story.Children.Add(daukf_translate);
            story.Children.Add(daukf_opacity);
            story.FillBehavior = FillBehavior.Stop;
            story.Completed += delegate
            {
                NotificationManager.IsNotificating = false;
                NotificationManager.HandleNotificationQueue();
            };
            story.Begin();
        }

        /// <summary>
        /// 获取或设置当前是否正在播放通知
        /// </summary>
        public static bool IsNotificating { get; set; } = false;

        /// <summary>
        /// 待处理消息队列
        /// </summary>
        private static readonly Queue<NotifyMessageItem> pendingMessages = new Queue<NotifyMessageItem>();

        /// <summary>
        /// 系统级通知计时器
        /// </summary>
        private static readonly DispatcherTimer msgTimer = new DispatcherTimer();

        /// <summary>
        /// 通知窗体容器的引用
        /// </summary>
        private static Grid BoxUI = null;

        /// <summary>
        /// 通知图标的引用
        /// </summary>
        private static Image IcoUI = null;

        /// <summary>
        /// 通知文本的引用
        /// </summary>
        private static TextBlock labelUI = null;

        /// <summary>
        /// 通知详情的引用
        /// </summary>
        private static TextBlock detailUI = null;

        /// <summary>
        /// 系统级通知的引用
        /// </summary>
        private static Label msgUI = null;

        /// <summary>
        /// 通知窗体距离右版边的距离
        /// </summary>
        public static double DeltaBoxRight = 50;

        /// <summary>
        /// 单趟动画的毫秒数
        /// </summary>
        public static int AnimationTimeMS = 1000;

        /// <summary>
        /// 消息显示的毫秒数
        /// </summary>
        //public static int PendingTimeMS = 5000;

        /// <summary>
        /// 队列中等待显示的通知
        /// </summary>
        private sealed class NotifyMessageItem
        {
            /// <summary>
            /// 消息显示的毫秒数
            /// </summary>
            public int PendingMs { get; set; } = 5000;

            /// <summary>
            /// 获取或设置消息标签
            /// </summary>
            public string Label { get; set; }

            /// <summary>
            /// 获取或设置消息详情
            /// </summary>
            public string Detail { get; set; }

            /// <summary>
            /// 获取或设置图标资源名
            /// </summary>
            public string IconFilename { get; set; }
        }
    }
}
