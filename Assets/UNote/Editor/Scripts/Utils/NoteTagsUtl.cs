using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UNote.Editor
{
    public static class NoteTagsUtl
    {
        private static Dictionary<NoteTags, string> tagIdDict;
        
        public static string ToNoteId(this NoteTags tags)
        {
            if (tagIdDict != null)
            {
                return tagIdDict[tags];
            }

            tagIdDict = new();

            List<UNoteTagData> tagList = UNoteSetting.TagList;
            
            foreach (NoteTags tag in Enum.GetValues(typeof(NoteTags)))
            {
                if (tag == NoteTags.All || tag == NoteTags.None)
                {
                    continue;
                }

                UNoteTagData tagData = tagList.Find(t => t.TagName == tag.ToString());
                if (tagData == null)
                {
                    continue;
                }
                
                tagIdDict.Add(tag, tagData.TagId);
            }

            return tagIdDict[tags];
        }
    }
}
