using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UNote.Editor;

namespace UNote.Editor
{
	/// <summary>
	/// Image editor and exporter
	/// </summary>
	public class ImageEditWindow : EditorWindow
	{
		private Texture2D m_editTargetTex;
		private Texture2D m_canvasBuffer;
		private Texture2D m_paintBuffer;
		private Texture2D m_clearBuffer;

		[SerializeField]
		private int m_texGen;

		private List<Texture2D> m_texCacheList;

		[SerializeField]
		private int m_brushSize = 1;

		[SerializeField]
		private Color m_brushColor = Color.white;

		private (int, int)? m_prevMousePos;

		private bool m_isDirty;
		private bool m_isPainting;

		/// <summary>
		/// Open window
		/// </summary>
		public static void OpenWithTexture(string texturePath)
		{
			// Make texture editable
			TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
			if (importer == null)
			{
				return;
			}
			importer.isReadable = true;
			AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);

			// Load texture
			Texture2D editTarget = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
			if (editTarget == null)
			{
				return;
			}

			var window = GetWindow<ImageEditWindow>();
			window.m_editTargetTex = editTarget;
		}

		private void OnEnable()
		{
			m_texGen = 0;
			m_texCacheList = new List<Texture2D>();
		}

		private void Update()
		{
			if (m_isDirty)
			{
				m_paintBuffer.Apply();
				Repaint();

				m_isDirty = false;
			}	
		}
		
		/// <summary>
		/// Create GUI
		/// </summary>
		private void CreateGUI()
		{
			// Load
			VisualTreeAsset tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath.ImageEditWindow);
			rootVisualElement.Add(tree.CloneTree());

			// Bind
			ColorField colorField = new ColorField("Brush Color");
			colorField.bindingPath = "m_brushColor";

			VisualElement colorFieldContainer = rootVisualElement.Q("ColorFieldContainer");
			colorFieldContainer.Add(colorField);

			rootVisualElement.Q("Spacing").style.flexGrow = 1;

			rootVisualElement.Bind(new SerializedObject(this));

