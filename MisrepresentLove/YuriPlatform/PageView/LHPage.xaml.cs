using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Yuri.PlatformCore;
using Yuri.PlatformCore.Graphic;
using Yuri.PlatformCore.VM;
using Yuri.Yuriri;

namespace Yuri.PageView
{
    /// <summary>
    /// LHPage.xaml 的交互逻辑
    /// </summary>
    public partial class LHPage : Page, YuriPage, RenderablePage, ILoadablePage
    {
        private static readonly ResourceManager resMana = ResourceManager.GetInstance();

        private static string CallbackTarget = String.Empty;
        private string EpisodeHint = String.Empty;
        private string EpisodeNameHint = String.Empty;

        private YuriSprite MainStageSprite;
        private YuriSprite NameLogoSprite;
        private YuriSprite PanelSprite;
        private YuriSprite WhitePanelSprite;
        private YuriSprite ButtonGroupSprite;
        private YuriSprite CommonBackButtonSprite;
        private YuriSprite EpisodeHintSprite;
        private YuriSprite SubTitleSprite;

        private YuriSprite BacklogStackSprite;
        private YuriSprite SaveLoadPageSprite;
        private YuriSprite SettingsPageSprite;

        private Director core = Director.GetInstance();

        private readonly Dictionary<string, BitmapImage> cachingButtons = new Dictionary<string, BitmapImage>();

        private string currentStaging = "root";

        private bool IsAllowClose = false;
        private DateTime ReopenTs;

        public LHPage()
        {
            InitializeComponent();

            this.Frame_SaveLoad.Content = new LHSaveLoadPage(this);
            this.Frame_Settings.Content = new LHSettingsPage(this);

            this.MainStageSprite = new YuriSprite()
            {
                DisplayBinding = ViewManager.mWnd.mainFrame,
                AnimationElement = ViewManager.mWnd.mainFrame,
                Descriptor = new SpriteDescriptor()
            };
            this.PanelSprite = new YuriSprite()
            {
                DisplayBinding = this.Image_Panel,
                AnimationElement = this.Image_Panel,
                Descriptor = new SpriteDescriptor()
            };
            this.WhitePanelSprite = new YuriSprite()
            {
                DisplayBinding = this.Image_WhiteSpan,
                AnimationElement = this.Image_WhiteSpan,
                Descriptor = new SpriteDescriptor()
            };
            this.NameLogoSprite = new YuriSprite()
            {
                DisplayBinding = this.Image_NameLogo,
                AnimationElement = this.Image_NameLogo,
                Descriptor = new SpriteDescriptor()
            };
            this.ButtonGroupSprite = new YuriSprite()
            {
                DisplayBinding = this.Canvas_RightButtonGroups,
                AnimationElement = this.Canvas_RightButtonGroups,
                Descriptor = new SpriteDescriptor()
            };
            this.CommonBackButtonSprite = new YuriSprite()
            {
                DisplayBinding = this.Image_Button_Back,
                AnimationElement = this.Image_Button_Back,
                Descriptor = new SpriteDescriptor()
            };
            this.EpisodeHintSprite = new YuriSprite()
            {
                DisplayBinding = this.Canvas_EpHinter,
                AnimationElement = this.Canvas_EpHinter,
                Descriptor = new SpriteDescriptor()
            };
            this.SubTitleSprite = new YuriSprite()
            {
                DisplayBinding = this.Image_SubTitle,
                AnimationElement = this.Image_SubTitle,
                Descriptor = new SpriteDescriptor()
            };

            this.BacklogStackSprite = new YuriSprite()
            {
                DisplayBinding = this.Scroll_BacklogBoxing,
                AnimationElement = this.Scroll_BacklogBoxing,
                Descriptor = new SpriteDescriptor()
            };
            this.SaveLoadPageSprite = new YuriSprite()
            {
                DisplayBinding = this.Frame_SaveLoad,
                AnimationElement = this.Frame_SaveLoad,
                Descriptor = new SpriteDescriptor()
            };
            this.SettingsPageSprite = new YuriSprite()
            {
                DisplayBinding = this.Frame_Settings,
                AnimationElement = this.Frame_Settings,
                Descriptor = new SpriteDescriptor()
            };
        }

