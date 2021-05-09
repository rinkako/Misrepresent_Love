using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Yuri.PlatformCore;
using Yuri.PlatformCore.Audio;
using Yuri.PlatformCore.Graphic;
using Yuri.PlatformCore.VM;
using Yuri.Utils;

namespace Yuri.PageView
{
    /// <summary>
    /// LHSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class LHSettingsPage : Page
    {
        private string currentTab = "Graphic";
        private Image currentTabImage;
        private readonly Dictionary<string, BitmapImage> cachingButtons = new Dictionary<string, BitmapImage>();
        private ILoadablePage parent;

        private YuriSprite GrahpicTabSprite;
        private YuriSprite VoiceTabSprite;
        private YuriSprite KeyboardTabSprite;
        private YuriSprite OthersTabSprite;

        private string startupDefaultFontName;
        private int startupDefaultFontSize;

        private readonly Color xichengBlue = Color.FromRgb(129, 179, 255);

        private static bool musicianInit = false;
        private static bool fontInit = false;

        private static object syncObj = new object();

        public LHSettingsPage(ILoadablePage parent)
        {
            InitializeComponent();
            this.parent = parent;
            this.currentTabImage = this.TabImage_Graphic;
            
            this.GrahpicTabSprite = new YuriSprite()
            {
                DisplayBinding = this.Grid_Tab_Graphic,
                AnimationElement = this.Grid_Tab_Graphic,
                Descriptor = new SpriteDescriptor()
            };
            this.VoiceTabSprite = new YuriSprite()
            {
                DisplayBinding = this.Grid_Tab_Voice,
                AnimationElement = this.Grid_Tab_Voice,
                Descriptor = new SpriteDescriptor()
            };
            this.KeyboardTabSprite = new YuriSprite()
            {
                DisplayBinding = this.Grid_Tab_Keyboard,
                AnimationElement = this.Grid_Tab_Keyboard,
                Descriptor = new SpriteDescriptor()
            };
            this.OthersTabSprite = new YuriSprite()
            {
                DisplayBinding = this.Grid_Tab_Others,
                AnimationElement = this.Grid_Tab_Others,
                Descriptor = new SpriteDescriptor()
            };
        }

        public void WhenInto()
        {
            ViewManager.mWnd.IsKeyAltWindowSizeEnabled = false;
            if (Director.IsFullScreen)
            {
                this.LabelRadio_Grahpic_Screen_Full.Foreground = new SolidColorBrush(this.xichengBlue);
                this.LabelRadio_Grahpic_Screen_Win.Foreground = new SolidColorBrush(Colors.Black);
            }
            else
            {
                this.LabelRadio_Grahpic_Screen_Win.Foreground = new SolidColorBrush(this.xichengBlue);
                this.LabelRadio_Grahpic_Screen_Full.Foreground = new SolidColorBrush(Colors.Black);
            }

            this.RefreshSteadyConfig();
        }

        public void WhenLeave()
        {
            ViewManager.mWnd.IsKeyAltWindowSizeEnabled = true;
        }

        public void RefreshSteadyConfig()
        {
            string rawRollback = PersistContextDAO.Fetch("system_config_rollback_enable")?.ToString();
            if (rawRollback == "0")
            {
                PersistContextDAO.Assign("system_config_rollback_enable", "true");
                rawRollback = "true";
            }
            Director.IsAllowRollback = rawRollback == "true";
            if (Director.IsAllowRollback)
            {
                this.LabelRadio_Others_Rollback_Enable.Foreground = new SolidColorBrush(this.xichengBlue);
                this.LabelRadio_Others_Rollback_Disable.Foreground = new SolidColorBrush(Colors.Black);
            }
            else
            {
                this.LabelRadio_Others_Rollback_Disable.Foreground = new SolidColorBrush(this.xichengBlue);
                this.LabelRadio_Others_Rollback_Enable.Foreground = new SolidColorBrush(Colors.Black);
            }

            string titleType = PersistContextDAO.Fetch("system_config_title_type")?.ToString();
            if (titleType == "0")
            {
                PersistContextDAO.Assign("system_config_title_type", "Timing");
                titleType = "Timing";
            }
            if (titleType == "Timing")
            {
                this.LabelRadio_Others_Title_Timing.Foreground = new SolidColorBrush(this.xichengBlue);
                this.LabelRadio_Others_Title_Dusk.Foreground = new SolidColorBrush(Colors.Black);
            }
            else
            {
                this.LabelRadio_Others_Title_Dusk.Foreground = new SolidColorBrush(this.xichengBlue);
                this.LabelRadio_Others_Title_Timing.Foreground = new SolidColorBrush(Colors.Black);
            }

            string performanceType = PersistContextDAO.Fetch("system_config_performance_set")?.ToString();
            if (performanceType == "0")
            {
                PersistContextDAO.Assign("system_config_performance_set", "Enable");
                performanceType = "Enable";
            }
            if (performanceType == "Enable")
            {
                this.LabelRadio_Others_Performance_Enable.Foreground = new SolidColorBrush(this.xichengBlue);
                this.LabelRadio_Others_Performance_Disable.Foreground = new SolidColorBrush(Colors.Black);
            }
            else
            {
                this.LabelRadio_Others_Performance_Disable.Foreground = new SolidColorBrush(this.xichengBlue);
                this.LabelRadio_Others_Performance_Enable.Foreground = new SolidColorBrush(Colors.Black);
            }

            string autoSpeedRaw = PersistContextDAO.Fetch("system_config_autospeed")?.ToString();
            if (autoSpeedRaw == "0")
            {
                PersistContextDAO.Assign("system_config_autospeed", "3");
                autoSpeedRaw = "3";
            }
            var autoSpeed = Convert.ToInt32(autoSpeedRaw);
            for (int i = 1; i <= 5; i++)
            {
                var cl = (Label)this.FindName("LabelRadio_Grahpic_Auto_" + i);
                if (i != autoSpeed)
                {
                    cl.Foreground = new SolidColorBrush(Colors.Black);
                }
                else
                {
                    cl.Foreground = new SolidColorBrush(this.xichengBlue);
                }
            }
            switch (autoSpeed)
            {
                case 1:
                    GlobalConfigContext.GAME_MSG_AUTOPLAY_DELAY = 7000;
                    break;
                case 2:
                    GlobalConfigContext.GAME_MSG_AUTOPLAY_DELAY = 3500;
                    break;
                case 3:
                    GlobalConfigContext.GAME_MSG_AUTOPLAY_DELAY = 2000;
                    break;
                case 4:
                    GlobalConfigContext.GAME_MSG_AUTOPLAY_DELAY = 1500;
                    break;
                case 5:
                    GlobalConfigContext.GAME_MSG_AUTOPLAY_DELAY = 600;
                    break;
            }

            string typeSpeedRaw = PersistContextDAO.Fetch("system_config_typespeed")?.ToString();
            if (typeSpeedRaw == "0")
            {
                PersistContextDAO.Assign("system_config_typespeed", "3");
                typeSpeedRaw = "3";
            }
            var typeSpeed = Convert.ToInt32(typeSpeedRaw);
            for (int i = 1; i <= 5; i++)
            {
                var cl = (Label)this.FindName("LabelRadio_Grahpic_Typing_" + i);
                if (i != typeSpeed)
                {
                    cl.Foreground = new SolidColorBrush(Colors.Black);
                }
                else
                {
                    cl.Foreground = new SolidColorBrush(this.xichengBlue);
                }
            }
            switch (typeSpeed)
            {
                case 1:
                    GlobalConfigContext.GAME_MSG_TYPING_DELAY = 120;
                    break;
                case 2:
                    GlobalConfigContext.GAME_MSG_TYPING_DELAY = 90;
                    break;
                case 3:
                    GlobalConfigContext.GAME_MSG_TYPING_DELAY = 30;
                    break;
                case 4:
                    GlobalConfigContext.GAME_MSG_TYPING_DELAY = 10;
                    break;
                case 5:
                    GlobalConfigContext.GAME_MSG_TYPING_DELAY = 0;
                    break;
            }

            string bgmRaw = PersistContextDAO.Fetch("system_config_audio_bgm")?.ToString();
            if (bgmRaw == "0")
            {
                PersistContextDAO.Assign("system_config_audio_bgm", "18");
                bgmRaw = "18";
            }
            var bgmRatio = Convert.ToInt32(bgmRaw);
            for (int i = 1; i <= bgmRatio; i++)
            {
                var curRec = (Rectangle)this.FindName("Rec_BGM_" + i);
                curRec.Fill = new SolidColorBrush(this.xichengBlue);
            }
            for (int i = bgmRatio + 1; i <= 20; i++)
            {
                var curRec = (Rectangle)this.FindName("Rec_BGM_" + i);
                curRec.Fill = new SolidColorBrush(Color.FromRgb(222, 222, 222));
            }
            if (!musicianInit)
            {
                if (bgmRatio == 1)
                {
                    Musician.GetInstance().BGMVolumeRatio = 0.0;
                }
                else if (bgmRatio == 20)
                {
                    Musician.GetInstance().BGMVolumeRatio = 1.0;
                }
                else
                {
                    Musician.GetInstance().BGMVolumeRatio = 1.0 / 20 * bgmRatio;
                }
                Musician.GetInstance().SetBGMVolume(800);
            }

            string seRaw = PersistContextDAO.Fetch("system_config_audio_se")?.ToString();
            if (seRaw == "0")
            {
                PersistContextDAO.Assign("system_config_audio_se", "18");
                seRaw = "18";
            }
            var seRatio = Convert.ToInt32(seRaw);
            for (int i = 1; i <= seRatio; i++)
            {
                var curRec = (Rectangle)this.FindName("Rec_SE_" + i);
                curRec.Fill = new SolidColorBrush(this.xichengBlue);
            }
            for (int i = seRatio + 1; i <= 20; i++)
            {
                var curRec = (Rectangle)this.FindName("Rec_SE_" + i);
                curRec.Fill = new SolidColorBrush(Color.FromRgb(222, 222, 222));
            }
            if (!musicianInit)
            {
                if (seRatio == 1)
                {
                    Musician.GetInstance().SEDefaultVolumeRatio = 0.0;
                }
                else if (seRatio == 20)
                {
                    Musician.GetInstance().SEDefaultVolumeRatio = 1.0;
                }
                else
                {
                    Musician.GetInstance().SEDefaultVolumeRatio = 1.0 / 20 * seRatio;
                }
            }

            string voiceRaw = PersistContextDAO.Fetch("system_config_audio_voice")?.ToString();
            if (voiceRaw == "0")
            {
                PersistContextDAO.Assign("system_config_audio_voice", "20");
                voiceRaw = "20";
            }
            var voiceRatio = Convert.ToInt32(voiceRaw);
            for (int i = 1; i <= voiceRatio; i++)
            {
                var curRec = (Rectangle)this.FindName("Rec_VOICE_" + i);
                curRec.Fill = new SolidColorBrush(this.xichengBlue);
            }
            for (int i = voiceRatio + 1; i <= 20; i++)
            {
                var curRec = (Rectangle)this.FindName("Rec_VOICE_" + i);
                curRec.Fill = new SolidColorBrush(Color.FromRgb(222, 222, 222));
            }
            if (!musicianInit)
            {
                if (voiceRatio == 1)
                {
                    Musician.GetInstance().VocalDefaultVolumeRatio = 0.0;
                }
                else if (voiceRatio == 20)
                {
                    Musician.GetInstance().VocalDefaultVolumeRatio = 1.0;
                }
                else
                {
                    Musician.GetInstance().VocalDefaultVolumeRatio = 1.0 / 20 * voiceRatio;
                }
            }

            musicianInit = true;

            string xwVoiceRaw = PersistContextDAO.Fetch("system_config_audio_voice_xw")?.ToString();
            if (xwVoiceRaw == "0")
            {
                PersistContextDAO.Assign("system_config_audio_voice_xw", "true");
                xwVoiceRaw = "true";
            }
            voice_xw = xwVoiceRaw == "true";
            var xwVoiceSourcePath = this.Chara_VOICE_XW.Source.ToString();
            if (voice_xw == true && xwVoiceSourcePath.EndsWith("On.png") || voice_xw == false && xwVoiceSourcePath.EndsWith("Off.png"))
            {
                // nothing
            }
            else if (voice_xw == true)
            {
                xwVoiceSourcePath = xwVoiceSourcePath.Replace("Off.png", "On.png");
            }
            else if (voice_xw == false)
            {
                xwVoiceSourcePath = xwVoiceSourcePath.Replace("On.png", "Off.png");
            }

            BitmapImage voiceXWRotating;
            if (this.cachingButtons.ContainsKey(xwVoiceSourcePath))
            {
                voiceXWRotating = this.cachingButtons[xwVoiceSourcePath];
            }
            else
            {
                voiceXWRotating = new BitmapImage();
                voiceXWRotating.BeginInit();
                voiceXWRotating.UriSource = new Uri(xwVoiceSourcePath);
                voiceXWRotating.EndInit();
                this.cachingButtons[xwVoiceSourcePath] = voiceXWRotating;
            }
            this.Chara_VOICE_XW.Source = voiceXWRotating;

            string qlVoiceRaw = PersistContextDAO.Fetch("system_config_audio_voice_ql")?.ToString();
            if (qlVoiceRaw == "0")
            {
                PersistContextDAO.Assign("system_config_audio_voice_ql", "true");
                qlVoiceRaw = "true";
            }
            voice_ql = qlVoiceRaw == "true";
            var qlVoiceSourcePath = this.Chara_VOICE_QL.Source.ToString();
            if (voice_ql == true && qlVoiceSourcePath.EndsWith("On.png") || voice_ql == false && qlVoiceSourcePath.EndsWith("Off.png"))
            {
                // nothing
            }
            else if (voice_ql == true)
            {
                qlVoiceSourcePath = qlVoiceSourcePath.Replace("Off.png", "On.png");
            }
            else if (voice_ql == false)
            {
                qlVoiceSourcePath = qlVoiceSourcePath.Replace("On.png", "Off.png");
            }

            BitmapImage voiceQLRotating;
            if (this.cachingButtons.ContainsKey(qlVoiceSourcePath))
            {
                voiceQLRotating = this.cachingButtons[qlVoiceSourcePath];
            }
            else
            {
                voiceQLRotating = new BitmapImage();
                voiceQLRotating.BeginInit();
                voiceQLRotating.UriSource = new Uri(qlVoiceSourcePath);
                voiceQLRotating.EndInit();
                this.cachingButtons[qlVoiceSourcePath] = voiceQLRotating;
            }
            this.Chara_VOICE_QL.Source = voiceQLRotating;

            string userFontName = PersistContextDAO.Fetch("system_config_userfont_name")?.ToString();
            if (userFontName == "0")
            {
                PersistContextDAO.Assign("system_config_userfont_name", "default");
                userFontName = "default";
            }
            string userFontSize = PersistContextDAO.Fetch("system_config_userfont_size")?.ToString();
            if (userFontSize == "0")
            {
                PersistContextDAO.Assign("system_config_userfont_size", GlobalConfigContext.GAME_FONT_FONTSIZE.ToString());
                userFontSize = GlobalConfigContext.GAME_FONT_FONTSIZE.ToString();
            }
            lock (LHSettingsPage.syncObj)
            {
                if (fontInit == false)
                {
                    try
                    {
                        this.startupDefaultFontName = GlobalConfigContext.GAME_FONT_NAME;
                        this.startupDefaultFontSize = GlobalConfigContext.GAME_FONT_FONTSIZE;
                        this.Label_FontName.Content = userFontName;
                        this.Label_FontName.FontSize = Convert.ToDouble(userFontSize);
                        this.Label_FontName.FontFamily = new FontFamily(userFontName);

                        Director.GetInstance().GetMainRender().MsgLayerOpt(0, "fontname", userFontName);
                        Director.GetInstance().GetMainRender().MsgLayerOpt(0, "fontsize", userFontSize);
                        GlobalConfigContext.GAME_FONT_NAME = userFontName;
                        GlobalConfigContext.GAME_FONT_FONTSIZE = int.Parse(userFontSize);
                    }
                    catch (Exception fe)
                    {
                        LogUtils.LogLine("cannot load user custom font: " + fe.ToString(), nameof(LHSettingsPage), LogLevel.Error);
                        Director.GetInstance().GetMainRender().MsgLayerOpt(0, "fontname", this.startupDefaultFontName);
                        Director.GetInstance().GetMainRender().MsgLayerOpt(0, "fontsize", this.startupDefaultFontSize.ToString());
                        GlobalConfigContext.GAME_FONT_NAME = this.startupDefaultFontName;
                        GlobalConfigContext.GAME_FONT_FONTSIZE = this.startupDefaultFontSize;
                    }
                    fontInit = true;
                }
                else
                {
                    if (userFontName == "default")
                    {
                        this.Label_FontName.Content = "（默认）Source Han Serif";
                        this.Label_FontName.FontSize = 32;
                        this.Label_FontName.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "Resources/#Source Han Serif CN SemiBold");
                    }
                    else
                    {
                        this.Label_FontName.Content = userFontName;
                        this.Label_FontName.FontSize = Convert.ToDouble(userFontSize);
                        this.Label_FontName.FontFamily = new FontFamily(userFontName);
                    }
                }
            }

        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                return;
            }
            var oldTab = this.currentTab;
            this.currentTab = ((Image)sender).Name.Replace("TabImage_", "");
            if (oldTab == this.currentTab)
            {
                return;
            }
            var oldTabImage = this.currentTabImage;