			EditorApplication.delayCall += () =>
			{
				if (m_editTargetTex == null)
				{
					Close();
				}
				
				SetupStyle();
				RegisterCallbacks();
			};
		}

		/// <summary>
		/// Set style
		/// </summary>
		private void SetupStyle()
		{
			VisualElement editTarget = rootVisualElement.Q("Target");
			VisualElement canvas = rootVisualElement.Q("Canvas");
			VisualElement paintLayer = rootVisualElement.Q("PaintLayer");

			int w = m_editTargetTex.width;
			int h = m_editTargetTex.height;

			editTarget.style.backgroundImage = m_editTargetTex;
			editTarget.style.width = w;
			editTarget.style.minWidth = w;
			editTarget.style.maxWidth = w;
			editTarget.style.height = h;
			editTarget.style.minHeight = h;
			editTarget.style.maxHeight = h;

			// Create textures
			m_clearBuffer = new Texture2D(w, h, TextureFormat.RGBA32, false);
			for (int x = 0; x < m_clearBuffer.width; ++x)
			{
				for (int y = 0; y < m_clearBuffer.height; ++y)
				{
					m_clearBuffer.SetPixel(x, y, Color.clear);
				}
			}
			m_clearBuffer.Apply();

			m_canvasBuffer = new Texture2D(w, h, TextureFormat.RGBA32, false);
			Graphics.CopyTexture(m_clearBuffer, m_canvasBuffer);

			m_paintBuffer = new Texture2D(w, h, TextureFormat.RGBA32, false);
			Graphics.CopyTexture(m_clearBuffer, m_paintBuffer);

			// Initialize background
			canvas.style.backgroundImage = m_canvasBuffer;
			paintLayer.style.backgroundImage = m_paintBuffer;
		}

		/// <summary>
		/// Register callbacks
		/// </summary>
		private void RegisterCallbacks()
		{
			VisualElement editTarget = rootVisualElement.Q("Target");
			VisualElement canvas = rootVisualElement.Q("Canvas");
			VisualElement paintLayer = rootVisualElement.Q("PaintLayer");
			Button saveButton = rootVisualElement.Q<Button>("SaveButton");

			// Mouse event
			paintLayer.RegisterCallback<MouseDownEvent>(OnMouseDown);
			
			paintLayer.RegisterCallback<MouseMoveEvent>(OnMouseMove);
			
			paintLayer.RegisterCallback<MouseUpEvent>(OnMouseUp);
			
			paintLayer.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
			
			// Button event
			saveButton.clicked += SaveImage;
			
			// Undo event
			Undo.undoRedoPerformed += () =>
			{
				// Update to current generation
				if (m_texCacheList.Count > m_texGen)
				{
					m_canvasBuffer = m_texCacheList[m_texGen];
					canvas.style.backgroundImage = m_texCacheList[m_texGen];
				}

				Repaint();
			};
		}

		/// <summary>
		/// Mouse down event
		/// </summary>
		/// <param name="evt"></param>
		private void OnMouseDown(MouseDownEvent evt)
		{
			if (m_editTargetTex == null)
			{
				return;
			}

			while(m_texCacheList.Count > m_texGen)
			{
				m_texCacheList.RemoveAt(m_texCacheList.Count - 1);
			}

			Undo.RecordObject(this, "Paint");
			Texture2D copyTex = new Texture2D(m_canvasBuffer.width, m_canvasBuffer.height, TextureFormat.RGBA32, false);
			Graphics.CopyTexture(m_canvasBuffer, copyTex);
			m_texCacheList.Add(copyTex);
			m_texGen++;

			int x = Mathf.RoundToInt(evt.localMousePosition.x);
			int y = Mathf.RoundToInt(m_editTargetTex.height - evt.localMousePosition.y);

			PaintPoint(x, y);
			m_isDirty = true;
			m_isPainting = true;

			m_prevMousePos = (x, y);
		}

		/// <summary>
		/// Mouse move event
		/// </summary>
		/// <param name="evt"></param>
		private void OnMouseMove(MouseMoveEvent evt)
		{
			if (!m_isPainting)
			{
				return;
			}

			if (m_editTargetTex == null)
			{
				return;
			}

			if (evt.pressedButtons != 1)
			{
				return;
			}

			int x = Mathf.RoundToInt(evt.localMousePosition.x);
			int y = Mathf.RoundToInt(m_editTargetTex.height - evt.localMousePosition.y);

			Paint(x, y);
			m_isDirty = true; 
		}

		/// <summary>
		/// Mouse up event
		/// </summary>
		/// <param name="evt"></param>
		private void OnMouseUp(MouseUpEvent evt)
		{
			FinishPaint();
		}

		/// <summary>
		/// Mouse leave event
		/// </summary>
		/// <param name="evt"></param>
		private void OnMouseLeave(MouseLeaveEvent evt)
		{
			FinishPaint();
		}

		/// <summary>
		/// Apply and cache paint
		/// </summary>
		private void FinishPaint()
		{
			if (!m_isPainting)
			{
				return;
			}

			// Apply
			Color[] canvasColors = m_canvasBuffer.GetPixels();
			Color[] paintColors = m_paintBuffer.GetPixels();
			
			for (int i = 0; i < canvasColors.Length; i++)
			{
				canvasColors[i] = BlendColor(canvasColors[i], paintColors[i]);
			}
			
			m_canvasBuffer.SetPixels(canvasColors);
			m_canvasBuffer.Apply();

			m_paintBuffer.SetPixels(m_clearBuffer.GetPixels());
			m_paintBuffer.Apply();

			// Cache
			Texture2D copyTex = new Texture2D(m_canvasBuffer.width, m_canvasBuffer.height, TextureFormat.RGBA32, false);
			Graphics.CopyTexture(m_canvasBuffer, copyTex);
			m_texCacheList.Add(copyTex);

			m_prevMousePos = null;
			m_isPainting = false;
		}

		/// <summary>
		/// Merge each layer and save
		/// </summary>
		private void SaveImage()
		{
			// Merge canvas layer to edit image and overwrite
			for (int x = 0; x < m_canvasBuffer.width; ++x)
			{
				for (int y = 0; y < m_canvasBuffer.height; ++y)
				{
					Color baseColor = m_editTargetTex.GetPixel(x, y);
					Color canvasColor = m_canvasBuffer.GetPixel(x, y);
					m_editTargetTex.SetPixel(x, y, BlendColor(baseColor, canvasColor));
				}
			}

			m_editTargetTex.Apply();

			string texturePath = AssetDatabase.GetAssetPath(m_editTargetTex);
				
			byte[] result = m_editTargetTex.EncodeToPNG();
			File.WriteAllBytes(texturePath, result);
				
			// Revert readable
			TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
			if (importer == null)
			{
				return;
			}
			importer.isReadable = false;
			AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);
				
			GetWindow<ImageEditWindow>().Close();
		}
		
		/// <summary>
		/// Draw between two points
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		private void Paint(int x, int y)
		{
			if (m_prevMousePos.HasValue)
			{
				PaintLine(m_prevMousePos.Value.Item1, m_prevMousePos.Value.Item2, x, y);
			}
			else
			{
				PaintPoint(x, y);
			}

			m_prevMousePos = (x, y);
		}

		/// <summary>
		/// Draw line with Bresenham's line algorithm
		/// </summary>
		/// <param name="x0"></param>
		/// <param name="y0"></param>
		/// <param name="x1"></param>
		/// <param name="y1"></param>
		private void PaintLine(int x0, int y0, int x1, int y1)
		{
			int dx = Mathf.Abs(x1 - x0);
			int dy = Mathf.Abs(y1 - y0);

			int sx = 0;
			int sy = 0;

			if (x0 != x1)
			{
				sx = x0 < x1 ? 1 : -1;
			}

			if (y0 != y1)
			{
				sy = y0 < y1 ? 1 : -1;
			}

			int error = dx - dy;

			while (true)
			{
				PaintPoint(x0, y0);

				if (x0 == x1 && y0 == y1) break;

				int e2 = error * 2;
				if (e2 > -dy)
				{
					error -= dy;
					x0 += sx;
				}
				if (e2 < dx)
				{
					error += dx;
					y0 += sy;
				}
			}
		}

		/// <summary>
		/// Paint points with current brush size
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		private void PaintPoint(int x, int y)
		{
			for (int px = x - m_brushSize; px < x + m_brushSize; ++px)
			{
				for (int py = y - m_brushSize; py < y + m_brushSize; ++py)
				{
					float xdiff = x - px;
					float ydiff = y - py;
					float sqrLength = xdiff * xdiff + ydiff * ydiff;

					if (sqrLength > m_brushSize * m_brushSize)
					{
						continue;
					}

					m_paintBuffer.SetPixel(px, py, m_brushColor);
				}
			}
		}

		/// <summary>
		/// Multiply color blend
		/// </summary>
		/// <param name="baseColor"></param>
		/// <param name="newColor"></param>
		/// <returns></returns>
		private Color BlendColor(Color baseColor, Color newColor)
		{
			if (baseColor.a == 0)
			{
				return newColor;
			}

			float alpha = newColor.a;

			float r = newColor.r * alpha + baseColor.r * (1 - alpha);
			float g = newColor.g * alpha + baseColor.g * (1 - alpha);
			float b = newColor.b * alpha + baseColor.b * (1 - alpha);
			float a = Mathf.Clamp01(newColor.a + baseColor.a * (1 - newColor.a));

			return new Color(r, g, b, a);
		}
	}

}
