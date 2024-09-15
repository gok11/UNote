using System.IO;
using System.Text;
using UnityEditor;

namespace UNote.Editor
{
    public class PathConstGenerator : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (!PathUtil.Initialized)
            {
                return;
            }

            if (PathUtil.IsInstalledAsPackage)
            {
                return;
            }
            
            foreach (var importedAsset in importedAssets)
            {
                if (CreateConstFile(importedAsset))
                {
                    return;
                }
            }

            foreach (var movedAsset in movedAssets)
            {
                if (CreateConstFile(movedAsset))
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Create paths of UXML and USS files during import
        /// </summary>
        private static bool CreateConstFile(string assetPath)
        {
            if (!assetPath.Contains("Editor/UXML") && !assetPath.Contains("Editor/USS"))
            {
                return false;
            }

            string targetDirectory = Directory.GetParent(assetPath).Parent.FullName;

            // 
            string[] uxmlPaths = Directory.GetFiles(targetDirectory, "*.uxml", SearchOption.AllDirectories);
            string[] ussPaths = Directory.GetFiles(targetDirectory, "*.uss", SearchOption.AllDirectories);

            // Create path file
            StringBuilder builder = StringUtil.StringBuilder;
            
            builder.AppendLine("using UnityEditor;");
            builder.AppendLine();
            builder.AppendLine("namespace UNote.Editor");
            builder.AppendLine("{");
            builder.AppendLine("    public static class UxmlPath");
            builder.AppendLine("    {");

            foreach (var uxmlPath in uxmlPaths)
            {
                string name = Path.GetFileNameWithoutExtension(uxmlPath);
                builder.AppendLine($"        public static readonly string {name} = PathUtil.GetUxmlPath(\"{name}.uxml\");");
            }

            builder.AppendLine("    }");
            builder.AppendLine();

            builder.AppendLine("    public static class UssPath");
            builder.AppendLine("    {");

            foreach (var ussPath in ussPaths)
            {
                string name = Path.GetFileNameWithoutExtension(ussPath);
                builder.AppendLine($"        public static readonly string {name} = PathUtil.GetUssPath(\"{name}.uss\");");
            }

            builder.AppendLine("    }");
            builder.AppendLine("}");         
            
            string outputPath = Path.Combine(targetDirectory, "Scripts", "FilePaths.cs");

            File.WriteAllText(outputPath, builder.ToString(), Encoding.UTF8);
            AssetDatabase.Refresh(ImportAssetOptions.ImportRecursive);

            return true;
        }
    }
}
