using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Yuri.PlatformCore;
using Yuri.PlatformCore.Graphic;
using Yuri.PlatformCore.VM;
using Yuri.Utils;

namespace Yuri.PageView
{
    /// <summary>
    /// LHSaveLoadPage.xaml 的交互逻辑
    /// </summary>
    public partial class LHSaveLoadPage : Page
    {
        private Director core = Director.GetInstance();
        private ILoadablePage parent;

        public string CurrentStaging { get; set; } = "save";
        private readonly Dictionary<string, BitmapImage> cachingButtons = new Dictionary<string, BitmapImage>();

        /// <summary>
        /// 最后指向的文件id
        /// </summary>
        private int lastPointed = -1;

        private int lastPage = 0;
        private int lastCorSlotId = -1;

        private bool IsRequestingReplace = false;

        private int lastUpdateSlot = -1;
        private string lastUpdateSlotTime = "1970-01-01 00:00:00";

        /// <summary>
        /// 文件信息向量
        /// </summary>
        private readonly List<FileInfo> saveList = new List<FileInfo>();
        private readonly List<string> saveDescList = new List<string>();

        private readonly List<string> slotContentList = new List<string>();

        private YuriSprite SaveSlotSprite;

        public LHSaveLoadPage(ILoadablePage parent)
        {
            InitializeComponent();
            this.parent = parent;
            this.SaveSlotSprite = new YuriSprite()
            {
                DisplayBinding = this.CanvasGroup_SaveSlots,
                AnimationElement = this.CanvasGroup_SaveSlots,
                Descriptor = new SpriteDescriptor()
            };
            for (int i = 0; i < GlobalConfigContext.GAME_SAVE_MAX; i++)
            {
                this.saveList.Add(null);
                this.saveDescList.Add(null);
                this.slotContentList.Add(null);
            }
        }

        public void WhenInto()
        {
            this.lastPointed = -1;
            this.lastPage = 0;
            this.IsRequestingReplace = false;
            this.ReLoadFileInfo();
            this.RefreshNavigationForLastUpdated();
            this.RefreshPaginator();
            this.RefreshSlots();
        }

        public bool WhenLeave()
        {
            return this.intentToEnsurePtr == -1;
        }

