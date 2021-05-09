using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Yuri.PlatformCore;
using Yuri.PlatformCore.Graphic;
using Yuri.PlatformCore.VM;
using Yuri.Utils;

namespace Yuri.PageView
{
    /// <summary>
    /// LHStartPage.xaml 的交互逻辑
    /// </summary>
    public partial class LHStartPage : Page, YuriPage, ILoadablePage
    {
        private delegate void StartPageExitAniCallback();
        private readonly Dictionary<string, BitmapImage> cachingButtons = new Dictionary<string, BitmapImage>();

        private string currentSubStage = "root";

        private Director core = Director.GetInstance();
        private string callbackTarget = String.Empty;
        private bool isTrueEndArrival = false;
        private bool isLoading = false;

        private YuriSprite BackGroundSprite;
        private YuriSprite ForeGroundSprite;

        private YuriSprite UiSprite;
        private YuriSprite MaskSprite;

        private YuriSprite RootButtonSprite;

        private YuriSprite CloudHolderSprite;
        private YuriSprite CloudSpriteA;
        private YuriSprite CloudSpriteB;

        private YuriSprite WarpedPageSprite;
        private YuriSprite SettingsPageSprite;

        private YuriSprite GalleryWarpSprite;
        private YuriSprite GalleryContentWarpSprite;

        private YuriSprite GalleryStaffSprite;
        private YuriSprite GalleryArtSprite;


        private Storyboard cloudSb;


        public LHStartPage()
        {
            InitializeComponent();

            this.Frame_SaveLoad.Content = new LHSaveLoadPage(this);
            this.Frame_Settings.Content = new LHSettingsPage(this);
            this.Frame_BonusStory.Content = new LongStoryPage(this);

            this.BackGroundSprite = new YuriSprite()
            {
                DisplayBinding = this.Art_Image_Background,
                AnimationElement = this.Art_Image_Background,
                Descriptor = new SpriteDescriptor()
            };

            TransformGroup aniGroup = new TransformGroup();
            TranslateTransform XYTransformer = new TranslateTransform();
            ScaleTransform ScaleTransformer = new ScaleTransform
            {
                CenterX = 1920 / 2,
                CenterY = 1080 / 2
            };
            RotateTransform RotateTransformer = new RotateTransform
            {
                CenterX = 1920 / 2,
                CenterY = 1080 / 2
            };
            aniGroup.Children.Add(XYTransformer);
            aniGroup.Children.Add(ScaleTransformer);
            aniGroup.Children.Add(RotateTransformer);
            this.Art_Image_Background.RenderTransform = aniGroup;
            this.BackGroundSprite.TranslateTransformer = XYTransformer;
            this.BackGroundSprite.ScaleTransformer = ScaleTransformer;
            this.BackGroundSprite.RotateTransformer = RotateTransformer;


            this.ForeGroundSprite = new YuriSprite()
            {
                DisplayBinding = this.Art_Image_Fore,
                AnimationElement = this.Art_Image_Fore,
                Descriptor = new SpriteDescriptor()
            };

            TransformGroup aniGroupFore = new TransformGroup();
            TranslateTransform XYTransformerFore = new TranslateTransform();
            ScaleTransform ScaleTransformerFore = new ScaleTransform
            {
                CenterX = 1920 / 2,
                CenterY = 1080 / 2
            };
            RotateTransform RotateTransformerFore = new RotateTransform
            {
                CenterX = 1920 / 2,
                CenterY = 1080 / 2
            };
            aniGroupFore.Children.Add(XYTransformerFore);
            aniGroupFore.Children.Add(ScaleTransformerFore);
            aniGroupFore.Children.Add(RotateTransformerFore);
            this.Art_Image_Fore.RenderTransform = aniGroupFore;
            this.ForeGroundSprite.TranslateTransformer = XYTransformerFore;
            this.ForeGroundSprite.ScaleTransformer = ScaleTransformerFore;
            this.ForeGroundSprite.RotateTransformer = RotateTransformerFore;

            this.UiSprite = new YuriSprite()
            {
                DisplayBinding = this.Grid_WarpHolder,
                AnimationElement = this.Grid_WarpHolder,
                Descriptor = new SpriteDescriptor()
            };
            this.RootButtonSprite = new YuriSprite()
            {
                DisplayBinding = this.Title_Root_Button_Warp,
                AnimationElement = this.Title_Root_Button_Warp,
                Descriptor = new SpriteDescriptor()
            };

            this.CloudHolderSprite = new YuriSprite()
            {
                DisplayBinding = this.Art_Canvas_Cloud,
                AnimationElement = this.Art_Canvas_Cloud,
                Descriptor = new SpriteDescriptor()
            };
            this.CloudSpriteA = new YuriSprite()
            {
                DisplayBinding = this.Art_Image_Background_Cloud_1,
                AnimationElement = this.Art_Image_Background_Cloud_1,
                Descriptor = new SpriteDescriptor()
            };
            this.CloudSpriteB = new YuriSprite()
            {
                DisplayBinding = this.Art_Image_Background_Cloud_2,
                AnimationElement = this.Art_Image_Background_Cloud_2,
                Descriptor = new SpriteDescriptor()
            };

            this.MaskSprite = new YuriSprite()
            {
                DisplayBinding = this.Mask_Black,
                AnimationElement = this.Mask_Black,
                Descriptor = new SpriteDescriptor()
            };
            this.WarpedPageSprite = new YuriSprite()
            {
                DisplayBinding = this.Grid_LoadUIWarp,
                AnimationElement = this.Grid_LoadUIWarp,
                Descriptor = new SpriteDescriptor()
            };

            this.GalleryWarpSprite = new YuriSprite()
            {
                DisplayBinding = this.Canvas_Gallery,
                AnimationElement = this.Canvas_Gallery,
                Descriptor = new SpriteDescriptor()
            };
            this.GalleryContentWarpSprite = new YuriSprite()
            {
                DisplayBinding = this.Canvas_Gallery_ContentWarp,
                AnimationElement = this.Canvas_Gallery_ContentWarp,
                Descriptor = new SpriteDescriptor()
            };
            this.GalleryStaffSprite = new YuriSprite()
            {
                DisplayBinding = this.Image_Gallery_Staff,
                AnimationElement = this.Image_Gallery_Staff,
                Descriptor = new SpriteDescriptor()
            };
            this.GalleryArtSprite = new YuriSprite()
            {
                DisplayBinding = this.Image_Art_Displayer,
                AnimationElement = this.Image_Art_Displayer,
                Descriptor = new SpriteDescriptor()
            };
        }

