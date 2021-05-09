using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
//using Yuri.ExtraContext;
using Yuri.PlatformCore;
using Yuri.PlatformCore.Audio;
using Yuri.PlatformCore.Graphic;
using Yuri.PlatformCore.VM;
using Yuri.Utils;
using Yuri.Yuriri;

namespace Yuri.PageView
{
    /// <summary>
    /// LongStoryPage.xaml 的交互逻辑
    /// </summary>
    public partial class LongStoryPage : Page, YuriPage, RenderablePage
    {
        private static readonly ResourceManager resMana = ResourceManager.GetInstance();
        private static YuriSprite DecorateSprite;
        private static YuriSprite DialogueSprite;
        private static YuriSprite MainFrameSprite;
        private static YuriSprite BgTransitorSprite;
        private static Queue<Queue<string>> PendingMessageQueue = new Queue<Queue<string>>();
        private static Stack<Queue<string>> BacklogStack = new Stack<Queue<string>>();
        private static Queue<string> IntentBacklog = null;
        private static readonly Dictionary<string, Color> CharacterColorDict = new Dictionary<string, Color>();
        private static Queue<string> CurrentMessageQueue = null;
        private static Storyboard MsgStoryboard = null;
        private static string ChosenDescriptor = String.Empty;
        private static string CallbackTarget = String.Empty;
        private bool IsStoryBranching = false;
        public static bool IsStoryBooking = false;

        private LHStartPage parent;
        
        public LongStoryPage(LHStartPage parent)
        {
            InitializeComponent();
            this.parent = parent;
            this.Image_StoryBook_Decorate.Source = resMana.GetPicture("Bonus_Mask.png", ResourceManager.FullImageRect).SpriteBitmapImage;
            this.Image_StoryBook_Decorate.Opacity = 0;
            DialogueSprite = new YuriSprite()
            {
                DisplayBinding = this.StackPanel_DialogList,
                AnimationElement = this.StackPanel_DialogList,
                Descriptor = new SpriteDescriptor()
            };
            DecorateSprite = new YuriSprite()
            {
                DisplayBinding = this.Image_StoryBook_Decorate,
                AnimationElement = this.Image_StoryBook_Decorate,
                Descriptor = new SpriteDescriptor()
            };
            BgTransitorSprite = new YuriSprite()
            {
                DisplayBinding = this.Image_StoryBook_BgTransitor,
                AnimationElement = this.Image_StoryBook_BgTransitor,
                Descriptor = new SpriteDescriptor()
            };
            //MainFrameSprite = new YuriSprite()
            //{
            //    DisplayBinding = ViewManager.mWnd.mainFrame,
            //    AnimationElement = ViewManager.mWnd.mainFrame,
            //    Descriptor = new SpriteDescriptor()
            //};
            this.RefreshCharacterColorDict();
            // 初始化回调标志位
            Director.RunMana.Symbols.GlobalCtxDao.GlobalSymbolTable.Assign("tracing_callback", String.Empty);
            Director.RunMana.Symbols.GlobalCtxDao.GlobalSymbolTable.Assign("tracing_idx", 0);
        }

        private void RefreshCharacterColorDict()
        {
            CharacterColorDict[@"红"] = Colors.PaleVioletRed;
            CharacterColorDict[@"蓝"] = Colors.DeepSkyBlue;
        }

