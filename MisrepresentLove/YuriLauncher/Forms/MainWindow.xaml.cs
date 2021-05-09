﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using MahApps.Metro.Controls;
using Yuri.YuriLauncher.Utils;

namespace Yuri.YuriLauncher.Forms
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        /// <summary>
        /// 后台配置信息
        /// </summary>
        private readonly LauncherConfigPackage cp = new LauncherConfigPackage();

        /// <summary>
        /// 打字效果预览故事板
        /// </summary>
        private Storyboard MsgLayerTypingStory;

        /// <summary>
        /// 打字效果预览文本
        /// </summary>
        private readonly string TypingStr = "伊泽塔爱菲涅。" + Environment.NewLine + "abcABC123?!";

        /// <summary>
        /// 声音控制器
        /// </summary>
        private readonly Musician musicMana = Musician.GetInstance();

        /// <summary>
        /// 字体选择窗体
        /// </summary>
        private System.Windows.Forms.FontDialog fontDialog;

        /// <summary>
        /// 当前测试SE项目计数
        /// </summary>
        private int seCounter = 1;

        /// <summary>
        ///  当前测试Vocal项目计数
        /// </summary>
        private int voiceCounter = 1;

        /// <summary>
        /// 构造器
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            // 加载原始设置数据
            try
            {
                if (File.Exists(IOUtils.ParseURItoURL("YuriConfig.dat")))
                {
                    this.cp.ReadConfigData();
                    this.UpdateViewContext();
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show(@"缺失配置文件，无法使用Launcher");
                    Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(@"加载配置失败 " + Environment.NewLine + ex);
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// 在指定的文字层绑定控件上进行打字动画
        /// </summary>
        /// <param name="appendString">要追加的字符串</param>
        /// <param name="msglayBinding">文字层的控件</param>
        /// <param name="wordTimeSpan">字符之间的打字时间间隔</param>
        private void TypeWriter(string appendString, TextBlock msglayBinding, int wordTimeSpan)
        {
            MsgLayerTypingStory = new Storyboard();
            StringAnimationUsingKeyFrames stringAnimationUsingKeyFrames = new StringAnimationUsingKeyFrames();
            Duration aniDuration = new Duration(TimeSpan.FromMilliseconds(wordTimeSpan * appendString.Length + 2000));
            stringAnimationUsingKeyFrames.Duration = aniDuration;
            MsgLayerTypingStory.Duration = aniDuration;
            string tmp = String.Empty;
            int ctr = 0;
            foreach (char c in appendString)
            {
                var discreteStringKeyFrame = new DiscreteStringKeyFrame
                {
                    KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(ctr++ * wordTimeSpan))
                };
                tmp += c;
                discreteStringKeyFrame.Value = tmp;
                stringAnimationUsingKeyFrames.KeyFrames.Add(discreteStringKeyFrame);
            }
            Storyboard.SetTarget(stringAnimationUsingKeyFrames, msglayBinding);
            Storyboard.SetTargetProperty(stringAnimationUsingKeyFrames, new PropertyPath(TextBlock.TextProperty));
            MsgLayerTypingStory.Children.Add(stringAnimationUsingKeyFrames);
            MsgLayerTypingStory.RepeatBehavior = RepeatBehavior.Forever;
            MsgLayerTypingStory.Begin();
        }

        /// <summary>
        /// 根据设置文件更新前端信息
        /// </summary>
        private void UpdateViewContext()
        {
            // 画面模式
            this.radioButton_Screen_Resolution_1.IsChecked = false;
            this.radioButton_Screen_Resolution_2.IsChecked = false;
            this.radioButton_Screen_Resolution_3.IsChecked = false;
            this.radioButton_Screen_Resolution_4.IsChecked = false;
            if (this.cp["GameViewportWidth"] == "1920")
            {
                this.radioButton_Screen_Resolution_1.IsChecked = true;
            }
            else if (this.cp["GameViewportWidth"] == "1600")
            {
                this.radioButton_Screen_Resolution_2.IsChecked = true;
            }
            else if (this.cp["GameViewportWidth"] == "1024")
            {
                this.radioButton_Screen_Resolution_4.IsChecked = true;
            }
            else
            {
                this.radioButton_Screen_Resolution_3.IsChecked = true;
            }
            // 屏显模式
            this.radioButton_Screen_Window_1.IsChecked = false;
            this.radioButton_Screen_Window_2.IsChecked = false;
            if (this.cp["GameFullScreen"] == "True")
            {
                this.radioButton_Screen_Window_1.IsChecked = true;
            }
            else
            {
                this.radioButton_Screen_Window_2.IsChecked = true;
            }
            // 动画特效
            this.radioButton_Screen_Animation_1.IsChecked = false;
            this.radioButton_Screen_Animation_2.IsChecked = false;
            this.radioButton_Screen_Animation_3.IsChecked = false;
            if (this.cp["GamePerformance"] == "2")
            {
                radioButton_Screen_Animation_3.IsChecked = true;
            }
            else if (this.cp["GamePerformance"] == "1")
            {
                radioButton_Screen_Animation_2.IsChecked = true;
            }
            else
            {
                radioButton_Screen_Animation_1.IsChecked = true;
            }
            // 场景镜头
            this.radioButton_Screen_SCamera_1.IsChecked = false;
            this.radioButton_Screen_SCamera_2.IsChecked = false;
            if (this.cp["GameEnableSCamera"] == "False")
            {
                this.radioButton_Screen_SCamera_2.IsChecked = true;
            }
            else
            {
                this.radioButton_Screen_SCamera_1.IsChecked = true;
            }
            // 打字动画
            this.radioButton_Screen_Typing_1.IsChecked = false;
            this.radioButton_Screen_Typing_2.IsChecked = false;
            if (this.cp["GameMsgLayerTypeSpeed"] == "0")
            {
                this.radioButton_Screen_Typing_2.IsChecked = true;
                this.slider_Screen_Typing.Value = 0;
            }
            else
            {
                this.radioButton_Screen_Typing_1.IsChecked = true;
                this.slider_Screen_Typing.Value = Convert.ToInt32(this.cp["GameMsgLayerTypeSpeed"]);
            }
            // 字体
            try
            {
                this.textblock_Screen_Typing.FontFamily = new System.Windows.Media.FontFamily(this.cp["GameMsgLayerFontName"]);
            }
            catch (Exception)
            {
                this.textblock_Screen_Typing.FontFamily = new System.Windows.Media.FontFamily("黑体");
            }
            // 音效
            this.slider_Sound_BGM.Value = Math.Round(Convert.ToInt32(this.cp["GameMusicBGMVol"]) / 10.0);
            this.slider_Sound_BGS.Value = Math.Round(Convert.ToInt32(this.cp["GameMusicBGSVol"]) / 10.0);
            this.slider_Sound_SE.Value = Math.Round(Convert.ToInt32(this.cp["GameMusicSEVol"]) / 10.0);
            this.slider_Sound_Vocal.Value = Math.Round(Convert.ToInt32(this.cp["GameMusicVocalVol"]) / 10.0);
            toggleSwitch_Sound_Mute.IsChecked = this.cp["GameMute"] == "True";
            // 鼠标右键
            this.radioButton_Rclick_1.IsChecked = this.cp["GameRClickMode"] == "0";
            // 鼠标滚轮
            this.radioButton_RollingBack_1.IsChecked = this.cp["GameScrollingMode"] == "0";
            // 自动移动指针
            this.radioButton_Quickmove_1.IsChecked = this.cp["GameAutoPointer"] == "True";
        }

        /// <summary>
        /// 更新后台的配置信息
        /// </summary>
        private void UpdateConfigContext()
        {
            // 画面模式
            if (this.radioButton_Screen_Resolution_1.IsChecked == true)
            {
                this.cp["GameViewportWidth"] = "1920";
                this.cp["GameViewportHeight"] = "1080";
            }
            else if (this.radioButton_Screen_Resolution_2.IsChecked == true)
            {
                this.cp["GameViewportWidth"] = "1600";
                this.cp["GameViewportHeight"] = "900";
            }
            else if (this.radioButton_Screen_Resolution_4.IsChecked == true)
            {
                this.cp["GameViewportWidth"] = "1024";
                this.cp["GameViewportHeight"] = "576";
            }
            else
            {
                this.cp["GameViewportWidth"] = "1280";
                this.cp["GameViewportHeight"] = "720";
            }
            // 屏显模式
            if (radioButton_Screen_Window_1.IsChecked == true)
            {
                this.cp["GameFullScreen"] = "True";
            }
            else
            {
                this.cp["GameFullScreen"] = "False";
            }
            // 动画特效
            if (radioButton_Screen_Animation_3.IsChecked == true)
            {
                this.cp["GamePerformance"] = "2";
            }
            else if (radioButton_Screen_Animation_2.IsChecked == true)
            {
                this.cp["GamePerformance"] = "1";
            }
            else
            {
                this.cp["GamePerformance"] = "0";
            }
            // 场景镜头
            if (radioButton_Screen_SCamera_2.IsChecked == true)
            {
                this.cp["GameEnableSCamera"] = "False";
            }
            else
            {
                this.cp["GameEnableSCamera"] = "True";
            }
            // 打字动画
            if (radioButton_Screen_Typing_1.IsChecked == true)
            {
                this.cp["GameMsgLayerTypeSpeed"] = this.slider_Screen_Typing.Value.ToString("0");
            }
            else
            {
                this.cp["GameMsgLayerTypeSpeed"] = "0";
            }
            // 字体
            try
            {
                this.cp["GameMsgLayerFontName"] = textblock_Screen_Typing.FontFamily.FamilyNames.Last().Value.ToString();
            }
            catch (Exception)
            {
                this.cp["GameMsgLayerFontName"] = "思源宋体";
            }
            // 音效
            this.cp["GameMusicBGMVol"] = (this.slider_Sound_BGM.Value * 10).ToString("0");
            this.cp["GameMusicBGSVol"] = (this.slider_Sound_BGS.Value * 10).ToString("0");
            this.cp["GameMusicSEVol"] = (this.slider_Sound_SE.Value * 10).ToString("0");
            this.cp["GameMusicVocalVol"] = (this.slider_Sound_Vocal.Value * 10).ToString("0");
            this.cp["GameMute"] = toggleSwitch_Sound_Mute.IsChecked == true ? "True" : "False";
            // 鼠标右键
            if (this.radioButton_Rclick_1.IsChecked == true)
            {
                this.cp["GameRClickMode"] = "0";
            }
            else
            {
                this.cp["GameRClickMode"] = "1";
            }
            // 鼠标滚轮
            if (this.radioButton_RollingBack_1.IsChecked == true)
            {
                this.cp["GameScrollingMode"] = "0";
            }
            else
            {
                this.cp["GameScrollingMode"] = "1";
            }
            // 自动移动指针
            if (this.radioButton_Quickmove_1.IsChecked == true)
            {
                this.cp["GameAutoPointer"] = "True";
            }
            else
            {
                this.cp["GameAutoPointer"] = "False";
            }
        }

        /// <summary>
        /// 保存设置到文件
        /// </summary>
        private void SaveConfigContext()
        {
            try
            {
                this.UpdateConfigContext();
                this.cp.WriteSteady();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(@"保存配置失败" + Environment.NewLine + ex);
            }
        }

        #region 窗体事件
        private void radioButton_Screen_Animation_2_Checked(object sender, RoutedEventArgs e)
        {
            if (this.radioButton_Screen_Animation_2.IsChecked == true)
            {
                this.groupBox_Screen_SCamera.IsEnabled = false;
                this.radioButton_Screen_SCamera_2.IsChecked = true;
            }
            else
            {
                this.groupBox_Screen_SCamera.IsEnabled = true;
            }
        }

        private void radioButton_Screen_Animation_3_Checked(object sender, RoutedEventArgs e)
        {
            if (this.radioButton_Screen_Animation_3.IsChecked == true)
            {
                this.groupBox_Screen_SCamera.IsEnabled = false;
                this.radioButton_Screen_SCamera_2.IsChecked = true;
            }
            else
            {
                this.groupBox_Screen_SCamera.IsEnabled = true;
            }
        }

        private void radioButton_Screen_Animation_1_Checked(object sender, RoutedEventArgs e)
        {
            if (this.radioButton_Screen_Animation_1.IsChecked == true)
            {
                if (this.groupBox_Screen_SCamera != null)
                {
                    this.groupBox_Screen_SCamera.IsEnabled = true;
                    this.radioButton_Screen_SCamera_1.IsChecked = true;
                }
            }
        }

        private void Tab_Screen_OnLoaded(object sender, RoutedEventArgs e)
        {
            this.TypeWriter(TypingStr, this.textblock_Screen_Typing, (int)this.slider_Screen_Typing.Value);
        }

        private void radioButton_Screen_Typing_2_Checked(object sender, RoutedEventArgs e)
        {
            if (this.slider_Screen_Typing != null)
            {
                this.slider_Screen_Typing.Value = 0;
                this.slider_Screen_Typing.IsEnabled = false;
            }
        }

        private void radioButton_Screen_Typing_1_Checked(object sender, RoutedEventArgs e)
        {
            if (this.slider_Screen_Typing != null)
            {
                this.slider_Screen_Typing.Value = 60;
                this.slider_Screen_Typing.IsEnabled = true;
            }
        }

        private void slider_Screen_Typing_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.label_Screen_Typing != null)
            {
                this.label_Screen_Typing.Content = this.slider_Screen_Typing.Value.ToString("0");
                MsgLayerTypingStory?.Stop();
                this.TypeWriter(TypingStr, this.textblock_Screen_Typing, (int)this.slider_Screen_Typing.Value);
            }
        }

        private void ToggleSwitch_Sound_Mute_OnIsCheckedChanged(object sender, EventArgs e)
        {
            this.groupBox_Sound_BGM.IsEnabled = this.groupBox_Sound_BGS.IsEnabled = groupBox_Sound_SE.IsEnabled
                = this.groupBox_Sound_Vocal.IsEnabled = this.toggleSwitch_Sound_Mute.IsChecked != true;
            if (this.toggleSwitch_Sound_Mute.IsChecked == true)
            {
                this.musicMana.StopBGS();
                this.musicMana.StopAndReleaseBGM();
                this.musicMana.StopAndReleaseVocal();
            }
        }

        private void button_Sound_BGM_Try_Click(object sender, RoutedEventArgs e)
        {
            this.musicMana.PlayBGM("bgm.mp3", IOUtils.ParseURItoURL(@"LItem\bgm.mp3"),
                (int)(this.slider_Sound_BGM.Value) * 10);
        }

        private void button_Sound_BGM_TryEnd_Click(object sender, RoutedEventArgs e)
        {
            this.musicMana.StopAndReleaseBGM();
        }

        private void slider_Sound_BGM_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.label_Sound_BGM != null)
            {
                this.label_Sound_BGM.Content = this.slider_Sound_BGM.Value.ToString("0");
                if (this.musicMana.IsBGMPlaying)
                {
                    this.musicMana.SetBGMVolume((int)(this.slider_Sound_BGM.Value) * 10);
                }
            }
        }

        private void button_about_reference_Click(object sender, RoutedEventArgs e)
        {
            (new LicenseForm()).ShowDialog();
        }

        private void button_Sound_SE_Try_Click(object sender, RoutedEventArgs e)
        {
            this.musicMana.PlaySE(IOUtils.ParseURItoURL(String.Format("LItem\\se{0}.mp3", seCounter++)),
                (int)(this.slider_Sound_SE.Value) * 10);
            if (seCounter == 4)
            {
                this.seCounter = 1;
            }
        }

        private void slider_Sound_SE_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.label_Sound_SE != null)
            {
                this.label_Sound_SE.Content = this.slider_Sound_SE.Value.ToString("0");
            }
        }

        private void button_Sound_BGS_Try_Click(object sender, RoutedEventArgs e)
        {
            this.musicMana.PlayBGS(IOUtils.ParseURItoURL(@"LItem\bgs.mp3"), (int)(this.slider_Sound_BGM.Value) * 10);
        }

        private void button_Sound_BGS_TryEnd_Click(object sender, RoutedEventArgs e)
        {
            this.musicMana.StopBGS();
        }

        private void slider_Sound_BGS_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.label_Sound_BGS != null)
            {
                this.label_Sound_BGS.Content = this.slider_Sound_BGS.Value.ToString("0");
                if (this.musicMana.IsAnyBGS)
                {
                    this.musicMana.SetBGSVolume((int)(this.slider_Sound_BGS.Value) * 10);
                }
            }
        }

        private void slider_Sound_Vocal_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.label_Sound_Vocal != null)
            {
                this.label_Sound_Vocal.Content = this.slider_Sound_Vocal.Value.ToString("0");
            }
        }

        private void button_Sound_Vocal_Try_Click(object sender, RoutedEventArgs e)
        {
            this.musicMana.PlayVocal(IOUtils.ParseURItoURL(String.Format("LItem\\vocal{0}.wav", voiceCounter++)),
                (int)(this.slider_Sound_Vocal.Value) * 10);
            if (voiceCounter == 4)
            {
                voiceCounter = 1;
            }
        }

        private void Button_Screen_Typing_ChangeFont_Click(object sender, RoutedEventArgs e)
        {
            var cFont = new Font(this.textblock_Screen_Typing.FontFamily.FamilyNames.Last().Value,
                (float)this.textblock_Screen_Typing.FontSize);
            fontDialog = new System.Windows.Forms.FontDialog
            {
                AllowVerticalFonts = false,
                AllowScriptChange = false,
                MinSize = 8,
                ShowEffects = false,
                ShowColor = false,
                ShowHelp = false,
                ShowApply = false,
                FontMustExist = true,
                Font = cFont
            };

            fontDialog.ShowDialog();
            if (fontDialog.Font.Name != cFont.Name || Math.Abs(fontDialog.Font.Size - cFont.Size) >= 1)
            {
                this.textblock_Screen_Typing.FontFamily = new System.Windows.Media.FontFamily(fontDialog.Font.Name);
                this.textblock_Screen_Typing.FontSize = fontDialog.Font.Size;
            }
        }

        /// <summary>
        /// 画面：全屏幕
        /// </summary>
        private void radioButton_Screen_Window_1_Checked(object sender, RoutedEventArgs e)
        {
            if (this.groupBox_Screen_Resolution != null)
            {
                this.radioButton_Screen_Resolution_3.IsChecked = true;
                this.groupBox_Screen_Resolution.IsEnabled = false;
            }
        }

        /// <summary>
        /// 画面：窗口
        /// </summary>
        private void radioButton_Screen_Window_2_Checked(object sender, RoutedEventArgs e)
        {
            if (this.groupBox_Screen_Resolution != null)
            {
                this.groupBox_Screen_Resolution.IsEnabled = true;
            }
        }

        /// <summary>
        /// 按钮：版权
        /// </summary>
        private void button_about_resources_Click(object sender, RoutedEventArgs e)
        {
            RightsForm rf = new RightsForm();
            rf.ShowDialog();
        }

        /// <summary>
        /// 按钮：开始游戏
        /// </summary>
        private void button_System_Launch_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            ProcessStartInfo p;
            p = new ProcessStartInfo("Yuri.exe") { WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory };
            Process.Start(p);
        }

        /// <summary>
        /// 事件：关闭窗体
        /// </summary>
        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            this.SaveConfigContext();
        }
        #endregion

        private void Button_Screen_Typing_DefaultFont_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