        public void PrepareClose()
        {
            this.Frame_SaveLoad.IsHitTestVisible = false;
            this.WarpedPageSprite.DisplayBinding.Visibility = Visibility.Hidden;
            if (this.isLoading == false && callbackTarget != String.Empty)
            {
                var callbackNtr = new Interrupt()
                {
                    Detail = "LHRCCallbackNTR",
                    InterruptSA = null,
                    Type = InterruptType.ButtonJump,
                    ReturnTarget = callbackTarget,
                    ExitWait = true
                };
                // 提交返回主舞台的中断到主调用堆栈
                Director.RunMana.CallStack.Submit(callbackNtr);
                // 重置回调
                callbackTarget = String.Empty;
                Director.RunMana.Symbols.GlobalCtxDao.GlobalSymbolTable.Assign("tracing_callback", String.Empty);
            }
            if (this.cloudSb != null)
            {
                this.cloudSb.SkipToFill();
                this.cloudSb = null;
            }
            ViewPageManager.CollapseUIPage();
        }

        public void PrepareOpen()
        {
            ((LHSettingsPage)this.Frame_Settings.Content).RefreshSteadyConfig();
            this.currentSubStage = "root";
            this.titlePlayingBGM = "2";
            this.isLoading = false;
            this.isTrueEndArrival = (double) PersistContextDAO.Fetch("Game_EndArrival_TE") > 0;
            this.Warp_Btn_Gallery.Visibility = this.isTrueEndArrival ? Visibility.Visible : Visibility.Hidden;


            if (isTrueEndArrival)
            {
                this.Art_Image_Background_Window.Source = new BitmapImage(new Uri("pack://application:,,,/Yuri;component/Resources/Title_Ground_T2_bg2.png"));
                this.Art_Image_Fore.Source = new BitmapImage(new Uri("pack://application:,,,/Yuri;component/Resources/Title_Ground_T2_fore.png"));
                this.Warp_Image_Title.Source = new BitmapImage(new Uri("pack://application:,,,/Yuri;component/Resources/Title_Logo_2.png"));
                
            }

            string titleType = PersistContextDAO.Fetch("system_config_title_type")?.ToString();
            if (titleType == "0")
            {
                PersistContextDAO.Assign("system_config_title_type", "Timing");
                titleType = "Timing";
            }

            if (false == isTrueEndArrival)
            {
                String cloudSourcePath;
                if (((DateTime.Now.Hour >= 5 && DateTime.Now.Hour < 17) || titleType != "Timing"))
                {
                    cloudSourcePath = ((Image)this.Art_Image_Background_Cloud_1).Source.ToString();
                    if (cloudSourcePath.EndsWith("_sunset.png"))
                    {
                        cloudSourcePath = cloudSourcePath.Replace("_sunset.png", ".png");
                    }
                }
                else
                {
                    cloudSourcePath = ((Image)this.Art_Image_Background_Cloud_1).Source.ToString();
                    if (!cloudSourcePath.EndsWith("_sunset.png"))
                    {
                        cloudSourcePath = cloudSourcePath.Replace(".png", "_sunset.png");
                    }
                }
                BitmapImage rotating;
                if (this.cachingButtons.ContainsKey(cloudSourcePath))
                {
                    rotating = this.cachingButtons[cloudSourcePath];
                }
                else
                {
                    rotating = new BitmapImage();
                    rotating.BeginInit();
                    rotating.UriSource = new Uri(cloudSourcePath);
                    rotating.EndInit();
                    this.cachingButtons[cloudSourcePath] = rotating;
                }
                ((Image)Art_Image_Background_Cloud_1).Source = rotating;
                ((Image)Art_Image_Background_Cloud_2).Source = rotating;
                ((Image)Art_Image_Background_Cloud_Static).Source = null;
            }
            else
            {
                ((Image)Art_Image_Background_Cloud_1).Source = new BitmapImage(new Uri("pack://application:,,,/Yuri;component/Resources/Title_Ground_TE_float.png"));
                ((Image)Art_Image_Background_Cloud_2).Source = new BitmapImage(new Uri("pack://application:,,,/Yuri;component/Resources/Title_Ground_TE_float.png"));
                ((Image)Art_Image_Background_Cloud_Static).Source = new BitmapImage(new Uri("pack://application:,,,/Yuri;component/Resources/Title_Ground_TE_bg.png"));
            }

            callbackTarget = SymbolTable.GetInstance().GlobalCtxDao.GlobalSymbolTable.Fetch("tracing_callback").ToString();
            this.BackGroundSprite.Descriptor.ToScaleX = 1.2;
            this.BackGroundSprite.Descriptor.ToScaleY = 1.2;
            this.BackGroundSprite.Descriptor.ToOpacity = 0;
            SpriteAnimation.ScaleToAnimation(this.BackGroundSprite, new Duration(TimeSpan.FromMilliseconds(0)), 1.2, 1.2, 0, 0);
            SpriteAnimation.OpacityToAnimation(this.BackGroundSprite, new Duration(TimeSpan.FromMilliseconds(0)), 0, 0);

            this.CloudSpriteA.Descriptor.ToX = 1920 / 2;
            this.CloudSpriteB.Descriptor.ToX = 1920 * 1.5;
            SpriteAnimation.XMoveToAnimation(this.CloudSpriteA, new Duration(TimeSpan.FromSeconds(0)), 1920 / 2, 0);
            SpriteAnimation.XMoveToAnimation(this.CloudSpriteB, new Duration(TimeSpan.FromSeconds(0)), 1920 * 1.5, 0);

            this.RootButtonSprite.Descriptor.ToOpacity = 1;
            SpriteAnimation.OpacityToAnimation(this.RootButtonSprite, new Duration(TimeSpan.FromMilliseconds(0)), 0, 0);
            this.UiSprite.Descriptor.ToOpacity = 0;
            SpriteAnimation.OpacityToAnimation(this.UiSprite, new Duration(TimeSpan.FromMilliseconds(0)), 0, 0);
            this.CloudHolderSprite.Descriptor.ToOpacity = 0;
            SpriteAnimation.OpacityToAnimation(this.CloudHolderSprite, new Duration(TimeSpan.FromMilliseconds(0)), 0, 0);
            this.MaskSprite.Descriptor.ToOpacity = 0;
            SpriteAnimation.OpacityToAnimation(this.MaskSprite, new Duration(TimeSpan.FromMilliseconds(0)), 0, 0);
            this.MaskSprite.DisplayBinding.Visibility = Visibility.Hidden;

            this.GalleryWarpSprite.Descriptor.ToOpacity = 0;
            SpriteAnimation.OpacityToAnimation(this.GalleryWarpSprite, new Duration(TimeSpan.FromMilliseconds(0)), 0, 0);
            this.Canvas_Gallery.Visibility = Visibility.Hidden;

            this.BackGroundSprite.Descriptor.ToScaleX = 1.0;
            this.BackGroundSprite.Descriptor.ToScaleY = 1.0;
            this.BackGroundSprite.Descriptor.ToOpacity = 1;
            SpriteAnimation.ScaleToAnimation(this.BackGroundSprite, new Duration(TimeSpan.FromMilliseconds(1000)), 1.0, 1.0, -0.7, -0.7);
            SpriteAnimation.OpacityToAnimation(this.BackGroundSprite, new Duration(TimeSpan.FromMilliseconds(500)), 1, -0.6);

            this.ForeGroundSprite.Descriptor.ToScaleX = 1.4;
            this.ForeGroundSprite.Descriptor.ToScaleY = 1.4;
            this.ForeGroundSprite.Descriptor.ToOpacity = 0;
            SpriteAnimation.ScaleToAnimation(this.ForeGroundSprite, new Duration(TimeSpan.FromMilliseconds(0)), 1.4, 1.4, 0, 0);
            SpriteAnimation.OpacityToAnimation(this.ForeGroundSprite, new Duration(TimeSpan.FromMilliseconds(0)), 0, 0);

            this.ForeGroundSprite.Descriptor.ToScaleX = 1.0;
            this.ForeGroundSprite.Descriptor.ToScaleY = 1.0;
            this.ForeGroundSprite.Descriptor.ToOpacity = 1;
            SpriteAnimation.ScaleToAnimation(this.ForeGroundSprite, new Duration(TimeSpan.FromMilliseconds(1000)), 1.0, 1.0, -0.7, -0.7, cb: (_, __) =>
            {
                this.UiSprite.Descriptor.ToOpacity = 1;
                SpriteAnimation.OpacityToAnimation(this.UiSprite, new Duration(TimeSpan.FromMilliseconds(500)), 1, -0.6);
                this.Grid_WarpHolder.IsHitTestVisible = true;
            });
            SpriteAnimation.OpacityToAnimation(this.ForeGroundSprite, new Duration(TimeSpan.FromMilliseconds(500)), 1, -0.6);

            this.CloudHolderSprite.Descriptor.ToOpacity = 1;
            SpriteAnimation.OpacityToAnimation(this.CloudHolderSprite, new Duration(TimeSpan.FromMilliseconds(500)), 1, -0.6);

            this.cloudSb = new Storyboard();
            this.CloudSpriteA.Descriptor.ToX = -1920 / 2;
            this.CloudSpriteB.Descriptor.ToX = 1920 / 2;
            SpriteAnimation.XMoveToAnimation(this.CloudSpriteA, new Duration(TimeSpan.FromSeconds(300)), -1920 / 2, 0, null, cloudSb);
            SpriteAnimation.XMoveToAnimation(this.CloudSpriteB, new Duration(TimeSpan.FromSeconds(300)), 1920 / 2, 0, null, cloudSb);
            this.cloudSb.RepeatBehavior = new RepeatBehavior(int.MaxValue);
            this.cloudSb.Begin();
        }