        /// <summary>
        /// 在指定的文字层绑定控件上进行打字动画
        /// </summary>
        /// <param name="orgString">原字符串</param>
        /// <param name="appendString">要追加的字符串</param>
        /// <param name="msglayBinding">文字层的控件</param>
        /// <param name="wordTimeSpan">字符之间的打字时间间隔</param>
        private void TypeWriter(string orgString, string appendString, TextBlock msglayBinding, int wordTimeSpan)
        { 
            //this.HideMessageTria();
            Storyboard MsgLayerTypingStory = new Storyboard();
            StringAnimationUsingKeyFrames stringAnimationUsingKeyFrames = new StringAnimationUsingKeyFrames();
            Duration aniDuration = new Duration(TimeSpan.FromMilliseconds(wordTimeSpan * appendString.Length));
            stringAnimationUsingKeyFrames.Duration = aniDuration;
            MsgLayerTypingStory.Duration = aniDuration;
            string tmp = orgString;
            foreach (char c in appendString)
            {
                var discreteStringKeyFrame = new DiscreteStringKeyFrame();
                discreteStringKeyFrame.KeyTime = KeyTime.Paced;
                tmp += c;
                discreteStringKeyFrame.Value = tmp;
                stringAnimationUsingKeyFrames.KeyFrames.Add(discreteStringKeyFrame);
            }
            Storyboard.SetTarget(stringAnimationUsingKeyFrames, msglayBinding);
            Storyboard.SetTargetProperty(stringAnimationUsingKeyFrames, new PropertyPath(TextBlock.TextProperty));
            MsgLayerTypingStory.Children.Add(stringAnimationUsingKeyFrames);
            MsgLayerTypingStory.Completed += this.TypeWriterAnimationCompletedCallback;
            MsgLayerTypingStory.Begin();
            MsgStoryboard = MsgLayerTypingStory;
        }

        private void TypeWriterAnimationCompletedCallback(object sender, EventArgs e)
        {
            //if (Math.Abs(MsgStoryboard.GetCurrentProgress() - 1.0) < 0.01)
            //{
            //this.ShowMessageTria();
            //this.BeginMessageTriaUpDownAnimation();
            //}
            lock (MsgStoryboard)
            {
                MsgStoryboard = null;
            }
        }

        private void Page_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.StoryBookForwardNextSteady();
        }

