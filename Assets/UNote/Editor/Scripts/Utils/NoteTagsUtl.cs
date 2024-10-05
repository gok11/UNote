using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UNote.Editor
{
    public static class NoteTagsUtl
    {
        private static Dictionary<NoteTags, string> tagIdDict;
        private static Dictionary<NoteTags, Color> tagColorDict;
        
        /// <summary>
        /// All tags except None and All
        /// </summary>
        internal static readonly NoteTags[] ValidTags = Enum.GetValues(typeof(NoteTags))
            .Cast<NoteTags>()
            .Where(t => t != NoteTags.None && t != NoteTags.All)
            .ToArray();
        
        /// <summary>
        /// Tag to UNoteTagData.TagId
        /// </summary>
        public static string ToNoteId(this NoteTags tags)
        {
            if (tagIdDict != null)
            {
                return tagIdDict[tags];
            }

            tagIdDict = new();

            List<UNoteTagData> tagList = UNoteSetting.TagList;
            
            foreach (NoteTags tag in ValidTags)
            {
                UNoteTagData tagData = tagList.Find(t => t.TagName == tag.ToString());
                if (tagData == null)
                {
                    continue;
                }
                
                tagIdDict.Add(tag, tagData.TagId);
            }

            return tagIdDict[tags];
        }
        
        /// <summary>
        /// Tag to UNoteTagData.Color
        /// </summary>
        public static Color ToColor(this NoteTags tags)
        {
            if (tagColorDict != null)
            {
                return tagColorDict[tags];
            }

            tagColorDict = new();

            List<UNoteTagData> tagList = UNoteSetting.TagList;
            
            foreach (NoteTags tag in ValidTags)
            {
                UNoteTagData tagData = tagList.Find(t => t.TagName == tag.ToString());
                if (tagData == null)
                {
                    continue;
                }
                
                tagColorDict.Add(tag, tagData.Color);
            }

            return tagColorDict[tags];
        }
    }
}
