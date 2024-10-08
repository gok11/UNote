using System;
using System.Buffers;
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
        private const string SplitKey = "[unatt-";
        private const string ObjectKey = "guid:";
        private const string GameCameraKey = "game-camera:";
        private const string SceneCameraKey = "scene-camera:";
        private const string PositionKey = "pos:";
        private const string ColorKey = "color:";
        
        /// <summary>
        /// Parse message into elements
        /// </summary>
        internal static void ParseToElements(NoteMessageBase message, VisualElement parentElem)
        {
            ReadOnlySpan<char> textSpan = message.NoteContent.AsSpan();
            ReadOnlySpan<char> splitKeySpan = SplitKey.AsSpan();
            ReadOnlySpan<char> objectKeySpan = ObjectKey.AsSpan();
            ReadOnlySpan<char> gameCameraKeySpan = GameCameraKey.AsSpan();
            ReadOnlySpan<char> sceneCameraKeySpan = SceneCameraKey.AsSpan();
            ReadOnlySpan<char> positionKeySpan = PositionKey.AsSpan();
            ReadOnlySpan<char> colorKeySpan = ColorKey.AsSpan();
            

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
                else if (textSpan.StartsWith(gameCameraKeySpan))
                {
                    ParseCameraText(textSpan, message, parentElem, true);
                }
                else if (textSpan.StartsWith(sceneCameraKeySpan))
                {
                    ParseCameraText(textSpan, message, parentElem, false);
                }
                
                textSpan = textSpan.Slice(textSpan.IndexOf("]") + 1);
            }
        }

        /// <summary>
        /// Parse text contains keywords into ObjectField and Image
        /// </summary>
        private static void ParseObjectText(ReadOnlySpan<char> splitText, NoteMessageBase message, VisualElement parentElem)
        {
            int startIndex = ObjectKey.Length;
            
            int endIndex = splitText.IndexOf(']');
            if (endIndex == -1)
            {
                InsertAsLabel(splitText.ToString(), parentElem);
                return;
            }

            ReadOnlySpan<char> guidSpan = splitText.Slice(startIndex, endIndex - startIndex);
            ReadOnlySpan<char> restSpan = splitText.Slice(endIndex + 1).Trim();
            
            if (guidSpan.IsEmpty || guidSpan.IsWhiteSpace())
            {
                InsertAsLabel(restSpan.ToString(), parentElem);
                return;
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
                            VisualElement editButton = CreateEditScreenshotButton(path);
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
        }

        /// <summary>
        /// Parse text contains keywords into camera info field
        /// </summary>
        private static void ParseCameraText(ReadOnlySpan<char> splitText, NoteMessageBase message,
            VisualElement parentElem, bool isGameView)
        {
            int startIndex = isGameView ? GameCameraKey.Length : SceneCameraKey.Length;

            int endIndex = splitText.IndexOf(']');
            if (endIndex == -1)
            {
                InsertAsLabel(splitText.ToString(), parentElem);
                return;
            }

            ReadOnlySpan<char> camSpan = splitText.Slice(startIndex, endIndex - startIndex);
            ReadOnlySpan<char> restSpan = splitText.Slice(endIndex + 1).Trim();
            
            if (camSpan.IsEmpty || camSpan.IsWhiteSpace())
            {
                InsertAsLabel(restSpan.ToString(), parentElem);
                return;
            }
            
            // Camera info field
            // (position), (rotation), (size)
            
            // Parse position
            ReadOnlySpan<char> positionSpan = camSpan.Slice(1, camSpan.IndexOf(')') - 1);
            Vector3? position = ParseSpanIntoVector3(positionSpan);
            if (position == null)
            {
                InsertAsLabel(camSpan.ToString(), parentElem);
                InsertAsLabel(restSpan.ToString(), parentElem);
                return;
            }
            
            // Parse rotation
            camSpan = camSpan.Slice(camSpan.IndexOf(')') + 3);
            ReadOnlySpan<char> rotationSpan = camSpan.Slice(1, camSpan.IndexOf(')') - 1);
            Vector3? rotation = ParseSpanIntoVector3(rotationSpan);
            if (rotation == null)
            {
                InsertAsLabel(camSpan.ToString(), parentElem);
                InsertAsLabel(restSpan.ToString(), parentElem);
                return;
            }

            // Parse size
            camSpan = camSpan.Slice(camSpan.IndexOf(')') + 3);
            ReadOnlySpan<char> sizeSpan = camSpan.Slice(1, camSpan.IndexOf(')') - 1);

            if (!float.TryParse(sizeSpan, out float size))
            {
                InsertAsLabel(camSpan.ToString(), parentElem);
                InsertAsLabel(restSpan.ToString(), parentElem);
                return;
            }
            
            parentElem.Add(CreateCameraInfoField(position.Value, rotation.Value, size, isGameView));
        }

        /// <summary>
        /// Parse span into Vector3
        /// </summary>
        /// <param name="span">"x, y, z"</param>
        /// <returns></returns>
        private static Vector3? ParseSpanIntoVector3(ReadOnlySpan<char> span)
        {
            int end = span.IndexOf(',');
            ReadOnlySpan<char> xSpan = span.Slice(0, end);

            span = span.Slice(end + 1);
            end = span.IndexOf(',');
            ReadOnlySpan<char> ySpan = span.Slice(0, end);

            span = span.Slice(end + 1);
            ReadOnlySpan<char> zSpan = span;

            if (!float.TryParse(xSpan, out float x) ||
                !float.TryParse(ySpan, out float y) ||
                !float.TryParse(zSpan, out float z))
            {
                return null;
            }

            return new Vector3(x, y, z);
        }
        
        /// <summary>
        /// Create ObjetField
        /// </summary>
        private static ObjectField CreateObjectField(Object obj)
        {
            ObjectField objectField = new ObjectField();
            objectField.SetEnabled(false);
            objectField.SetValueWithoutNotify(obj);
            return objectField;
        }


        /// <summary>
        /// Create camera info display field
        /// </summary>
        private static VisualElement CreateCameraInfoField(Vector3 position, Vector3 rotation, float size, bool isGameView)
        {
            VisualElement cameraInfoField = new();

            string cameraLabel = isGameView ? "Game Camera" : "Scene Camera";

            Label label = new Label($"{cameraLabel} Info");
            label.style.fontSize = 11;
            cameraInfoField.Add(label);

            VisualElement line1 = new VisualElement();
            line1.style.flexDirection = FlexDirection.Row;
            line1.style.marginTop = 2;
            cameraInfoField.Add(line1);
            
            VisualElement line2 = new VisualElement();
            line2.style.flexDirection = FlexDirection.Row;
            cameraInfoField.Add(line2);
            
            // Position
            Label positionLabel = new Label($"Position: {position.ToString("0.#")}");
            positionLabel.style.flexGrow = 1;
            positionLabel.style.fontSize = 10;
            positionLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            positionLabel.SetEnabled(false);
            line1.Add(positionLabel);

            // Rotation
            Label rotationLabel = new Label($"Rotation: {rotation.ToString("0.#")}");
            rotationLabel.style.flexGrow = 1;
            rotationLabel.style.fontSize = 10;
            rotationLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            rotationLabel.SetEnabled(false);
            line1.Add(rotationLabel);

            // Size
            Label sizeLabel = new Label($"Size: {size:0.#}");
            sizeLabel.style.flexGrow = 1;
            sizeLabel.style.fontSize = 10;
            sizeLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            sizeLabel.SetEnabled(false);
            line2.Add(sizeLabel);   

            // Apply Button
            Button applyButton = new Button();
            applyButton.text = $"Apply to {cameraLabel}";
            applyButton.style.flexGrow = 1;
            applyButton.style.fontSize = 10;

            applyButton.clicked += () =>
            {
                // Apply camera info to camera
                if (isGameView)
                {
                    Camera cam = Camera.main;
                    if (!cam)
                    {
                        cam = Object.FindObjectOfType<Camera>();
                    }

                    cam.transform.SetPositionAndRotation(position, Quaternion.Euler(rotation));
                    cam.fieldOfView = size;
                }
                else
                {
                    SceneView sceneView = SceneView.lastActiveSceneView;
                    sceneView.LookAt(position, Quaternion.Euler(rotation), size);
                }
            };
            line2.Add(applyButton);

            // Style
            cameraInfoField.style.SetBorderColor(StyleUtil.GrayBorderColor);
            cameraInfoField.style.SetPadding(4);
            cameraInfoField.style.SetBorderWidth(1);
            
            return cameraInfoField;
        }

        /// <summary>
        /// Create FlexibleImageElem
        /// </summary>
        private static VisualElement CreateFlexibleImageElement(Texture2D tex, VisualElement parentElem)
        {
            VisualElement texElem = new FlexibleImageElem(tex, parentElem);
            texElem.style.SetMargin(2, 0, 2, 0);
            return texElem;
        }
        
        /// <summary>
        /// Insert text as Label element
        /// </summary>
        private static void InsertAsLabel(string text, VisualElement parentElem)
        {
            if (!text.IsNullOrWhiteSpace())
            {
                Label label = new Label(text.Trim());
                label.style.SetMargin(2, 0, 2, 0);
                label.style.whiteSpace = WhiteSpace.Normal;
                parentElem.Add(label);   
            }
        }
        
        /// <summary>
        /// Create screenshot edit button
        /// </summary>
        private static VisualElement CreateEditScreenshotButton(string texPath)
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
