using System;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UNote.Runtime;
using Object = UnityEngine.Object;

namespace UNote.Editor
{
    internal static class NoteTextParser
    {
        const string SplitKey = "[unatt-";
        const string ObjectKey = "guid:";
        
        internal static void ParseToElements(NoteMessageBase message, VisualElement parentElem)
        {
            ReadOnlySpan<char> textSpan = message.NoteContent.AsSpan();
            ReadOnlySpan<char> splitKeySpan = SplitKey.AsSpan();
            ReadOnlySpan<char> objectKeySpan = ObjectKey.AsSpan();

            while (true)
            {
                int splitKeyIndex = textSpan.IndexOf(splitKeySpan);

                if (splitKeyIndex == -1)
                {
                    InsertAsLabel(textSpan.ToString(), parentElem);
                    break;
                }
                
                // Insert label before SplitKey
                InsertAsLabel(textSpan.Slice(0, splitKeyIndex).ToString(), parentElem);

                // Update textSpan
                textSpan = textSpan.Slice(splitKeyIndex + splitKeySpan.Length);

                if (textSpan.StartsWith(objectKeySpan))
                {
                    ParseObjectText(textSpan, message, parentElem);
                }
                
                textSpan = textSpan.Slice(textSpan.IndexOf("]") + 1);
            }
        }

        private static int ParseObjectText(ReadOnlySpan<char> splitText, NoteMessageBase message, VisualElement parentElem)
        {
            int startIndex = ObjectKey.Length;
            
            int endIndex = splitText.IndexOf("]");
            if (endIndex == -1)
            {
                InsertAsLabel(splitText.ToString(), parentElem);
                return 0;
            }

            ReadOnlySpan<char> guidSpan = splitText.Slice(startIndex, endIndex - startIndex);
            ReadOnlySpan<char> restSpan = splitText.Slice(endIndex + 1).Trim();
            
            if (guidSpan.IsEmpty || guidSpan.IsWhiteSpace())
            {
                InsertAsLabel(splitText.ToString(), parentElem);
                InsertAsLabel(restSpan.ToString(), parentElem);   
                return endIndex;
            }

            // Object field
            string path = AssetDatabase.GUIDToAssetPath(guidSpan.ToString());
            if (!string.IsNullOrWhiteSpace(path))
            {
                Object obj = AssetDatabase.LoadAssetAtPath<Object>(path);

                // Add ObjectField 
                parentElem.Add(CreateObjectField(obj));   
                
                // Draw texture
                if (obj is Texture2D tex)
                {
                    VisualElement texElem = CreateFlexibleImageElement(tex, parentElem);
                    parentElem.Add(texElem);

                    // Add edit button if this is owned screenshot
                    if (message.Author == UNoteSetting.UserName)
                    {
                        DirectoryInfo dir = Directory.GetParent(path);
                        if (dir is { Name: "Screenshots" })
                        {
                            VisualElement editButton = CreateEditButton(path);
                            editButton.visible = false;
                            texElem.Add(editButton);
                    
                            texElem.RegisterCallback<MouseEnterEvent>(_ =>
                            {
                                editButton.visible = true;
                            });
                    
                            texElem.RegisterCallback<MouseLeaveEvent>(_ =>
                            {
                                editButton.visible = false;
                            });      
                        }
                    }
                }
                
                InsertAsLabel(restSpan.ToString(), parentElem);   
            }

            return endIndex;
        }

        private static ObjectField CreateObjectField(Object obj)
        {
            ObjectField objectField = new ObjectField();
            objectField.SetEnabled(false);
            objectField.SetValueWithoutNotify(obj);
            return objectField;
        }

        private static VisualElement CreateFlexibleImageElement(Texture2D tex, VisualElement parentElem)
        {
            VisualElement texElem = new FlexibleImageElem(tex, parentElem);
            texElem.style.SetMargin(2, 0, 2, 0);
            return texElem;
        }
        
        private static void InsertAsLabel(string text, VisualElement parentElem)
        {
            if (!text.IsNullOrWhiteSpace())
            {
                Label label = new Label(text.Trim());
                label.style.SetMargin(2, 0, 2, 0);
                parentElem.Add(label);   
            }
        }
        
        private static VisualElement CreateEditButton(string texPath)
        {
            Button editButton = new Button();
            editButton.name = "EditButton";

            editButton.style.width = editButton.style.height = 20f;
            editButton.style.SetPadding(2);
            editButton.style.alignSelf = Align.FlexEnd;

            VisualElement icon = new VisualElement();
            icon.style.flexGrow = 1;
            icon.style.backgroundImage =
                AssetDatabase.LoadAssetAtPath<Texture2D>(PathUtil.GetTexturePath("brush.png"));
            icon.style.unityBackgroundImageTintColor = StyleUtil.GrayColor;
            editButton.Add(icon);
                            
            editButton.clicked += () =>
            {
                ImageEditWindow.OpenWithTexture(texPath);
            };

            return editButton;
        }
    }
}