        public void OnSceneActionDone(SceneAction action)
        {
            // nothing
        }

        public void PrepareClose()
        {
            ViewPageManager.CollapseUIPage();
        }

        public void PrepareOpen()
        {
            this.currentStaging = "root";
            this.isResumingFromLoad = false;
            this.IsFromAutoChanged = false;
            this.IsFromReturnTitle = false;
            this.Image_Button_Auto.Visibility = Visibility.Visible;
            this.CanvasGroup_EnsureMask.Visibility = Visibility.Hidden;
            
            // 检查回调标志位
            CallbackTarget = SymbolTable.GetInstance().GlobalCtxDao.GlobalSymbolTable.Fetch("tracing_callback").ToString();
            this.EpisodeHint = SymbolTable.GetInstance().GlobalCtxDao.GlobalSymbolTable.Fetch("gb_episode_hint")?.ToString();
            this.EpisodeNameHint = SymbolTable.GetInstance().GlobalCtxDao.GlobalSymbolTable.Fetch("gb_episode_name_hint")?.ToString();
            this.ReopenTs = DateTime.Now;

            this.Label_Hint_Episode.Content = this.EpisodeHint;
            this.Label_Hint_EpName.Content = this.EpisodeNameHint;

            Storyboard startupSb = new Storyboard();

            this.PanelSprite.Descriptor.ToX = 1762;
            SpriteAnimation.XMoveToAnimation(this.PanelSprite, new Duration(TimeSpan.FromMilliseconds(300)), 1762, -0.8, providedStory: startupSb);

            this.NameLogoSprite.Descriptor.ToOpacity = 1;
            SpriteAnimation.OpacityToAnimation(this.NameLogoSprite, new Duration(TimeSpan.FromMilliseconds(300)), 1, 0, providedStory: startupSb);
            this.EpisodeHintSprite.Descriptor.ToOpacity = 1;
            SpriteAnimation.OpacityToAnimation(this.EpisodeHintSprite, new Duration(TimeSpan.FromMilliseconds(300)), 1, 0, providedStory: startupSb);
            this.ButtonGroupSprite.Descriptor.ToOpacity = 1;
            this.ButtonGroupSprite.Descriptor.ToY = 1080 / 2;
            SpriteAnimation.OpacityToAnimation(this.ButtonGroupSprite, new Duration(TimeSpan.FromMilliseconds(300)), 1, 0, providedStory: startupSb);
            SpriteAnimation.YMoveToAnimation(this.ButtonGroupSprite, new Duration(TimeSpan.FromMilliseconds(300)), 1080 / 2, -0.8, providedStory: startupSb);

            SpriteAnimation.BlurMutexAnimation(this.MainStageSprite, TimeSpan.FromMilliseconds(300), 0, 30, providedStory: startupSb);
            this.currentStaging = "root";
            startupSb.Completed += (_, __) =>
            {
                this.IsAllowClose = true;
            };

            if (this.core.GetMainRender().IsBranching)
            {
                this.Image_Button_Auto.Visibility = Visibility.Hidden;
            }

            startupSb.Begin();
        }

        public bool PreviewSceneAction(SceneAction action)
        {
            return false;
        }