        public void StoryBookForwardNextSteady()
        {
            // 如果在看背景就先切回故事模式
            if (this.ShowingBackground)
            {
                DecorateSprite.Descriptor.ToOpacity = 1;
                DialogueSprite.Descriptor.ToOpacity = 1;
                SpriteAnimation.OpacityToAnimation(DecorateSprite, TimeSpan.FromMilliseconds(100), 1);
                SpriteAnimation.OpacityToAnimation(DialogueSprite, TimeSpan.FromMilliseconds(100), 1);
                //SpriteAnimation.BlurMutexAnimation(MainFrameSprite, TimeSpan.FromMilliseconds(100), 0, 30);
                this.ShowingBackground = false;
                return;
            }
            // 选择项就忽略
            if (this.IsStoryBranching)
            {
                return;
            }
            // 打字动画中就跳过
            if (MsgStoryboard != null)
            {
                lock (MsgStoryboard)
                {
                    if (MsgStoryboard != null)
                    {
                        MsgStoryboard.SkipToFill();
                        return;
                    }
                }
            }
            // 瞄下队列头，是否是空队列，是的话说明要换页
            if (CurrentMessageQueue.Count == 0)
            {
                // 全部都结束啦
                if (PendingMessageQueue.Count == 0)
                {
                    this.PrepareClose();
                    return;
                }
                this.StackPanel_DialogList.Children.Clear();
                CurrentMessageQueue = PendingMessageQueue.Dequeue();
                if (IntentBacklog != null)
                {
                    BacklogStack.Push(IntentBacklog);
                }
                IntentBacklog = ForkableState.DeepCopyBySerialization(CurrentMessageQueue);
            }
            // 取下一条要显示的内容
            var currentRun = CurrentMessageQueue.Dequeue();
            // 选择支：SELECT@分支1@分支2@...
            if (currentRun.StartsWith("SELECT@"))
            {
                var selectionItem = currentRun.Split('@');
                var insertBlock = new TextBlock
                {
                    Opacity = 1,
                    Margin = new Thickness(10, 16, 10, 50),
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = new SolidColorBrush(Colors.White),
                    FontSize = 30,
                    FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "Resources/#Source Han Serif CN SemiBold"),
                    Text = selectionItem[1]
                };
                this.StackPanel_DialogList.Children.Add(insertBlock);
                for (var i = 2; i < selectionItem.Length; i++)
                {
                    var selectionLable = new Label
                    {
                        Background = new SolidColorBrush(Color.FromArgb(70, 0, 0, 0)),
                        Margin = new Thickness(10, 80, 10, 0),
                        Foreground = new SolidColorBrush(Colors.White),
                        Height = 60,
                        FontSize = 30,
                        FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "Resources/#Source Han Serif CN SemiBold"),
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center,
                        Content = selectionItem[i],
                        Tag = i.ToString()
                    };
                    selectionLable.MouseEnter += this.Selection_OnMouseEnter;
                    selectionLable.MouseLeave += this.Selection_OnMouseLeave;
                    selectionLable.MouseLeftButtonUp += this.Selection_OnMouseLeftButtonUp;
                    this.StackPanel_DialogList.Children.Add(selectionLable);
                }
                this.IsStoryBranching = true;
            }
            // 背景：BG@变换时间@图片资源名
            else if (currentRun.StartsWith("BG@"))
            {
                var selectionItem = currentRun.Split('@');
                var bgTransformTime = Convert.ToInt32(selectionItem[1]);
                var bgResourceName = selectionItem[2];
                if (bgResourceName != String.Empty)
                {
                    // backlay
                    if (this.Image_StoryBook_BgTransitor.Source == null)
                    {
                        this.Grid_StoryBook.Background = new SolidColorBrush(Colors.Transparent);
                    }
                    else
                    {
                        this.Grid_StoryBook.Background = new ImageBrush(this.Image_StoryBook_BgTransitor.Source);
                    }
                    BgTransitorSprite.Descriptor.Opacity = 0;
                    BgTransitorSprite.DisplayBinding.Opacity = 0;
                    this.Image_StoryBook_BgTransitor.Source = resMana.GetPicture(bgResourceName, ResourceManager.FullImageRect).SpriteBitmapImage;
                    BgTransitorSprite.Descriptor.ToOpacity = 1;
                    SpriteAnimation.OpacityToAnimation(BgTransitorSprite, TimeSpan.FromMilliseconds(bgTransformTime), 1);
                }
                else
                {
                    this.Grid_StoryBook.Background = new SolidColorBrush(Colors.Transparent);
                    BgTransitorSprite.Descriptor.ToOpacity = 0;
                    SpriteAnimation.OpacityToAnimation(BgTransitorSprite, TimeSpan.FromMilliseconds(bgTransformTime), 0);
                }
                this.StoryBookForwardNextSteady();
            }
            // 声效：SE@音量@SE资源名
            else if (currentRun.StartsWith("SE@"))
            {
                var selectionItem = currentRun.Split('@');
                var volume = Convert.ToDouble(selectionItem[1]);
                Musician.GetInstance().PlaySE(resMana.GetSE(selectionItem[2]), (float)volume);
                this.StoryBookForwardNextSteady();
            }
            else
            {
                var dialogItem = currentRun.Split('@');
                var fgColor = Colors.White;
                if (dialogItem.Length > 1)
                {
                    if (CharacterColorDict.ContainsKey(dialogItem[0]))
                    {
                        fgColor = CharacterColorDict[dialogItem[0]];
                    }

                    currentRun = dialogItem[1];
                }
                var insertBlock = new TextBlock
                {
                    Opacity = 1,
                    Margin = new Thickness(10, 16, 10, 10),
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = new SolidColorBrush(fgColor),
                    FontSize = 30,
                    FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "Resources/#Source Han Serif CN SemiBold"),
                    Text = String.Empty
                };
                // 把前一条内容透明化
                if (this.StackPanel_DialogList.Children.Count > 0)
                {
                    var lastMsgBlock = this.StackPanel_DialogList.Children[this.StackPanel_DialogList.Children.Count - 1] as TextBlock;
                    var OpacingDescriptor = new YuriSprite()
                    {
                        DisplayBinding = lastMsgBlock,
                        AnimationElement = lastMsgBlock,
                        Descriptor = new SpriteDescriptor()
                    };
                    OpacingDescriptor.Descriptor.ToOpacity = 0.4;
                    SpriteAnimation.OpacityToAnimation(OpacingDescriptor, TimeSpan.FromMilliseconds(500), 0.4);
                }
                this.StackPanel_DialogList.Children.Add(insertBlock);
                this.TypeWriter(String.Empty, currentRun, insertBlock, GlobalConfigContext.GAME_MSG_TYPING_DELAY);
            }
        }

        public Queue<Queue<string>> ReloadTraceStory(int idx)
        {
            Queue<Queue<string>> mQ = new Queue<Queue<string>>();
            if (idx == 0)
            {
                Queue<string> page = new Queue<string>();
                page.Enqueue("BG@500@Bonus_Ground.png");
                page.Enqueue("那些曾经年轻的愿望，事到如今去向何方，也许活在当下的我们在到达那未到的未来之前，无论怎样探求都难以得知。");
                page.Enqueue("至于那名为过往的时刻，就算对时至今日的时光造成了多大的影响，也无法改变它是需要挥手作别的过去，这一简单的事实。");
                page.Enqueue("我们就这样，踏着那些终将逝去的过往，继续前行。");
                mQ.Enqueue(page);

                page = new Queue<string>();
                page.Enqueue("是的，改变总是伴随着风险，当时间跨过一个阶段，时针越过一条粗刻，就算并非本人的意愿，周遭也会发生剧变，相伴而行的两人迎来分岔的路口，很快，选择的时刻就将来临……");
                mQ.Enqueue(page);

                page = new Queue<string>();
                page.Enqueue("蓝@『身份证带好了吗？』");
                page.Enqueue("红@『诶？为什么要带身份证？』");
                page.Enqueue("蓝@『你说呢？我昨天才告诉过你。』");
                page.Enqueue("红@『哦哦！我想起来了！』");
                mQ.Enqueue(page);

                page = new Queue<string>();
                page.Enqueue("在经过欣雯长时间指导后，曲璃学习方面的细心程度明明已经提高了不少，但在这种小事儿上，还是免不了粗心大意以及健忘的坏习惯。又或许是因为已经不用再紧绷着神经应对考试，所以才再一次变得粗心了吧？");
                page.Enqueue("总之，在这个高考刚刚结束的、还伴随着密集蝉鸣的六月，欣雯和曲璃，正在房间中做着出门前最后的准备。");
                mQ.Enqueue(page);

                page = new Queue<string>();
                page.Enqueue("蓝@『好看么？』");
                page.Enqueue("欣雯在曲璃面前旋转一圈，脚尖轻点地面，白皙的左肩在黑色长发的衬托下更显光洁。");
                page.Enqueue("红@『好看！』");
                page.Enqueue("曲璃拿上身份证看着曲璃，顺手从挂钩上取下了自己的外套，穿在身上，待欣雯走出房门，她才紧随其后跟出去，将门关上。");
                page.Enqueue("关门前的最后一刻，突然有些犹豫地抬起头，通过门缝看内部。整洁的房间，铺好的床，椅子增加到两把的书桌，平整舒适的沙发，无论是她一个人，还是欣雯与她一起，都在这间房间留下了诸多难以忘怀的回忆。");
                page.Enqueue("在这间七月份租期就迎来尽头的房间中。");
                mQ.Enqueue(page);

                page = new Queue<string>();
                page.Enqueue("蓝@『走吧。』");
                page.Enqueue("红@『好。』");
                page.Enqueue("她关上门，转向等在前面的欣雯。");
                mQ.Enqueue(page);

                page = new Queue<string>();
                page.Enqueue("今天是工作日，暑假也尚未到来，游乐园并没有多少游客，一眼看过去，大多数都是年龄相仿的高三毕业生。");
                page.Enqueue("在仔细确认了身份证、高考准考证和本人样貌后，工作人员把活动特惠票递到两人手中，脸上带着职业的微笑，欢迎两人入园。");
                page.Enqueue("门口的音乐喷泉本应是被众人围观的明星，在这样的时间段却显得尤为孤独。");
                mQ.Enqueue(page);

                page = new Queue<string>();
                page.Enqueue("红@『欣雯！下一个就去过山车吧？』");
                page.Enqueue("蓝@『诶……』");
                page.Enqueue("在游玩了数个项目后，这是欣雯自入园以来第一次犯难。");
                page.Enqueue("明明刚开始的项目都还算温和，现在的曲璃终于再也不掩藏自己爱玩的性格，频频拉着欣雯去玩些刺激的内容，欣雯虽自认为心理承受能力不算差，陪着曲璃玩了这么多，可听到过山车时，双腿还是不由得有些发软。");
                page.Enqueue("逞强答应下来的她，一路上都在努力暗示自己，过山车并不可怕，过山车并不可怕，过山车并不可怕，但临到检票处，还是狠心撒开了曲璃牵着的手，临阵脱逃。");
                mQ.Enqueue(page);

                page = new Queue<string>();
                page.Enqueue("蓝@『那、那个，我还是算了吧！曲璃一个人去就好，我在下面等着！』");
                page.Enqueue("她退后两步，撤出了排队通道。");
                page.Enqueue("红@『诶？为什么啦，一起去嘛，不可怕的。』");
                page.Enqueue("蓝@『不要！』");
                page.Enqueue("检票的工作人员苦笑着旁观两人的纠缠，若是在客人多的时候，她也许会将两人请到一边，避免她们阻塞入口，但今天的顾客实在是少，她有充足的时间观赏发生在眼前的闹剧，甚至有些享受两人此时的滑稽表现。");
                mQ.Enqueue(page);

                page = new Queue<string>();
                page.Enqueue("见数次相劝都没有任何作用，曲璃扭头目测了一下过山车队伍的长度，确认此时的人数几乎不需要排队后，才对欣雯说道：");
                page.Enqueue("红@『那我去玩一次，很快就回来哦？』");
                page.Enqueue("蓝@『嗯。』");
                page.Enqueue("红@『在出口处等我哦？』");
                page.Enqueue("蓝@『安心啦，我又不会跑。』");
                page.Enqueue("红@『好好好～』");
                page.Enqueue("欣雯长舒一口气，看着曲璃像小孩子似的蹦蹦跳跳前往队尾，刚到地方，又开心地回头朝自己招手。她面向曲璃无奈地笑笑，时近正午，阳光有些刺眼。");
                page.Enqueue("蓝@『好了，那我去买点东西吧。』");
                mQ.Enqueue(page);

                page = new Queue<string>();
                page.Enqueue("红@『我回来了！』");
                page.Enqueue("曲璃刚从出口处窜出来，一看到不远处大遮阳伞下等待的欣雯，就快步跑着想要抱过去，丝毫也不顾及身后其他游客的目光。");
                page.Enqueue("蓝@『诶等等！』");
                page.Enqueue("欣雯被扑了个搓手不及，只能高举着手中的东西，避免因为被曲璃撞到造成悲剧。");
                mQ.Enqueue(page);

                page = new Queue<string>();
                page.Enqueue("抬头看一眼，自己正面朝着和曲璃一起从出口出来，那看着两人大白天毫不害臊行为的人群，尴尬地笑笑，埋头轻声劝着曲璃：");
                page.Enqueue("蓝@『好啦好啦，那么多人看着呢！』");
                page.Enqueue("红@『不行——过山车好可怕，先抱一下嘛？』");
                page.Enqueue("蓝@『你也不看看我手里空不空就抱上来……』");
                page.Enqueue("兴许是刚才的阳光太过刺眼没能看清，曲璃抬起头，才意识到自己刚才行为的危险。");
                mQ.Enqueue(page);

                page = new Queue<string>();
                page.Enqueue("欣雯一手拿着一支甜筒冰淇淋，另一只手则是牵着三只气球，要不是欣雯反应快，恐怕不仅自己的衣服会被冰淇淋弄脏，气球也会脱手飞走。");
                page.Enqueue("红@『抱歉！不过……冰淇淋是很正常，欣雯为什么买了气球？』");
                page.Enqueue("蓝@『买冰淇淋送的啦，人家硬塞过来，我又不好意思不要。』");
                page.Enqueue("她说着，把气球的绳子递到曲璃面前。");
                page.Enqueue("曲璃看了看气球的绳尾，从她手里接下，继而走到了她的身旁。");
                page.Enqueue("红@『走吧？公主殿下。』");
                mQ.Enqueue(page);

                page = new Queue<string>();
                page.Enqueue("她抬起自己没有牵着气球的另外一只手，并不是为了牵上欣雯，而是为手臂和身体之间留出空隙。");
                page.Enqueue("蓝@『真拿你没办法。』");
                page.Enqueue("欣雯将甜筒冰淇淋换到了另一边，轻轻挽上了曲璃的手臂，与她一起走出了遮阳伞的阴影。");
                mQ.Enqueue(page);

                page = new Queue<string>();
                page.Enqueue("曲璃哪里像是守护公主的骑士，倒是更像掳走公主的恶龙，带着欣雯挑战这个挑战那个，几乎都是刺激的项目。");
                page.Enqueue("红@『要去坐摩天轮吗？』");
                page.Enqueue("下午都已经过半，她才意识到自己太过得意忘形，一直带着欣雯在玩自己想玩的项目，却没有顾及欣雯想玩什么。");
                page.Enqueue("欣雯从来不对曲璃说想去哪里，一直都默默跟在曲璃身边，这才让曲璃一度忽视了如此重要的内容。");
                mQ.Enqueue(page);

                page = new Queue<string>();
                page.Enqueue("红@『那个的话，晚上坐会更漂亮哦？』");
                page.Enqueue("欣雯回答道。游乐场离市区并不远，如果在晚间登上摩天轮，甚至有机会可以看到城市的夜景。");
                page.Enqueue("这是她唯一在网上查过的内容，至于其他项目，虽然路上也看到了不少自己感兴趣的内容，但是欣雯很少来游乐场，纵使有感兴趣的地方，也并不清楚“好不好玩”，只得一路跟着曲璃。");
                page.Enqueue("她也并不讨厌曲璃这样，倒不如说，这孩子气的活泼一面，也让自己心动，甚至感到心安。");
                page.Enqueue("也许很多事情，都是只有拥有孩童般勇气的人，才能做到的吧？");
                mQ.Enqueue(page);

                page = new Queue<string>();
                page.Enqueue("红@『那么，就由欣雯来选下一个项目吧。』");
                page.Enqueue("曲璃停下脚步，转向欣雯，等待着对方来做选择。");
                page.Enqueue("蓝@『可是……我不知道什么好玩。』");
                page.Enqueue("欣雯环顾一圈，仍然拿不定主意。");
                page.Enqueue("红@『哪有什么好玩不好玩的，只要是想玩的，就一定是好玩的！』");
                page.Enqueue("曲璃的兴致依旧高涨，即使这时候的她还不知道等会儿会被欣雯带上旋转木马这件事。");
                mQ.Enqueue(page);

                page = new Queue<string>();
                page.Enqueue("蓝@『那么，曲璃陪我一起吗？』");
                page.Enqueue("欣雯滑下了自己挽着她的手，将手掌与她的手掌重叠，手指自然地扣进指缝，将对方的手按入自己的掌中。");                
                page.Enqueue("在这如今，高考已经结束的六月，气温微热，掌心也难免有些汗水，但欣雯的紧张并不来源于初夏的燥热。");
                page.Enqueue("她知道，等曲璃家人替她为学业租的那间房子租期到底时，两人专属的容身之所就会消失，现在的她们，正站在早已意识到，却始终不愿提及的分岔口。");
                page.Enqueue("成绩尚未发布，大学的事情，没有谁能保证，两人会去向何方，会在同一所学校吗？退而求其次，会在同一座城市吗？");
                mQ.Enqueue(page);

                page = new Queue<string>();
                page.Enqueue("能够继续有机会住在一处吗？如果不行的话，多久才能见面一次？虽然自己已经把曲璃的成绩抬高到了不错的位置，但最后的成绩，真的如同之前设想的那样吗？");
                page.Enqueue("她总是尽量克制自己不去想，因为一切都是最好的安排，只要顺其自然就好，原本也应该这样。");
                page.Enqueue("但手中的温度，在将其握于手中的此刻，又无论如何都不愿意任其溜走，不愿意轻易放开。");
                page.Enqueue("上次率先做出改变的，率先拿出勇气的，不也是曲璃吗？");
                page.Enqueue("让人不免会期待未来，这就是她面前的爱人。");
                mQ.Enqueue(page);

                page = new Queue<string>();
                page.Enqueue("事实证明，思考有时候，确实是一项多余的脑力活动。");
                page.Enqueue("特别是在自己尚未到达的阶段，面对完全未知的未来，那些犹豫和担忧纯粹是坐井观天。");
                page.Enqueue("在必须要迎来变化的如今，是否还能依赖感性的主导，将风险背负，率先做出改变呢？是否能不顾可能产生的后悔，一往无前呢？");
                page.Enqueue("悔恨过去和担忧未来，只需要一个就够了。");
                mQ.Enqueue(page);

                page = new Queue<string>();
                page.Enqueue("红@『嗯，我陪你。』");
                page.Enqueue("曲璃微笑着回答，她当然没有傻到察觉不到对方握紧的手的意义，就算她不懂得与人交流，不懂得察言观色，就算她读不懂其他人的任何深意，但她一定懂得欣雯：");
                page.Enqueue("红@『无论是哪里，我都陪你去。』");
                mQ.Enqueue(page);
            }
            return mQ;
        }

        public void PrepareClose()
        {
            lock (this)
            {
                DecorateSprite.Descriptor.ToOpacity = 0;
                BgTransitorSprite.Descriptor.ToOpacity = 0;
                SpriteAnimation.OpacityToAnimation(DecorateSprite, TimeSpan.FromMilliseconds(500), 0);
                SpriteAnimation.OpacityToAnimation(BgTransitorSprite, TimeSpan.FromMilliseconds(500), 0);
                //SpriteAnimation.BlurMutexAnimation(MainFrameSprite, TimeSpan.FromMilliseconds(500), 30, 0);

                this.parent.onBonusExit();
            }
            //LongStoryPage.IsStoryBooking = false;
            //PersistContextDAO.Assign("Callback_tracing_chosen", ChosenDescriptor);
            //if (CallbackTarget != String.Empty)
            //{
            //    var callbackNtr = new Interrupt()
            //    {
            //        Detail = "StoryBookCallbackNTR",
            //        InterruptSA = null,
            //        Type = InterruptType.ButtonJump,
            //        ReturnTarget = CallbackTarget,
            //        ExitWait = true
            //    };
            //    // 提交回到中断到主调用堆栈
            //    Director.RunMana.CallStack.Submit(callbackNtr);
            //    // 重置回调
            //    CallbackTarget = String.Empty;
            //    Director.RunMana.Symbols.GlobalCtxDao.GlobalSymbolTable.Assign("tracing_callback", String.Empty);
            //}
            //ViewPageManager.CollapseUIPage();
        }
        
        public void PrepareOpen()
        {
            lock (this)
            {
                this.Image_StoryBook_BgTransitor.Source = null;
                this.Grid_StoryBook.Background = new SolidColorBrush(Colors.Transparent);
                DecorateSprite.Descriptor.ToOpacity = 1;
                SpriteAnimation.OpacityToAnimation(DecorateSprite, TimeSpan.FromMilliseconds(300), 1);
                //SpriteAnimation.BlurMutexAnimation(MainFrameSprite, TimeSpan.FromMilliseconds(300), 0, 30);
                this.ShowingBackground = false;
                this.IsStoryBranching = false;
                this.StackPanel_DialogList.Children.Clear();
                BacklogStack.Clear();
                IntentBacklog = null;
                // 检查回调标志位
                //CallbackTarget = SymbolTable.GetInstance().GlobalCtxDao.GlobalSymbolTable.Fetch("tracing_callback").ToString();
                // 从全局变量挑选故事载入
                //var trace_idx = Convert.ToInt32(SymbolTable.GetInstance().GlobalCtxDao.GlobalSymbolTable.Fetch("tracing_idx"));
                PendingMessageQueue = this.ReloadTraceStory(0);
                if (PendingMessageQueue == null)
                {
                    LogUtils.LogLine("Load tracing story, but NULL", "LongStoryPage", LogLevel.Warning);
                    this.PrepareClose();
                    return;
                }

                LongStoryPage.IsStoryBooking = true;
                CurrentMessageQueue = PendingMessageQueue.Dequeue();

                // 主动调用一次左键
                this.StoryBookForwardNextSteady();
            }
        }

        private bool ShowingBackground = false;

        private void Page_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!this.ShowingBackground)
            {
                DecorateSprite.Descriptor.ToOpacity = 0;
                DialogueSprite.Descriptor.ToOpacity = 0;
                SpriteAnimation.OpacityToAnimation(DecorateSprite, TimeSpan.FromMilliseconds(200), 0);
                SpriteAnimation.OpacityToAnimation(DialogueSprite, TimeSpan.FromMilliseconds(200), 0);
                //SpriteAnimation.BlurMutexAnimation(MainFrameSprite, TimeSpan.FromMilliseconds(200), 30, 0);
            }
            else
            {
                DecorateSprite.Descriptor.ToOpacity = 1;
                DialogueSprite.Descriptor.ToOpacity = 1;
                SpriteAnimation.OpacityToAnimation(DecorateSprite, TimeSpan.FromMilliseconds(100), 1);
                SpriteAnimation.OpacityToAnimation(DialogueSprite, TimeSpan.FromMilliseconds(100), 1);
                //SpriteAnimation.BlurMutexAnimation(MainFrameSprite, TimeSpan.FromMilliseconds(100), 0, 30);
            }
            this.ShowingBackground = !this.ShowingBackground;
        }

        private void Page_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta < 0)
            {
                this.StoryBookForwardNextSteady();
            }
            else if (e.Delta > 0)
            {
                if (BacklogStack.Count > 0)
                {
                    
                }
            }
        }

        private void Selection_OnMouseEnter(object sender, MouseEventArgs e)
        {
            (sender as Label).Foreground = new SolidColorBrush(Colors.PaleVioletRed);
        }

        private void Selection_OnMouseLeave(object sender, MouseEventArgs e)
        {
            (sender as Label).Foreground = new SolidColorBrush(Colors.White);
        }
        
        private void Selection_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.ShowingBackground)
            {
                return;
            }
            ChosenDescriptor += (sender as Label).Tag as string;
            this.IsStoryBranching = false;
            // 再调一次刷新页面
            this.StoryBookForwardNextSteady();
        }

        private void Page_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                this.StoryBookForwardNextSteady();
            }
        }

        public void OnSceneActionDone(SceneAction action)
        {
            // nothing
        }

        public bool PreviewSceneAction(SceneAction action)
        {
            // nothing
            return false;
        }
    }
}
