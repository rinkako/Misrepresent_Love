using System;
using System.Collections.Generic;
using System.Linq;

namespace Yuri.PlatformCore
{
    [Serializable]
    internal class BacklogHolder
    {
        private static readonly int MaxBacklogItemSize = 100;

        private readonly LinkedList<BacklogItem> BacklogList = new LinkedList<BacklogItem>();

        public void AddLog(string content)
        {
            while (this.BacklogList.Count >= MaxBacklogItemSize)
            {
                this.BacklogList.RemoveFirst();
            }
            try
            {
                string[] tagItem = content.Split(new string[] { "::" }, StringSplitOptions.None);
                int startIdx = tagItem[0].Length + tagItem[1].Length + 4;
                string cutted = content.Substring(startIdx, content.LastIndexOf('#') - startIdx);
                this.BacklogList.AddLast(new BacklogItem() { CharacterName = tagItem[0].Trim(), Dialogue = cutted.Trim(), VoiceId = tagItem[1].Trim(), IsContinous = content.Last() == '1' });
            }
            catch (Exception e)
            {
                try
                {
                    string cutted = content.Substring(0, content.LastIndexOf('#'));
                    this.BacklogList.AddLast(new BacklogItem() { Dialogue = cutted.Trim(), IsContinous = content.Last() == '1' });
                }
                catch (Exception ie)
                {
                    this.BacklogList.AddLast(new BacklogItem() { Dialogue = content, IsContinous = content.Last() == '1' });
                }
            }
        }

        public LinkedList<BacklogItem> GetLogItems()
        {
            return this.BacklogList;
        }

        public BacklogItem GetLast()
        {
            return this.BacklogList.Last();
        }

        public void Clear()
        {
            this.BacklogList.Clear();
        }

        [Serializable]
        public class BacklogItem
        {
            public string CharacterName { get; set; }

            public string Dialogue { get; set; }

            public string VoiceId { get; set; }

            public bool IsContinous { get; set; } = false;
        }
    }
}