            var sourcePath = oldTabImage.Source.ToString().Replace("_Select.png", ".png");
            BitmapImage rotating;
            if (this.cachingButtons.ContainsKey(sourcePath))
            {
                rotating = this.cachingButtons[sourcePath];
            }
            else
            {
                rotating = new BitmapImage();
                rotating.BeginInit();
                rotating.UriSource = new Uri(sourcePath);
                rotating.EndInit();
                this.cachingButtons[sourcePath] = rotating;
            }
            oldTabImage.Source = rotating;

            var sourcePath2 = ((Image)sender).Source.ToString().Replace(".png", "_Select.png");
            BitmapImage rotating2;
            if (this.cachingButtons.ContainsKey(sourcePath2))
            {
                rotating2 = this.cachingButtons[sourcePath2];
            }
            else
            {
                rotating2 = new BitmapImage();
                rotating2.BeginInit();
                rotating2.UriSource = new Uri(sourcePath2);
                rotating2.EndInit();
                this.cachingButtons[sourcePath2] = rotating2;
            }
            ((Image)sender).Source = rotating2;

            this.currentTabImage = (Image)sender;
            
            this.transitTab(oldTab, this.currentTab);
        }

        private void transitTab(string fromTab, string toTab)
        {
            YuriSprite fromSprite = null;
            YuriSprite toSprite = null;
            switch (fromTab)
            {
                case "Graphic":
                    fromSprite = this.GrahpicTabSprite;
                    break;
                case "Voice":
                    fromSprite = this.VoiceTabSprite;
                    break;
                case "Keyboard":
                    fromSprite = this.KeyboardTabSprite;
                    break;
                case "Others":
                    fromSprite = this.OthersTabSprite;
                    break;
            }
            switch (toTab)
            {
                case "Graphic":
                    toSprite = this.GrahpicTabSprite;
                    break;
                case "Voice":
                    toSprite = this.VoiceTabSprite;
                    break;
                case "Keyboard":
                    toSprite = this.KeyboardTabSprite;
                    break;
                case "Others":
                    toSprite = this.OthersTabSprite;
                    break;
            }
            this.Canvas_SettingsRoot.IsHitTestVisible = false;
            toSprite.DisplayBinding.Visibility = Visibility.Visible;

            toSprite.Descriptor.ToX = 1920 / 2 + 100;
            toSprite.Descriptor.ToOpacity = 0;
            SpriteAnimation.XMoveToAnimation(toSprite, new Duration(TimeSpan.FromMilliseconds(0)), toSprite.Descriptor.ToX, 0, null);
            SpriteAnimation.OpacityToAnimation(toSprite, new Duration(TimeSpan.FromMilliseconds(0)), 0, 0, null);

            Storyboard sb = new Storyboard();


            fromSprite.Descriptor.ToX = 1920 / 2 - 100;
            fromSprite.Descriptor.ToOpacity = 0;
            toSprite.Descriptor.ToX = 1920 / 2;
            toSprite.Descriptor.ToOpacity = 1;
            SpriteAnimation.XMoveToAnimation(fromSprite, new Duration(TimeSpan.FromMilliseconds(200)), fromSprite.Descriptor.ToX, -0.6, null, providedStory: sb);
            SpriteAnimation.XMoveToAnimation(toSprite, new Duration(TimeSpan.FromMilliseconds(200)), toSprite.Descriptor.ToX, -0.6, null, providedStory: sb);
            SpriteAnimation.OpacityToAnimation(fromSprite, new Duration(TimeSpan.FromMilliseconds(200)), 0, 0, null, providedStory: sb);
            SpriteAnimation.OpacityToAnimation(toSprite, new Duration(TimeSpan.FromMilliseconds(200)), 1, 0, null, providedStory: sb);
            sb.Completed += (_, __) =>
            {
                this.Canvas_SettingsRoot.IsHitTestVisible = true;
                fromSprite.DisplayBinding.Visibility = Visibility.Hidden;
            };
            sb.Begin();
        }

        private void Warp_Btn_MouseEnter(object sender, MouseEventArgs e)
        {
            var sourcePath = ((Image)sender).Source.ToString().Replace(".png", "_Select.png");
            BitmapImage rotating;
            if (this.cachingButtons.ContainsKey(sourcePath))
            {
                rotating = this.cachingButtons[sourcePath];
            }
            else
            {
                rotating = new BitmapImage();
                rotating.BeginInit();
                rotating.UriSource = new Uri(sourcePath);
                rotating.EndInit();
                this.cachingButtons[sourcePath] = rotating;
            }
            ((Image)sender).Source = rotating;
        }

        private void Warp_Btn_MouseLeave(object sender, MouseEventArgs e)
        {
            var sourcePath = ((Image)sender).Source.ToString().Replace("_Select.png", ".png");
            BitmapImage rotating;
            if (this.cachingButtons.ContainsKey(sourcePath))
            {
                rotating = this.cachingButtons[sourcePath];
            }
            else
            {
                rotating = new BitmapImage();
                rotating.BeginInit();
                rotating.UriSource = new Uri(sourcePath);
                rotating.EndInit();
                this.cachingButtons[sourcePath] = rotating;
            }
            ((Image)sender).Source = rotating;
        }

        private void Rec_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                return;
            }
            var selectedName = ((Rectangle)sender).Name;
            var selectedCount = Convert.ToInt32(selectedName.Split('_').Last());
            var selectedTerm = selectedName.Substring(0, selectedName.LastIndexOf('_') + 1);
            for (int i = 1; i <= selectedCount; i++)
            {
                var curRec = (Rectangle)this.FindName(selectedTerm + i);
                curRec.Fill = new SolidColorBrush(this.xichengBlue);
            }
            for (int i = selectedCount + 1; i <= 20; i++)
            {
                var curRec = (Rectangle)this.FindName(selectedTerm + i);
                curRec.Fill = new SolidColorBrush(Color.FromRgb(222, 222, 222));
            }
            if (selectedTerm.StartsWith("Rec_BGM_"))
            {
                if (selectedCount == 1)
                {
                    Musician.GetInstance().BGMVolumeRatio = 0.0;
                }
                else if (selectedCount == 20)
                {
                    Musician.GetInstance().BGMVolumeRatio = 1.0;
                }
                else
                {
                    Musician.GetInstance().BGMVolumeRatio = 1.0 / 20 * selectedCount;
                }
                PersistContextDAO.Assign("system_config_audio_bgm", selectedCount.ToString());
                Musician.GetInstance().SetBGMVolume(800);
            }
            else if (selectedTerm.StartsWith("Rec_SE_"))
            {
                if (selectedCount == 1)
                {
                    Musician.GetInstance().SEDefaultVolumeRatio = 0.0;
                }
                else if (selectedCount == 20)
                {
                    Musician.GetInstance().SEDefaultVolumeRatio = 1.0;
                }
                else
                {
                    Musician.GetInstance().SEDefaultVolumeRatio = 1.0 / 20 * selectedCount;
                }
                PersistContextDAO.Assign("system_config_audio_se", selectedCount.ToString());
            }
            else if (selectedTerm.StartsWith("Rec_VOICE_"))
            {
                if (selectedCount == 1)
                {
                    Musician.GetInstance().VocalDefaultVolumeRatio = 0.0;
                }
                else if (selectedCount == 20)
                {
                    Musician.GetInstance().VocalDefaultVolumeRatio = 1.0;
                }
                else
                {
                    Musician.GetInstance().VocalDefaultVolumeRatio = 1.0 / 20 * selectedCount;
                }
                PersistContextDAO.Assign("system_config_audio_voice", selectedCount.ToString());
            }
        }

        private static bool voice_xw = true;
        private static bool voice_ql = true;

        private void Chara_VOICE_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                return;
            }
            bool? judger = null;
            var name = ((Image)sender).Name;
            switch (name)
            {
                case "Chara_VOICE_XW":
                    voice_xw = !voice_xw;
                    judger = voice_xw;
                    PersistContextDAO.Assign("system_config_audio_voice_xw", judger.ToString().ToLower());
                    break;
                case "Chara_VOICE_QL":
                    voice_ql = !voice_ql;
                    judger = voice_ql;
                    PersistContextDAO.Assign("system_config_audio_voice_ql", judger.ToString().ToLower());
                    break;
            }
            
            var sourcePath = ((Image)sender).Source.ToString();
            if (judger == true && sourcePath.EndsWith("On.png") || judger == false && sourcePath.EndsWith("Off.png"))
            {
                return;
            }
            else if (judger == true)
            {
                sourcePath = sourcePath.Replace("Off.png", "On.png");
            }
            else if (judger == false)
            {
                sourcePath = sourcePath.Replace("On.png", "Off.png");
            }

            BitmapImage rotating;
            if (this.cachingButtons.ContainsKey(sourcePath))
            {
                rotating = this.cachingButtons[sourcePath];
            }
            else
            {
                rotating = new BitmapImage();
                rotating.BeginInit();
                rotating.UriSource = new Uri(sourcePath);
                rotating.EndInit();
                this.cachingButtons[sourcePath] = rotating;
            }
            ((Image)sender).Source = rotating;
        }

        private void LabelRadio_Grahpic_Screen_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                return;
            }
            var name = ((Label)sender).Name;
            if (name.Contains("Full"))
            {
                this.LabelRadio_Grahpic_Screen_Full.Foreground = new SolidColorBrush(this.xichengBlue);
                this.LabelRadio_Grahpic_Screen_Win.Foreground = new SolidColorBrush(Colors.Black);
                if (!Director.IsFullScreen)
                {
                    ViewManager.mWnd.FullScreenTransform();
                    Director.IsFullScreen = true;
                }
            }
            else if (name.Contains("Win"))
            {
                this.LabelRadio_Grahpic_Screen_Win.Foreground = new SolidColorBrush(this.xichengBlue);
                this.LabelRadio_Grahpic_Screen_Full.Foreground = new SolidColorBrush(Colors.Black);
                if (Director.IsFullScreen)
                {
                    ViewManager.mWnd.WindowScreenTransform(1280, 720 + 30);
                    Director.IsFullScreen = false;
                }
            }
        }

        private void LabelRadio_Others_Rollback_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                return;
            }
            var name = ((Label)sender).Name;
            if (name.Contains("Enable"))
            {
                this.LabelRadio_Others_Rollback_Enable.Foreground = new SolidColorBrush(this.xichengBlue);
                this.LabelRadio_Others_Rollback_Disable.Foreground = new SolidColorBrush(Colors.Black);
                Director.IsAllowRollback = true;
                PersistContextDAO.Assign("system_config_rollback_enable", "true");
            }
            else if (name.Contains("Disable"))
            {
                this.LabelRadio_Others_Rollback_Disable.Foreground = new SolidColorBrush(this.xichengBlue);
                this.LabelRadio_Others_Rollback_Enable.Foreground = new SolidColorBrush(Colors.Black);
                Director.IsAllowRollback = false;
                PersistContextDAO.Assign("system_config_rollback_enable", "false");
            }
        }

        private void Image_Btn_SetFont_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                return;
            }
            var fontDialog = new System.Windows.Forms.FontDialog
            {
                AllowVerticalFonts = false,
                AllowScriptChange = false,
                MinSize = 8,
                ShowEffects = false,
                ShowColor = false,
                ShowHelp = false,
                ShowApply = false,
                FontMustExist = true
            };

            var dr = fontDialog.ShowDialog();
            if (dr  == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            
            var newFontName = fontDialog.Font.Name;
            var newFontSize = fontDialog.Font.Size;

            try
            {
                this.Label_FontName.Content = newFontName;
                this.Label_FontName.FontSize = newFontSize;
                this.Label_FontName.FontFamily = new FontFamily(newFontName);
            }
            catch (Exception fe)
            {
                LogUtils.LogLine("cannot load user custom font: " + fe.ToString(), nameof(LHSettingsPage), LogLevel.Error);
                GlobalConfigContext.GAME_FONT_NAME = this.startupDefaultFontName;
                GlobalConfigContext.GAME_FONT_FONTSIZE = this.startupDefaultFontSize;
                Director.GetInstance().GetMainRender().MsgLayerOpt(0, "fontname", this.startupDefaultFontName);
                Director.GetInstance().GetMainRender().MsgLayerOpt(0, "fontsize", this.startupDefaultFontSize.ToString());
                return;
            }

            Director.GetInstance().GetMainRender().MsgLayerOpt(0, "fontname", newFontName);
            Director.GetInstance().GetMainRender().MsgLayerOpt(0, "fontsize", newFontSize.ToString());
            
            PersistContextDAO.Assign("system_config_userfont_name", newFontName);
            PersistContextDAO.Assign("system_config_userfont_size", newFontSize);

            GlobalConfigContext.GAME_FONT_NAME = newFontName;
            GlobalConfigContext.GAME_FONT_FONTSIZE = Convert.ToInt32(newFontSize);
        }

        private void Image_Btn_DefaultFont_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                return;
            }
            GlobalConfigContext.GAME_FONT_NAME = "default";
            GlobalConfigContext.GAME_FONT_FONTSIZE = 32;
            Director.GetInstance().GetMainRender().MsgLayerOpt(0, "fontname", "default");
            Director.GetInstance().GetMainRender().MsgLayerOpt(0, "fontsize", "32");
            PersistContextDAO.Assign("system_config_userfont_name", "default");
            PersistContextDAO.Assign("system_config_userfont_size", "32");
            this.Label_FontName.Content = "（默认）Source Han Serif";
            this.Label_FontName.FontSize = 32;
            this.Label_FontName.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "Resources/#Source Han Serif CN SemiBold");
        }

        private void LabelRadio_Others_Title_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                return;
            }
            switch (((Label) sender).Name)
            {
                case "LabelRadio_Others_Title_Timing":
                    PersistContextDAO.Assign("system_config_title_type", "Timing");
                    this.LabelRadio_Others_Title_Timing.Foreground = new SolidColorBrush(this.xichengBlue);
                    this.LabelRadio_Others_Title_Dusk.Foreground = new SolidColorBrush(Colors.Black);
                    break;
                case "LabelRadio_Others_Title_Dusk":
                    PersistContextDAO.Assign("system_config_title_type", "Dusk");
                    this.LabelRadio_Others_Title_Dusk.Foreground = new SolidColorBrush(this.xichengBlue);
                    this.LabelRadio_Others_Title_Timing.Foreground = new SolidColorBrush(Colors.Black);
                    break;
            }
        }
        
        private void LabelRadio_Grahpic_Auto_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                return;
            }
            var name = ((Label)sender).Name;
            var id = Convert.ToInt32(name.Replace("LabelRadio_Grahpic_Auto_", ""));
            for (int i = 1; i <= 5; i++)
            {
                var cl = (Label)this.FindName("LabelRadio_Grahpic_Auto_" + i);
                if (i != id)
                {   
                    cl.Foreground = new SolidColorBrush(Colors.Black);
                }
                else
                {
                    cl.Foreground = new SolidColorBrush(this.xichengBlue);
                }
            }
            PersistContextDAO.Assign("system_config_autospeed", id.ToString());
            switch (id)
            {
                case 1:
                    GlobalConfigContext.GAME_MSG_AUTOPLAY_DELAY = 7000;
                    break;
                case 2:
                    GlobalConfigContext.GAME_MSG_AUTOPLAY_DELAY = 3500;
                    break;
                case 3:
                    GlobalConfigContext.GAME_MSG_AUTOPLAY_DELAY = 2000;
                    break;
                case 4:
                    GlobalConfigContext.GAME_MSG_AUTOPLAY_DELAY = 1500;
                    break;
                case 5:
                    GlobalConfigContext.GAME_MSG_AUTOPLAY_DELAY = 600;
                    break;
            }
        }

        private void LabelRadio_Grahpic_Typing_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                return;
            }
            var name = ((Label)sender).Name;
            var id = Convert.ToInt32(name.Replace("LabelRadio_Grahpic_Typing_", ""));
            for (int i = 1; i <= 5; i++)
            {
                var cl = (Label)this.FindName("LabelRadio_Grahpic_Typing_" + i);
                if (i != id)
                {
                    cl.Foreground = new SolidColorBrush(Colors.Black);
                }
                else
                {
                    cl.Foreground = new SolidColorBrush(this.xichengBlue);
                }
            }
            PersistContextDAO.Assign("system_config_typespeed", id.ToString());
            switch (id)
            {
                case 1:
                    GlobalConfigContext.GAME_MSG_TYPING_DELAY = 120;
                    break;
                case 2:
                    GlobalConfigContext.GAME_MSG_TYPING_DELAY = 90;
                    break;
                case 3:
                    GlobalConfigContext.GAME_MSG_TYPING_DELAY = 30;
                    break;
                case 4:
                    GlobalConfigContext.GAME_MSG_TYPING_DELAY = 10;
                    break;
                case 5:
                    GlobalConfigContext.GAME_MSG_TYPING_DELAY = 0;
                    break;
            }
        }

        private void LabelRadio_Others_Performance_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                return;
            }
            switch (((Label)sender).Name)
            {
                case "LabelRadio_Others_Performance_Enable":
                    PersistContextDAO.Assign("system_config_performance_set", "Enable");
                    this.LabelRadio_Others_Performance_Enable.Foreground = new SolidColorBrush(this.xichengBlue);
                    this.LabelRadio_Others_Performance_Disable.Foreground = new SolidColorBrush(Colors.Black);
                    break;
                case "LabelRadio_Others_Performance_Disable":
                    PersistContextDAO.Assign("system_config_performance_set", "Disable");
                    this.LabelRadio_Others_Performance_Disable.Foreground = new SolidColorBrush(this.xichengBlue);
                    this.LabelRadio_Others_Performance_Enable.Foreground = new SolidColorBrush(Colors.Black);
                    break;
            }
        }
    }
}