        public void RefreshSlots()
        {
            // refresh draw
            int beginIdx = this.lastPage * 10;
            this.TextBlock_SaveSlot_1.Text = this.slotContentList[beginIdx];
            this.TextBlock_SaveSlot_2.Text = this.slotContentList[beginIdx + 1];
            this.TextBlock_SaveSlot_3.Text = this.slotContentList[beginIdx + 2];
            this.TextBlock_SaveSlot_4.Text = this.slotContentList[beginIdx + 3];
            this.TextBlock_SaveSlot_5.Text = this.slotContentList[beginIdx + 4];
            this.TextBlock_SaveSlot_6.Text = this.slotContentList[beginIdx + 5];
            this.TextBlock_SaveSlot_7.Text = this.slotContentList[beginIdx + 6];
            this.TextBlock_SaveSlot_8.Text = this.slotContentList[beginIdx + 7];
            this.TextBlock_SaveSlot_9.Text = this.slotContentList[beginIdx + 8];
            this.TextBlock_SaveSlot_10.Text = this.slotContentList[beginIdx + 9];
            this.TextBlock_SaveSlot_1.Foreground = new SolidColorBrush(Colors.Black);
            this.TextBlock_SaveSlot_2.Foreground = new SolidColorBrush(Colors.Black);
            this.TextBlock_SaveSlot_3.Foreground = new SolidColorBrush(Colors.Black);
            this.TextBlock_SaveSlot_4.Foreground = new SolidColorBrush(Colors.Black);
            this.TextBlock_SaveSlot_5.Foreground = new SolidColorBrush(Colors.Black);
            this.TextBlock_SaveSlot_6.Foreground = new SolidColorBrush(Colors.Black);
            this.TextBlock_SaveSlot_7.Foreground = new SolidColorBrush(Colors.Black);
            this.TextBlock_SaveSlot_8.Foreground = new SolidColorBrush(Colors.Black);
            this.TextBlock_SaveSlot_9.Foreground = new SolidColorBrush(Colors.Black);
            this.TextBlock_SaveSlot_10.Foreground = new SolidColorBrush(Colors.Black);
            if (this.lastCorSlotId != -1) {
                switch (this.lastCorSlotId)
                {
                    case 0:
                        this.TextBlock_SaveSlot_1.Foreground = new SolidColorBrush(Colors.CornflowerBlue);
                        break;
                    case 1:
                        this.TextBlock_SaveSlot_2.Foreground = new SolidColorBrush(Colors.CornflowerBlue);
                        break;
                    case 2:
                        this.TextBlock_SaveSlot_3.Foreground = new SolidColorBrush(Colors.CornflowerBlue);
                        break;
                    case 3:
                        this.TextBlock_SaveSlot_4.Foreground = new SolidColorBrush(Colors.CornflowerBlue);
                        break;
                    case 4:
                        this.TextBlock_SaveSlot_5.Foreground = new SolidColorBrush(Colors.CornflowerBlue);
                        break;
                    case 5:
                        this.TextBlock_SaveSlot_6.Foreground = new SolidColorBrush(Colors.CornflowerBlue);
                        break;
                    case 6:
                        this.TextBlock_SaveSlot_7.Foreground = new SolidColorBrush(Colors.CornflowerBlue);
                        break;
                    case 7:
                        this.TextBlock_SaveSlot_8.Foreground = new SolidColorBrush(Colors.CornflowerBlue);
                        break;
                    case 8:
                        this.TextBlock_SaveSlot_9.Foreground = new SolidColorBrush(Colors.CornflowerBlue);
                        break;
                    case 9:
                        this.TextBlock_SaveSlot_10.Foreground = new SolidColorBrush(Colors.CornflowerBlue);
                        break;
                }
            }
        }
        
        public void RefreshNavigationForLastUpdated()
        {
            if (this.lastUpdateSlot != -1)
            {
                this.lastPage = this.lastUpdateSlot / 10;
                this.lastCorSlotId = this.lastUpdateSlot % 10;
            }
        }