        private void Page_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e != null)
            {
                this.core.UpdateMouse(e);
            }
            if (this.isResumingFromLoad || (!SpriteAnimation.IsAnyAnimation && this.IsAllowClose))
            {
                if (this.currentStaging == "root")
                {
                    this.IsAllowClose = false;
                    if (CallbackTarget != String.Empty)
                    {
                        var callbackNtr = new Interrupt()
                        {
                            Detail = "LHRCCallbackNTR",
                            InterruptSA = null,
                            Type = InterruptType.ButtonJump,
                            ReturnTarget = CallbackTarget,
                            ExitWait = true
                        };
                        // 提交返回主舞台的中断到主调用堆栈
                        Director.RunMana.CallStack.Submit(callbackNtr);
                        // 重置回调
                        CallbackTarget = String.Empty;
                        Director.RunMana.Symbols.GlobalCtxDao.GlobalSymbolTable.Assign("tracing_callback", String.Empty);
                    }
                    Storyboard closeSb = new Storyboard();
                    this.PanelSprite.Descriptor.ToX = 1920 + 1920 / 2;
                    SpriteAnimation.XMoveToAnimation(this.PanelSprite, new Duration(TimeSpan.FromMilliseconds(150)), 1920 + 1920 / 2, -0.8, providedStory: closeSb);
                    this.NameLogoSprite.Descriptor.ToOpacity = 0;
                    SpriteAnimation.OpacityToAnimation(this.NameLogoSprite, new Duration(TimeSpan.FromMilliseconds(150)), 0, 0, providedStory: closeSb);
                    this.EpisodeHintSprite.Descriptor.ToOpacity = 0;
                    SpriteAnimation.OpacityToAnimation(this.EpisodeHintSprite, new Duration(TimeSpan.FromMilliseconds(150)), 0, 0, providedStory: closeSb);
                    this.ButtonGroupSprite.Descriptor.ToOpacity = 0;
                    this.ButtonGroupSprite.Descriptor.ToY = 1080 / 2 + 50;
                    SpriteAnimation.OpacityToAnimation(this.ButtonGroupSprite, new Duration(TimeSpan.FromMilliseconds(150)), 0, 0, providedStory: closeSb);
                    SpriteAnimation.YMoveToAnimation(this.ButtonGroupSprite, new Duration(TimeSpan.FromMilliseconds(150)), 1080 / 2 + 25, 0, providedStory: closeSb);
                    SpriteAnimation.BlurMutexAnimation(this.MainStageSprite, TimeSpan.FromMilliseconds(300), 0, 0, providedStory: closeSb);
                    closeSb.Completed += (_, __) =>
                    {
                        this.PrepareClose();
                        this.core.GetMainRender().ResumeFromRclick();
                        if (this.IsFromAutoChanged)
                        {
                            RollbackManager.IsRollingBack = false;
                            Director.RunMana.IsAutoplaying = true;
                            Director.RunMana.ExitUserWait();
                            Director.RunMana.AutoPlayWait();
                            this.core.GetMainRender().HideMessageTria();
                            NotificationManager.Notify(String.Empty, "已启用自动播放", String.Empty, 1500);
                            this.IsFromAutoChanged = false;
                        }
                        else if (this.IsFromReturnTitle)
                        {
                            this.IsFromReturnTitle = false;
                            this.core.GetMainRender().Title();
                        }
                    };
                    closeSb.Begin();
                }
                else if (this.currentStaging == "backlog")
                {
                    this.HandleBackward((_, __) => this.currentStaging = "root");
                }
                else if (this.currentStaging == "save")
                {
                    this.HandleBackward((_, __) => this.currentStaging = "root");
                }
                else if (this.currentStaging == "load")
                {
                    this.HandleBackward((_, __) => this.currentStaging = "root");
                }
                else if (this.currentStaging == "settings")
                {
                    this.HandleBackward((_, __) => this.currentStaging = "root");
                }
            }
        }

        private bool isResumingFromLoad = false;

        public void HandleResumeFromLoad()
        {
            this.HandleBackward((_, __) => 
            {
                LHPage.CallbackTarget = String.Empty;
                this.currentStaging = "root";
                this.isResumingFromLoad = true;
                this.Page_PreviewMouseRightButtonUp(null, null);
            });
        }

        private void Image_Button_Outer_MouseEnter(object sender, MouseEventArgs e)
        {
            var sourcePath = ((Image)sender).Source.ToString().Replace(".png", "_Select.png");
            BitmapImage rotating;
            if (this.cachingButtons.ContainsKey(sourcePath))
            {
                rotating = this.cachingButtons[sourcePath];
            } else
            {
                rotating = new BitmapImage();
                rotating.BeginInit();
                rotating.UriSource = new Uri(sourcePath);
                rotating.EndInit();
                this.cachingButtons[sourcePath] = rotating;
            }
            ((Image)sender).Source = rotating;
        }

        private void Image_Button_Outer_MouseLeave(object sender, MouseEventArgs e)
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

        private void IntoSubModuleAnimation(SpriteAnimation.SpriteAnimationCallback callback)
        {
            Storyboard sb = new Storyboard();
            this.PanelSprite.Descriptor.ToX = -155;
            SpriteAnimation.XMoveToAnimation(this.PanelSprite, new Duration(TimeSpan.FromMilliseconds(200)), -155, 0.8, providedStory: sb);
            this.WhitePanelSprite.Descriptor.ToX = 1920 / 2;
            SpriteAnimation.XMoveToAnimation(this.WhitePanelSprite, new Duration(TimeSpan.FromMilliseconds(200)), 1920 / 2, 0.8, providedStory: sb);
            this.ButtonGroupSprite.Descriptor.ToOpacity = 0;
            this.ButtonGroupSprite.Descriptor.ToY = 1080 / 2 + 50;
            this.ButtonGroupSprite.DisplayBinding.IsHitTestVisible = false;
            SpriteAnimation.OpacityToAnimation(this.ButtonGroupSprite, new Duration(TimeSpan.FromMilliseconds(200)), 0, 0, providedStory: sb);
            SpriteAnimation.YMoveToAnimation(this.ButtonGroupSprite, new Duration(TimeSpan.FromMilliseconds(200)), 1080 / 2 + 25, -0.8, providedStory: sb);
            this.CommonBackButtonSprite.Descriptor.ToOpacity = 1;
            SpriteAnimation.OpacityToAnimation(this.CommonBackButtonSprite, new Duration(TimeSpan.FromMilliseconds(200)), 1, 0, cb: (_, __) => {
                this.CommonBackButtonSprite.DisplayBinding.IsHitTestVisible = true;
            }, providedStory: sb);
            this.EpisodeHintSprite.Descriptor.ToOpacity = 0;
            SpriteAnimation.OpacityToAnimation(this.EpisodeHintSprite, new Duration(TimeSpan.FromMilliseconds(200)), 0, 0, providedStory: sb);
            sb.Completed += (_, __) => callback?.Invoke(_, __);
            sb.Begin();
        }

        private void ReturnSubModuleAnimation(SpriteAnimation.SpriteAnimationCallback callback)
        {
            Storyboard sb = new Storyboard();
            this.PanelSprite.Descriptor.ToX = 1761;
            SpriteAnimation.XMoveToAnimation(this.PanelSprite, new Duration(TimeSpan.FromMilliseconds(200)), 1761, -0.8, providedStory: sb);
            this.WhitePanelSprite.Descriptor.ToX = 1920 + 1920 / 2;
            SpriteAnimation.XMoveToAnimation(this.WhitePanelSprite, new Duration(TimeSpan.FromMilliseconds(200)), 1920 + 1920 / 2, -0.8, providedStory: sb);
            this.ButtonGroupSprite.Descriptor.ToOpacity = 1;
            this.ButtonGroupSprite.Descriptor.ToY = 1080 / 2;
            SpriteAnimation.OpacityToAnimation(this.ButtonGroupSprite, new Duration(TimeSpan.FromMilliseconds(200)), 1, 0, providedStory: sb);
            SpriteAnimation.YMoveToAnimation(this.ButtonGroupSprite, new Duration(TimeSpan.FromMilliseconds(200)), 1080 / 2, -0.8, (_, __) => {
                this.ButtonGroupSprite.DisplayBinding.IsHitTestVisible = true;
            }, providedStory: sb);
            this.CommonBackButtonSprite.DisplayBinding.IsHitTestVisible = false;
            this.CommonBackButtonSprite.Descriptor.ToOpacity = 0;
            SpriteAnimation.OpacityToAnimation(this.CommonBackButtonSprite, new Duration(TimeSpan.FromMilliseconds(200)), 0, 0, providedStory: sb);
            this.EpisodeHintSprite.Descriptor.ToOpacity = 1;
            SpriteAnimation.OpacityToAnimation(this.EpisodeHintSprite, new Duration(TimeSpan.FromMilliseconds(300)), 1, 0, providedStory: sb);
            sb.Completed += (_, __) => callback?.Invoke(_, __);
            sb.Begin();
        }

        private void Image_Button_Back_MouseDown(object sender, MouseButtonEventArgs e)
        {
            switch (this.currentStaging)
            {
                case "backlog":
                case "save":
                case "load":
                case "settings":
                    this.HandleBackward((_, __) => this.currentStaging = "root");
                    break;
            }
        }

        private void HandleBackward(SpriteAnimation.SpriteAnimationCallback callback)
        {
            switch (this.currentStaging)
            {
                case "backlog":
                    this.BacklogStackSprite.Descriptor.ToOpacity = 0;
                    SpriteAnimation.OpacityToAnimation(this.BacklogStackSprite, new Duration(TimeSpan.FromMilliseconds(100)), 0, 0, cb: (_, __) => {
                        this.StackPanel_Backlog_MainStack.Children.Clear();
                    });
                    break;
                case "save":
                case "load":
                    var shouldExit = ((LHSaveLoadPage)this.Frame_SaveLoad.Content).WhenLeave();
                    if (!shouldExit)
                    {
                        return;
                    }
                    this.SaveLoadPageSprite.Descriptor.ToOpacity = 0;
                    SpriteAnimation.OpacityToAnimation(this.SaveLoadPageSprite, new Duration(TimeSpan.FromMilliseconds(100)), 0, 0, cb: (_, __) => {
                        this.SaveLoadPageSprite.DisplayBinding.Visibility = Visibility.Hidden;
                    });
                    break;
                case "settings":
                    ((LHSettingsPage)this.Frame_Settings.Content).WhenLeave();
                    this.SettingsPageSprite.Descriptor.ToOpacity = 0;
                    SpriteAnimation.OpacityToAnimation(this.SettingsPageSprite, new Duration(TimeSpan.FromMilliseconds(100)), 0, 0, cb: (_, __) => {
                        this.SettingsPageSprite.DisplayBinding.Visibility = Visibility.Hidden;
                    });
                    break;
            }
            this.SubTitleSprite.Descriptor.ToOpacity = 0;
            SpriteAnimation.OpacityToAnimation(this.SubTitleSprite, new Duration(TimeSpan.FromMilliseconds(100)), 0, 0);
            this.ReturnSubModuleAnimation(callback);
        }

        private void Image_Button_Backlog_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.LoadBacklogItems();
            this.Scroll_BacklogBoxing.ScrollToEnd();
            this.LoadSubTitle("Backlog");
            this.BacklogStackSprite.Descriptor.ToOpacity = 1;
            SpriteAnimation.OpacityToAnimation(this.BacklogStackSprite, new Duration(TimeSpan.FromMilliseconds(250)), 1, 0);
            this.IntoSubModuleAnimation((tSender, tArgs) => {
                this.currentStaging = "backlog";
            });
        }

        private void Image_Button_Save_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ((LHSaveLoadPage)this.Frame_SaveLoad.Content).CurrentStaging = "save";
            this.LoadSubTitle("Save");
            this.SaveLoadPageSprite.DisplayBinding.Visibility = Visibility.Visible;
            this.SaveLoadPageSprite.Descriptor.ToOpacity = 1;
            SpriteAnimation.OpacityToAnimation(this.SaveLoadPageSprite, new Duration(TimeSpan.FromMilliseconds(250)), 1, 0);
            this.IntoSubModuleAnimation((tSender, tArgs) => {
                this.currentStaging = "save";
            });
            ((LHSaveLoadPage)this.Frame_SaveLoad.Content).WhenInto();
        }

        private void Image_Button_Load_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ((LHSaveLoadPage)this.Frame_SaveLoad.Content).CurrentStaging = "load";
            this.LoadSubTitle("Load");
            this.SaveLoadPageSprite.DisplayBinding.Visibility = Visibility.Visible;
            this.SaveLoadPageSprite.Descriptor.ToOpacity = 1;
            SpriteAnimation.OpacityToAnimation(this.SaveLoadPageSprite, new Duration(TimeSpan.FromMilliseconds(250)), 1, 0);
            this.IntoSubModuleAnimation((tSender, tArgs) => {
                this.currentStaging = "load";
            });
            ((LHSaveLoadPage)this.Frame_SaveLoad.Content).WhenInto();
        }

        private bool IsFromAutoChanged = false;
        private bool IsFromReturnTitle = false;

        private void Image_Button_Auto_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!this.core.GetMainRender().IsBranching)
            {
                this.IsFromAutoChanged = true;
                this.Page_PreviewMouseRightButtonUp(null, null);
            }
        }

        private void Image_Button_Title_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.CanvasGroup_EnsureMask.Visibility = Visibility.Visible;
        }

        private void Image_Button_Settings_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.LoadSubTitle("Settings");
            this.SettingsPageSprite.DisplayBinding.Visibility = Visibility.Visible;
            this.SettingsPageSprite.Descriptor.ToOpacity = 1;
            SpriteAnimation.OpacityToAnimation(this.SettingsPageSprite, new Duration(TimeSpan.FromMilliseconds(250)), 1, 0);
            this.IntoSubModuleAnimation((tSender, tArgs) => {
                this.currentStaging = "settings";
            });
            ((LHSettingsPage)this.Frame_Settings.Content).WhenInto();
        }

        private void LoadSubTitle(string subTitle)
        {
            string sourcePath = "Rclick_SubTitle_" + subTitle + ".png";
            BitmapImage rotating;
            if (this.cachingButtons.ContainsKey(sourcePath))
            {
                rotating = this.cachingButtons[sourcePath];
            }
            else
            {
                rotating = resMana.GetPicture(sourcePath, ResourceManager.FullImageRect).SpriteBitmapImage;
                this.cachingButtons[sourcePath] = rotating;
            }
            ((Image)this.SubTitleSprite.DisplayBinding).Source = rotating;
            this.SubTitleSprite.Descriptor.ToOpacity = 1;
            SpriteAnimation.OpacityToAnimation(this.SubTitleSprite, new Duration(TimeSpan.FromMilliseconds(200)), 1, 0);
        }

        private ImageBrush BacklogBoxImageBrush = null;

        private void LoadBacklogItems()
        {
            if (this.BacklogBoxImageBrush == null)
            {
                this.BacklogBoxImageBrush = new ImageBrush(resMana.GetPicture("Rclick_Backlog_Box.png", ResourceManager.FullImageRect).SpriteBitmapImage);
            }
            var logItems = Director.RunMana.Backlogs.GetLogItems();
            var iterItem = logItems.Last;
            while (iterItem != null)
            {
                var insertCanvas = new Canvas
                {
                    Width = 1018,
                    Height = 178,
                    Opacity = 1,
                    Background = this.BacklogBoxImageBrush,
                };
                var insertNameBlock = new TextBlock
                {
                    Width = 950,
                    Height = 50,
                    Opacity = 1,
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = new SolidColorBrush(Colors.Black),
                    FontSize = 30,
                    FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "Resources/#Source Han Serif CN SemiBold"),
                    Text = iterItem.Value.CharacterName
                };
                Canvas.SetLeft(insertNameBlock, 8);
                Canvas.SetTop(insertNameBlock, 8);
                insertCanvas.Children.Add(insertNameBlock);
                var insertMsgBlock = new TextBlock
                {
                    Width = 950,
                    Height = 150,
                    Opacity = 1,
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = new SolidColorBrush(Colors.Black),
                    FontSize = 24,
                    FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "Resources/#Source Han Serif CN SemiBold"),
                    Text = iterItem.Value.Dialogue
                };
                Canvas.SetLeft(insertMsgBlock, 50);
                Canvas.SetTop(insertMsgBlock, 50);
                insertCanvas.Children.Add(insertMsgBlock);
                iterItem = iterItem.Previous;
                this.StackPanel_Backlog_MainStack.Children.Insert(0, insertCanvas);
            }
        }
        
        private void Image_Button_Mask_MouseEnter(object sender, MouseEventArgs e)
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

        private void Image_Button_Mask_MouseLeave(object sender, MouseEventArgs e)
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

        private void Image_Button_Yes_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.IsFromReturnTitle = true;
            this.Page_PreviewMouseRightButtonUp(null, null);
        }

        private void Image_Button_No_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.CanvasGroup_EnsureMask.Visibility = Visibility.Hidden;
        }
    }
}