        private void Page_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.Art_Image_Background != null)
            {
                var page = sender as LHStartPage;
                var p = e.GetPosition(page);
                var mx = page.ActualWidth / 2;
                var my = page.ActualHeight / 2;
                var dx = p.X - mx;
                var dy = p.Y - my;
                var rx = dx / mx;
                var ry = dy / my;
                var tx = rx * -4;
                var ty = ry * -4;
                this.Art_Image_Background.Margin = new Thickness(-4 + tx, -4 + ty, -4 - tx, -4 - ty);
            }
            if (this.Art_Image_Fore != null)
            {
                var page = sender as LHStartPage;
                var p = e.GetPosition(page);
                var mx = page.ActualWidth / 2;
                var my = page.ActualHeight / 2;
                var dx = p.X - mx;
                var dy = p.Y - my;
                var rx = dx / mx;
                var ry = dy / my;
                var tx = rx * -10;
                var ty = ry * -10;
                this.Art_Image_Fore.Margin = new Thickness(-10 + tx, -10 + ty, -10 - tx, -10 - ty);
            }
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

        private void Warp_Btn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                switch (((Image)sender).Name)
                {
                    case "Warp_Btn_Exit":
                        this.CanvasGroup_EnsureMask.Visibility = Visibility.Visible;
                        break;
                    case "Warp_Btn_Load":
                    case "Warp_Btn_Settings":
                        this.Grid_WarpHolder.IsHitTestVisible = false;
                        if (((Image)sender).Name == "Warp_Btn_Load")
                        {
                            ((LHSaveLoadPage)this.Frame_SaveLoad.Content).CurrentStaging = "load";
                            this.Frame_SaveLoad.Visibility = Visibility.Visible;
                            this.Frame_Settings.Visibility = Visibility.Hidden;
                            this.refreshTitleSubHead("load");
                        } else if (((Image)sender).Name == "Warp_Btn_Settings")
                        {
                            this.Frame_SaveLoad.Visibility = Visibility.Hidden;
                            this.Frame_Settings.Visibility = Visibility.Visible;
                            this.refreshTitleSubHead("settings");
                        }
                        this.WarpedPageSprite.DisplayBinding.Visibility = Visibility.Visible;
                        this.WarpedPageSprite.Descriptor.ToOpacity = 1;

                        SpriteAnimation.OpacityToAnimation(this.WarpedPageSprite, new Duration(TimeSpan.FromMilliseconds(300)), 1, 0, cb: (_, __) =>
                        {
                            if (((Image)sender).Name == "Warp_Btn_Load")
                            {
                                this.Frame_SaveLoad.IsHitTestVisible = true;
                            }
                            else if (((Image)sender).Name == "Warp_Btn_Settings")
                            {
                                this.Frame_Settings.IsHitTestVisible = true;
                            }
                            this.currentSubStage = ((Image)sender).Name.Replace("Warp_Btn_", "").ToLower();
                        });
                        if (((Image)sender).Name == "Warp_Btn_Load")
                        {
                            ((LHSaveLoadPage)this.Frame_SaveLoad.Content).WhenInto();
                        } else if (((Image)sender).Name == "Warp_Btn_Settings")
                        {
                            ((LHSettingsPage)this.Frame_Settings.Content).WhenInto();
                        }
                        break;
                    case "Warp_Btn_Start":
                        this.Grid_WarpHolder.IsHitTestVisible = false;
                        this.BlackAnimationBeforeLeave(() => this.PrepareClose());
                        break;
                    case "Warp_Btn_Gallery":
                        this.transitToGalleryPanel(true);
                        break;
                    case "Warp_Gallery_Btn_Back":
                        this.transitBackToRoot(true);
                        break;
                    case "Warp_Gallery_Btn_Staff":
                        this.refreshGalleryInto("Staff");
                        break;
                    case "Warp_Gallery_Btn_EDCard":
                        this.refreshGalleryInto("EDCard");
                        break;
                    case "Warp_Gallery_Btn_Music":
                        this.refreshGalleryInto("Music");
                        break;
                    case "Warp_Gallery_Btn_Art":
                        this.refreshGalleryInto("Art");
                        break;
                    case "Warp_Gallery_Btn_Bonus":
                        this.refreshGalleryInto("Bonus");
                        break;
                    default:
                        break;
                }
            }
        }

        private string currentGalleryStage = "root";
        private bool isGalleryAnimating = false;

        private void refreshGalleryInto(string type)
        {
            lock (this)
            {
                if (this.isGalleryAnimating)
                {
                    return;
                }
                this.isGalleryAnimating = true;
            }
            this.Canvas_Gallery_ButtonsWarp.IsHitTestVisible = false;
            foreach (var wrapper in this.Canvas_Gallery_ContentWarp.Children)
            {
                if (wrapper is Canvas)
                {
                    var wrapperCanvas = (Canvas)wrapper;
                    if (wrapperCanvas.Name.EndsWith("_Wrapper")) {
                        if (wrapperCanvas.Name == ("Canvas_Gallery_" + type + "_Wrapper"))
                        {
                            wrapperCanvas.Visibility = Visibility.Visible;
                            wrapperCanvas.Opacity = 1;
                        }
                        else
                        {
                            wrapperCanvas.Visibility = Visibility.Hidden;
                            wrapperCanvas.Opacity = 0;
                        }
                    }
                }
            }
            foreach (var btn in this.Canvas_Gallery_ButtonsWarp.Children)
            {
                if (btn is Image)
                {
                    if (((Image)btn).Name.StartsWith("Warp_Gallery_Btn_") && ((Image)btn).Name != ("Warp_Gallery_Btn_" + type))
                    {
                        ((Image)btn).Opacity = 0.3;
                    }
                }
            }

            // 进入刷新的具体逻辑
            switch (type)
            {
                case "EDCard":
                    var loopEndFool = (double)PersistContextDAO.Fetch("loop_end_fool") > 0;
                    if (loopEndFool)
                    {
                        var endPath = this.Image_Gallery_EDCard_FoolEnd.Source.ToString();
                        if (endPath.EndsWith("NoOpen.png"))
                        {
                            endPath = endPath.Replace("NoOpen.png", "FoolEnd.png");
                            BitmapImage endRotating;
                            endRotating = new BitmapImage();
                            endRotating.BeginInit();
                            endRotating.UriSource = new Uri(endPath);
                            endRotating.EndInit();
                            this.Image_Gallery_EDCard_FoolEnd.Source = endRotating;
                        }
                    }
                    var loopBadBad = (double)PersistContextDAO.Fetch("loop_end_bad") > 0;
                    if (loopBadBad)
                    {
                        var endPath = this.Image_Gallery_EDCard_BadEnd.Source.ToString();
                        if (endPath.EndsWith("NoOpen.png"))
                        {
                            endPath = endPath.Replace("NoOpen.png", "BadEnd.png");
                            BitmapImage endRotating;
                            endRotating = new BitmapImage();
                            endRotating.BeginInit();
                            endRotating.UriSource = new Uri(endPath);
                            endRotating.EndInit();
                            this.Image_Gallery_EDCard_BadEnd.Source = endRotating;
                        }
                    }
                    var loopEndNormal = (double)PersistContextDAO.Fetch("loop_end_normal") > 0;
                    if (loopEndNormal)
                    {
                        var endPath = this.Image_Gallery_EDCard_NormalEnd.Source.ToString();
                        if (endPath.EndsWith("NoOpen.png"))
                        {
                            endPath = endPath.Replace("NoOpen.png", "NormalEnd.png");
                            BitmapImage endRotating;
                            endRotating = new BitmapImage();
                            endRotating.BeginInit();
                            endRotating.UriSource = new Uri(endPath);
                            endRotating.EndInit();
                            this.Image_Gallery_EDCard_NormalEnd.Source = endRotating;
                        }
                    }
                    var loopEndTrue = (double)PersistContextDAO.Fetch("loop_end_true") > 0;
                    if (loopEndTrue)
                    {
                        var endPath = this.Image_Gallery_EDCard_TrueEnd.Source.ToString();
                        if (endPath.EndsWith("NoOpen.png"))
                        {
                            endPath = endPath.Replace("NoOpen.png", "TrueEnd.png");
                            BitmapImage endRotating;
                            endRotating = new BitmapImage();
                            endRotating.BeginInit();
                            endRotating.UriSource = new Uri(endPath);
                            endRotating.EndInit();
                            this.Image_Gallery_EDCard_TrueEnd.Source = endRotating;
                        }
                    }
                    string loopTimesStr;
                    try
                    {
                        var loopTimes = PersistContextDAO.Fetch("Game_LoopTime");
                        int loopTimeRaw = Convert.ToInt32(loopTimes);
                        if (loopTimeRaw >= 100)
                        {
                            loopTimesStr = "99+";
                        } else
                        {
                            loopTimesStr = loopTimeRaw.ToString();
                        }
                    } catch (Exception e)
                    {
                        loopTimesStr = "N/A";
                    }
                    this.Label_Gallery_EDCard_LoopTime_Hint.Content = $"通关次数：{loopTimesStr}";
                    string accTimesStr;
                    try
                    {
                        TimeSpan accTimes = (TimeSpan)PersistContextDAO.Fetch("___YURIRI@ACCDURATION___");
                        accTimesStr = "";
                        if (accTimes.Days != 0)
                        {
                            accTimesStr += accTimes.Days + "天";
                        }
                        if (accTimes.Hours != 0)
                        {
                            accTimesStr += accTimes.Hours + "小时";
                        }
                        accTimesStr += accTimes.Minutes + "分钟";
                    }
                    catch (Exception e)
                    {
                        accTimesStr = "N/A";
                    }
                    this.Label_Gallery_EDCard_AccTime_Hint.Content = $"累计时长：{accTimesStr}";
                    break;
                case "Music":
                    this.RefreshMusicPlayingStatus();
                    break;
                case "Bonus":
                    try
                    {
                        ((LongStoryPage)this.Frame_BonusStory.Content).PrepareOpen();
                    }
                    catch (Exception e)
                    {
                        LogUtils.LogLine("cannot open bonus long story page, " + e.ToString(), nameof(LHStartPage), LogLevel.Error);
                    }
                    break;
                default:
                    break;
            }

            this.GalleryContentWarpSprite.DisplayBinding.Visibility = Visibility.Visible;
            this.GalleryContentWarpSprite.DisplayBinding.IsHitTestVisible = false;
            this.GalleryContentWarpSprite.Descriptor.ToOpacity = 1;
            SpriteAnimation.OpacityToAnimation(this.GalleryContentWarpSprite, new Duration(TimeSpan.FromMilliseconds(300)), 1, 0,
                cb: (_, __) => 
                {
                    this.GalleryContentWarpSprite.DisplayBinding.IsHitTestVisible = true;
                    this.currentGalleryStage = type;
                    this.isGalleryAnimating = false;
                });
        }

        private bool isShowingCG = false;

        private void refreshGalleryOut()
        {
            if (this.isShowingCG)
            {
                return;
            }
            foreach (var btn in this.Canvas_Gallery_ButtonsWarp.Children)
            {
                if (btn is Image)
                {
                    if (((Image)btn).Name.StartsWith("Warp_Gallery_Btn_"))
                    {
                        ((Image)btn).Opacity = 1;
                    }
                }
            }
            this.Canvas_Gallery_ContentWarp.IsHitTestVisible = false;
            this.GalleryContentWarpSprite.Descriptor.ToOpacity = 0;
            SpriteAnimation.OpacityToAnimation(this.GalleryContentWarpSprite, new Duration(TimeSpan.FromMilliseconds(300)), 0, 0,
                cb: (_, __) =>
                {
                    this.Canvas_Gallery_ButtonsWarp.IsHitTestVisible = true;
                    this.Canvas_Gallery_ContentWarp.Visibility = Visibility.Hidden;
                    this.currentGalleryStage = "root";
                });
        }

        private void refreshTitleSubHead(string toStage)
        {
            var rawSp = this.Image_Title_Ground.Source.ToString();
            var rp = rawSp.Substring(0, rawSp.LastIndexOf("/") + "Title_UI_".Length);
            string sourcePath = rp;
            switch (toStage)
            {
                case "load":
                    sourcePath += "_Load_Ground.png";
                    break;
                case "settings":
                    sourcePath += "_Settings_Ground.png";
                    break;
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
            this.Image_Title_Ground.Source = rotating;
        }

        private void Image_Button_No_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.CanvasGroup_EnsureMask.Visibility = Visibility.Hidden;
        }

        private void Image_Button_Yes_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.BlackAnimationBeforeLeave(() => this.core.GetMainRender().Shutdown());
        }

        public void HandleResumeFromLoad()
        {
            this.isLoading = true;
            this.BlackAnimationBeforeLeave(() => this.PrepareClose());
        }

        private void BlackAnimationBeforeLeave(StartPageExitAniCallback exitCb)
        {
            this.MaskSprite.DisplayBinding.Visibility = Visibility.Visible;
            this.MaskSprite.Descriptor.ToOpacity = 1;
            SpriteAnimation.OpacityToAnimation(this.MaskSprite, new Duration(TimeSpan.FromMilliseconds(1000)), 1, 0, cb: (_, __) =>
            {
                exitCb?.Invoke();
            });
        }

        private void Image_Load_Button_Back_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Frame_SaveLoad.IsHitTestVisible = false;
            this.WarpedPageSprite.Descriptor.ToOpacity = 0;
            SpriteAnimation.OpacityToAnimation(this.WarpedPageSprite, new Duration(TimeSpan.FromMilliseconds(200)), 0, 0, cb: (_, __) =>
            {
                this.Frame_SaveLoad.IsHitTestVisible = false;
                this.WarpedPageSprite.DisplayBinding.Visibility = Visibility.Hidden;
                this.currentSubStage = "root";
                this.Grid_WarpHolder.IsHitTestVisible = true;
            });
            switch (this.currentSubStage)
            {
                case "load":
                    ((LHSaveLoadPage)this.Frame_SaveLoad.Content).WhenLeave();
                    break;
                case "settings":
                    ((LHSettingsPage)this.Frame_Settings.Content).WhenLeave();
                    break;
            }
        }

        private void Page_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.isGalleryAnimating || this.transitingBackFromGalleryToRoot)
            {
                return;
            }
            switch (this.currentSubStage)
            {
                case "settings":
                case "load":
                    this.Image_Load_Button_Back_MouseDown(null, null);
                    break;
                case "gallery":
                    if (this.currentGalleryStage == "root")
                    {
                        this.transitBackToRoot(true);
                    }
                    else if (this.currentGalleryStage == "Bonus")
                    {
                        return;
                    }
                    else
                    {
                        this.refreshGalleryOut();
                    }
                    break;
            }
        }

        public void onBonusExit()
        {
            this.refreshGalleryOut();
        }

        private void transitToGalleryPanel(bool withWindow)
        {
            this.GalleryContentWarpSprite.DisplayBinding.Visibility = Visibility.Hidden;
            this.GalleryContentWarpSprite.DisplayBinding.IsHitTestVisible = false;

            this.Grid_WarpHolder.IsHitTestVisible = false;
            this.Canvas_Gallery.Visibility = Visibility.Visible;
            this.Canvas_Gallery.IsHitTestVisible = false;

            this.GalleryContentWarpSprite.Descriptor.ToOpacity = 0;
            SpriteAnimation.OpacityToAnimation(this.GalleryContentWarpSprite, new Duration(TimeSpan.Zero), 0, 0);
            this.GalleryWarpSprite.Descriptor.ToOpacity = 0;
            SpriteAnimation.OpacityToAnimation(this.GalleryWarpSprite, new Duration(TimeSpan.Zero), 0, 0);

            Storyboard sb = new Storyboard();

            if (withWindow)
            {
                this.BackGroundSprite.Descriptor.ToScaleX = 1.2;
                this.BackGroundSprite.Descriptor.ToScaleY = 1.2;
                this.BackGroundSprite.Descriptor.ToOpacity = 0;
                SpriteAnimation.ScaleToAnimation(this.BackGroundSprite, new Duration(TimeSpan.FromMilliseconds(300)), 1.2, 1.2, 0.6, 0.6, providedStory: sb);
                SpriteAnimation.OpacityToAnimation(this.BackGroundSprite, new Duration(TimeSpan.FromMilliseconds(300)), 0, 0.6, providedStory: sb);
            }

            this.ForeGroundSprite.Descriptor.ToScaleX = 1.4;
            this.ForeGroundSprite.Descriptor.ToScaleY = 1.4;
            this.ForeGroundSprite.Descriptor.ToOpacity = 0;
            SpriteAnimation.ScaleToAnimation(this.ForeGroundSprite, new Duration(TimeSpan.FromMilliseconds(300)), 1.4, 1.4, 0.6, 0.6, providedStory: sb);
            SpriteAnimation.OpacityToAnimation(this.ForeGroundSprite, new Duration(TimeSpan.FromMilliseconds(300)), 0, 0.6, providedStory: sb);
            SpriteAnimation.BlurMutexAnimation(this.ForeGroundSprite, new Duration(TimeSpan.FromMilliseconds(300)), 0, 20, providedStory: sb);

            this.RootButtonSprite.Descriptor.ToOpacity = 0;
            SpriteAnimation.OpacityToAnimation(this.RootButtonSprite, new Duration(TimeSpan.FromMilliseconds(300)), 0, 0.6, providedStory: sb);

            this.GalleryWarpSprite.Descriptor.ToOpacity = 1;
            SpriteAnimation.OpacityToAnimation(this.GalleryWarpSprite, new Duration(TimeSpan.FromMilliseconds(300)), 1, -0.6, providedStory: sb);

            sb.Completed += (_, __) =>
            {
                this.currentSubStage = "gallery";
                this.Canvas_Gallery.IsHitTestVisible = true;
            };

            sb.Begin();
        }

        private bool transitingBackFromGalleryToRoot = false;

        private void transitBackToRoot(bool withWindow)
        {
            this.transitingBackFromGalleryToRoot = true;
            this.Canvas_Gallery.IsHitTestVisible = false;
            Storyboard sb = new Storyboard();
            if (withWindow)
            {
                this.BackGroundSprite.Descriptor.ToScaleX = 1;
                this.BackGroundSprite.Descriptor.ToScaleY = 1;
                this.BackGroundSprite.Descriptor.ToOpacity = 1;
                SpriteAnimation.ScaleToAnimation(this.BackGroundSprite, new Duration(TimeSpan.FromMilliseconds(300)), 1, 1, -0.6, -0.6, providedStory: sb);
                SpriteAnimation.OpacityToAnimation(this.BackGroundSprite, new Duration(TimeSpan.FromMilliseconds(300)), 1, -0.6, providedStory: sb);
            }

            this.ForeGroundSprite.Descriptor.ToScaleX = 1;
            this.ForeGroundSprite.Descriptor.ToScaleY = 1;
            this.ForeGroundSprite.Descriptor.ToOpacity = 1;
            SpriteAnimation.ScaleToAnimation(this.ForeGroundSprite, new Duration(TimeSpan.FromMilliseconds(300)), 1, 1, -0.6, -0.6, providedStory: sb);
            SpriteAnimation.OpacityToAnimation(this.ForeGroundSprite, new Duration(TimeSpan.FromMilliseconds(300)), 1, -0.6, providedStory: sb);
            SpriteAnimation.BlurMutexAnimation(this.ForeGroundSprite, new Duration(TimeSpan.FromMilliseconds(300)), 20, 0, providedStory: sb);

            this.RootButtonSprite.Descriptor.ToOpacity = 1;
            SpriteAnimation.OpacityToAnimation(this.RootButtonSprite, new Duration(TimeSpan.FromMilliseconds(300)), 1, -0.6, providedStory: sb);

            this.GalleryWarpSprite.Descriptor.ToOpacity = 0;
            SpriteAnimation.OpacityToAnimation(this.GalleryWarpSprite, new Duration(TimeSpan.FromMilliseconds(300)), 0, 0.6, providedStory: sb);

            sb.Completed += (_, __) =>
            {
                this.currentSubStage = "root";
                this.Canvas_Gallery.Visibility = Visibility.Hidden;
                this.Grid_WarpHolder.IsHitTestVisible = true;
                this.transitingBackFromGalleryToRoot = false;
            };

            sb.Begin();
        }

        private void Image_Gallery_Staff_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Page_PreviewMouseRightButtonUp(sender, e);
        }

        private string titlePlayingBGM { get; set; } = "2";

        private void Image_Gallery_MusicItem_MouseEnter(object sender, MouseEventArgs e)
        {
            ((Image)sender).Opacity = 1;
        }

        private void Image_Gallery_MusicItem_MouseLeave(object sender, MouseEventArgs e)
        {
            ((Image)sender).Opacity = 0.7;
        }

        private void Image_Gallery_MusicItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var selectedName = ((Image)sender).Name;
            var selectedIdx = selectedName[selectedName.Length - 1].ToString();
            if (selectedIdx == this.titlePlayingBGM)
            {
                return;
            }
            this.titlePlayingBGM = selectedIdx;
            var selectedLabel = (Label) this.Canvas_Gallery_Music_Wrapper.FindName("LabelRadio_Gallery_Music_" + this.titlePlayingBGM);
            this.RefreshMusicPlayingStatus();

            switch (this.titlePlayingBGM)
            {
                case "1":
                    Director.GetInstance().GetMainRender().Bgm("1_暮映银丝.wav", GlobalConfigContext.GAME_SOUND_BGMVOL);
                    break;
                case "2":
                    Director.GetInstance().GetMainRender().Bgm("2_以谎织缘.wav", GlobalConfigContext.GAME_SOUND_BGMVOL);
                    break;
                case "3":
                    Director.GetInstance().GetMainRender().Bgm("3_心乱兮怅惘.wav", GlobalConfigContext.GAME_SOUND_BGMVOL);
                    break;
                case "4":
                    Director.GetInstance().GetMainRender().Bgm("4_真挚的坦白.wav", GlobalConfigContext.GAME_SOUND_BGMVOL);
                    break;
            }
        }

        private void RefreshMusicPlayingStatus()
        {
            foreach (var cItem in this.Canvas_Gallery_Music_Wrapper.Children)
            {
                if (cItem is Label cLabel)
                {
                    if (cLabel.Name[cLabel.Name.Length - 1].ToString() == this.titlePlayingBGM)
                    {
                        cLabel.Foreground = new SolidColorBrush(Color.FromRgb(129, 179, 255));
                    }
                    else
                    {
                        cLabel.Foreground = new SolidColorBrush(Colors.Black);
                    }
                }
            }
        }

        private void Image_ArtItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var name = ((Image)sender).Name;
            var cgName = name.Replace("Image_", "") + ".png";
            var cg = ResourceManager.GetInstance().GetPicture(cgName, ResourceManager.FullImageRect);
            this.Image_Art_Displayer.Source = cg.SpriteBitmapImage;
            this.Image_Art_Displayer.Visibility = Visibility.Visible;
            this.isShowingCG = true;
            this.Canvas_ArtItem_Warp.IsHitTestVisible = false;
            this.Image_Art_Displayer.IsHitTestVisible = false;
            this.GalleryArtSprite.Descriptor.ToOpacity = 1;
            SpriteAnimation.OpacityToAnimation(this.GalleryArtSprite, TimeSpan.FromMilliseconds(300), 1, 0, cb: (_, __) =>
            {
                this.Image_Art_Displayer.IsHitTestVisible = true;
            });
        }

        private void Image_ArtDisplayer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.Image_Art_Displayer.IsHitTestVisible = false;
                this.GalleryArtSprite.Descriptor.ToOpacity = 0;
                SpriteAnimation.OpacityToAnimation(this.GalleryArtSprite, TimeSpan.FromMilliseconds(300), 0, 0, cb: (_, __) =>
                {
                    this.Image_Art_Displayer.Visibility = Visibility.Hidden;
                    this.Canvas_ArtItem_Warp.IsHitTestVisible = true;
                    this.isShowingCG = false;
                });
            }
        }
    }
}