        /// <summary>
        /// 加载保存文件的信息
        /// </summary>
        public void ReLoadFileInfo()
        {
            for (int i = 0; i < GlobalConfigContext.GAME_SAVE_MAX; i++)
            {
                this.saveList[i] = null;
                this.saveDescList[i] = null;
                this.slotContentList[i] = null;
            }
            var saveDirPathStr = IOUtils.ParseURItoURL(GlobalConfigContext.GAME_SAVE_DIR);
            if (!Directory.Exists(saveDirPathStr))
            {
                Directory.CreateDirectory(saveDirPathStr);
            }
            DirectoryInfo dirInfo = new DirectoryInfo(saveDirPathStr);
            foreach (var fInfo in dirInfo.GetFiles())
            {
                if (fInfo.Name.StartsWith(GlobalConfigContext.GAME_SAVE_PREFIX + "-") &&
                    String.Compare(fInfo.Extension, GlobalConfigContext.GAME_SAVE_POSTFIX, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    var timeItems = fInfo.Name.Split('-');
                    var pointedId = Convert.ToInt32(timeItems[1]) - 1;
                    this.saveList[pointedId] = fInfo;

                    var descName = GlobalConfigContext.GAME_SAVE_DIR + @"\" + GlobalConfigContext.GAME_SAVE_DESCRIPTOR_PREFIX +
                       saveList[pointedId].Name.Substring(GlobalConfigContext.GAME_SAVE_PREFIX.Length).Replace(GlobalConfigContext.GAME_SAVE_POSTFIX,
                       GlobalConfigContext.GAME_SAVE_DESCRIPTOR_POSTFIX);
                    var fullDescName = IOUtils.ParseURItoURL(descName);
                    FileStream fs = new FileStream(fullDescName, FileMode.Open);
                    StreamReader sr = new StreamReader(fs);
                    this.saveDescList[pointedId] = sr.ReadToEnd();
                    sr.Close();
                    fs.Close();
                    var descCtx = this.saveDescList[pointedId];
                    if (descCtx != null)
                    {
                        var splitedCtx = descCtx.Split('\n');
                        var trimedDt = splitedCtx[0].Trim();
                        this.slotContentList[pointedId] = String.Format("#{0:D2}   {1}   {2} {3}",
                            pointedId + 1, trimedDt.Substring(5), splitedCtx[1].Trim().Replace("Episode ", "Ep."), splitedCtx[2].Trim());
                        if (String.Compare(this.lastUpdateSlotTime, trimedDt) < 0)
                        {
                            this.lastUpdateSlotTime = trimedDt;
                            this.lastUpdateSlot = pointedId;
                        }
                    }
                }
            }
        }

        private void TextBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            if (this.IsRequestingReplace)
            {
                return;
            }
            int refreshPtr;
            if (sender is TextBlock)
            {
                refreshPtr = Convert.ToInt32(((TextBlock)sender).Name.Split('_').Last()) - 1;
                var sourcePath = ((ImageBrush)((TextBlock)sender).Background).ImageSource.ToString();
                if (!sourcePath.Contains("_Select"))
                {
                    sourcePath = sourcePath.Replace(".png", "_Select.png");
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
                ((ImageBrush)((TextBlock)sender).Background).ImageSource = rotating;
                this.lastPointed = refreshPtr + this.lastPage * 10;
            }
            else
            {
                refreshPtr = (int)sender;
            }
            var descCtx = this.saveDescList[this.lastPointed];
            if (descCtx != null)
            {
                var splitedCtx = descCtx.Split('\n');
                this.Label_SaveInfo_Time.Content = splitedCtx[0].Trim();
                this.Label_SaveInfo_Episode.Content = splitedCtx[1].Trim();
                this.Label_SaveInfo_EpisodeName.Content = splitedCtx[2].Trim();
                this.Label_SaveInfo_Hint.Content = splitedCtx[3].Trim();

                var shotName = GlobalConfigContext.GAME_SAVE_DIR + @"\" + GlobalConfigContext.GAME_SAVE_SNAPSHOT_PREFIX +
                    saveList[this.lastPointed].Name.Substring(GlobalConfigContext.GAME_SAVE_PREFIX.Length).Replace(GlobalConfigContext.GAME_SAVE_POSTFIX,
                    GlobalConfigContext.GAME_SAVE_SNAPSHOT_POSTFIX);
                var fullShotName = IOUtils.ParseURItoURL(shotName);
                if (File.Exists(fullShotName))
                {
                    var p = ResourceManager.GetInstance().GetSaveSnapshot(shotName);
                    this.Image_Snapshot.Source = p.SpriteBitmapImage;
                }
                else
                {
                    this.Image_Snapshot.Source = null;
                }
            } else
            {
                this.Image_Snapshot.Source = null;
                this.Label_SaveInfo_Time.Content = String.Empty;
                this.Label_SaveInfo_Episode.Content = String.Empty;
                this.Label_SaveInfo_EpisodeName.Content = String.Empty;
                this.Label_SaveInfo_Hint.Content = String.Empty;
            }
        }

        private void TextBlock_MouseLeave(object sender, MouseEventArgs e)
        {
            if (this.IsRequestingReplace)
            {
                return;
            }
            this.lastPointed = -1;
            var sourcePath = ((ImageBrush)((TextBlock)sender).Background).ImageSource.ToString().Replace("_Select.png", ".png");
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
            ((ImageBrush)((TextBlock)sender).Background).ImageSource = rotating;
        }

        private void ActualSave(int savePtr)
        {
            // 获得存档时间戳 
            var nowTs = DateTime.Now;
            var timeStamp = nowTs.ToString("yyyy-MM-dd-HH-mm-ss-fff");
            var timeItems = timeStamp.Split('-');
            var saveIdentity = String.Format("-{0}-{1}", savePtr + 1, timeStamp);
            var EpisodeHint = SymbolTable.GetInstance()?.GlobalCtxDao?.GlobalSymbolTable?.Fetch("gb_episode_hint")?.ToString();
            var EpisodeNameHint = SymbolTable.GetInstance()?.GlobalCtxDao?.GlobalSymbolTable?.Fetch("gb_episode_name_hint")?.ToString();
            // 不可容忍的错误段
            try
            {
                // 构造存档文件名（不需要后缀，UR的Save方法已经封装了）
                var fname = String.Format("{0}{1}", GlobalConfigContext.GAME_SAVE_PREFIX, saveIdentity);
                // 保存游戏信息
                this.core.GetMainRender().ActualSave(fname);
                // 更新页面的信息
                this.saveList[savePtr] = new FileInfo(IOUtils.ParseURItoURL(GlobalConfigContext.GAME_SAVE_DIR + @"\" + fname + GlobalConfigContext.GAME_SAVE_POSTFIX));
                this.slotContentList[savePtr] = String.Format("#{0:D2}   {1}   {2} {3}",
                    savePtr + 1, nowTs.ToString("MM-dd HH:mm:ss"), EpisodeHint.Replace("Episode ", "Ep."), EpisodeNameHint);
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存文件失败！在SLPage(in CLR)触发了：" + Environment.NewLine + ex);
                return;
            }
            // 可容忍的错误段
            try
            {
                // 保存截图文件
                if (File.Exists(IOUtils.ParseURItoURL(GlobalConfigContext.GAME_SAVE_DIR + "\\tempSnapshot.jpg")))
                {
                    File.Copy(IOUtils.ParseURItoURL(GlobalConfigContext.GAME_SAVE_DIR + "\\tempSnapshot.jpg"),
                        IOUtils.ParseURItoURL(String.Format("{0}\\{1}{2}{3}", GlobalConfigContext.GAME_SAVE_DIR,
                        GlobalConfigContext.GAME_SAVE_SNAPSHOT_PREFIX, saveIdentity, GlobalConfigContext.GAME_SAVE_SNAPSHOT_POSTFIX)));
                }
                // 保存描述子
                var descFname = String.Format("{0}\\{1}{2}{3}", GlobalConfigContext.GAME_SAVE_DIR,
                    GlobalConfigContext.GAME_SAVE_DESCRIPTOR_PREFIX, saveIdentity, GlobalConfigContext.GAME_SAVE_DESCRIPTOR_POSTFIX);
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(nowTs.ToString("yyyy-MM-dd HH:mm:ss"));
                sb.AppendLine(EpisodeHint ?? String.Empty);
                sb.AppendLine(EpisodeNameHint ?? String.Empty);
                string hintStr = "";
                if (this.core.GetMainRender().IsBranching)
                {
                    try
                    {
                        var lastDialogue = Director.RunMana.Backlogs.GetLast().Dialogue;
                        if (lastDialogue.Length >= 28)
                        {
                            lastDialogue = lastDialogue.Substring(0, 26) + "...";
                        }
                        else
                        {
                            hintStr = lastDialogue;
                        }
                    }
                    catch (Exception ee)
                    {
                        hintStr = "[选择项]";
                    }
                }
                else
                {
                    var lastDialogue = Director.RunMana.Backlogs.GetLast().Dialogue;
                    if (lastDialogue.Length >= 28)
                    {
                        hintStr = lastDialogue.Substring(0, 26) + "...";
                    }
                    else
                    {
                        hintStr = lastDialogue;
                    }
                }
                sb.AppendLine(hintStr.Replace("\n", " ").Replace("\r", ""));
                sb.AppendLine();
                var writeSb = sb.ToString();
                FileStream fs = new FileStream(IOUtils.ParseURItoURL(descFname), FileMode.Create);
                StreamWriter sw = new StreamWriter(fs);
                sw.Write(writeSb);
                sw.Close();
                fs.Close();
                this.saveDescList[savePtr] = writeSb;
            }
            catch (Exception ex)
            {
                LogUtils.LogLine("保存存档的辅助文件出现异常" + Environment.NewLine + ex, "SLPage", LogLevel.Warning);
            }
            // 保存完毕强制刷新页面
            this.lastUpdateSlot = savePtr;
            this.lastUpdateSlotTime = nowTs.ToString("yyyy-MM-dd HH:mm:ss");
            this.RefreshNavigationForLastUpdated();
            this.RefreshSlots();
            this.TextBlock_MouseEnter(savePtr, null);
        }

        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            int copiedPointer;
            lock (this)
            {
                if (this.lastPointed == -1)
                {
                    return;
                }
                else
                {
                    copiedPointer = this.lastPointed;
                }
            }
            // 存档
            if (this.CurrentStaging == "save")
            {
                // 是否覆盖
                if (this.saveList[copiedPointer] != null)
                {
                    this.IsRequestingReplace = true;
                    this.intentToEnsurePtr = copiedPointer;
                    this.CanvasGroup_EnsureMask.Visibility = Visibility.Visible;
                    return;
                }
                else
                {
                    this.ActualSave(copiedPointer);
                }
            }
            // 读档
            else
            {
                // 读取文件
                try
                {
                    if (this.saveDescList[this.lastPointed] != null)
                    {
                        this.core.GetMainRender().ActualLoad(this.saveList[this.lastPointed].Name.Replace(GlobalConfigContext.GAME_SAVE_POSTFIX, String.Empty));
                        // 返回主舞台
                        this.parent.HandleResumeFromLoad();
                    }
                }
                catch (Exception ex)
                {
                    var exStr = "读取存档文件失败，存档是损坏的？" + Environment.NewLine + ex;
                    LogUtils.LogLine(exStr, "SLPage", LogLevel.Error);
                    MessageBox.Show(exStr);
                }
            }
        }

        private void TextBlock_PageItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.lastPage = Convert.ToInt32(((TextBlock)sender).Name.Split('_').Last()) - 1;
            this.RefreshPaginator();
            this.RefreshSlots();
        }

        private void RefreshPaginator()
        {
            foreach (var c in this.CanvasGroup_Paginator.Children)
            {
                if (c is TextBlock)
                {
                    if (((TextBlock)c).Name.EndsWith("_" + (this.lastPage + 1)))
                    {
                        ((TextBlock)c).Opacity = 1;
                        ((TextBlock)c).Foreground = new SolidColorBrush(Colors.CornflowerBlue);
                    }
                    else { 
                        ((TextBlock)c).Opacity = 0.75;
                        ((TextBlock)c).Foreground = new SolidColorBrush(Colors.Black);
                    }
                }
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

        private int intentToEnsurePtr = -1;

        private void Image_Button_Yes_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.CurrentStaging == "save")
            {
                try
                {
                    if (this.intentToEnsurePtr == -1)
                    {
                        return;
                    }
                    // 移除原来的文件
                    try
                    {
                        // 处理过时存档
                        File.Delete(this.saveList[this.intentToEnsurePtr].FullName);
                        // 处理过时描述子
                        var descName = GlobalConfigContext.GAME_SAVE_DIR + @"\" + GlobalConfigContext.GAME_SAVE_DESCRIPTOR_PREFIX +
                            saveList[this.intentToEnsurePtr].Name.Substring(GlobalConfigContext.GAME_SAVE_PREFIX.Length).Replace(GlobalConfigContext.GAME_SAVE_POSTFIX,
                            GlobalConfigContext.GAME_SAVE_DESCRIPTOR_POSTFIX);
                        File.Delete(IOUtils.ParseURItoURL(descName));
                    }
                    catch (Exception ex)
                    {
                        LogUtils.LogLine("覆盖存档时，在移除过时文件过程出现异常" + Environment.NewLine + ex, "SLPage", LogLevel.Error);
                    }
                    this.ActualSave(this.intentToEnsurePtr);
                }
                finally
                {
                    this.intentToEnsurePtr = -1;
                    this.CanvasGroup_EnsureMask.Visibility = Visibility.Hidden;
                    this.IsRequestingReplace = false;
                }
            }
        }

        private void Image_Button_No_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.intentToEnsurePtr = -1;
            this.CanvasGroup_EnsureMask.Visibility = Visibility.Hidden;
            this.IsRequestingReplace = false;
        }
    }
}
