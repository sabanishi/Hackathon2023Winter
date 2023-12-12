using System.IO;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Sabanishi.Editor
{
    /// <summary>
    /// タグ名を定数管理するクラスを生成するエディタ拡張
    /// </summary>
    public static class TagNameCreator
    {
        private const string CommandName = "Tools/Create/Tag Name";
        private const string FilePath = "Assets/Scripts/Common/TagName.cs";

        private static readonly string FileName = Path.GetFileName(FilePath);
        private static readonly string FileNameWithoutExtension = Path.GetFileNameWithoutExtension(FilePath);

        // 無効な文字を管理する配列
        private static readonly string[] InvalidChars =
        {
            " ", "!", "\"", "#", "$",
            "%", "&", "\'", "(", ")",
            "-", "=", "^", "~", "\\",
            "|", "[", "{", "@", "`",
            "]", "}", ":", "*", ";",
            "+", "/", "?", ".", ">",
            ",", "<"
        };

        [MenuItem(CommandName)]
        public static void Create()
        {
            if (!CanCreate()) return;
            CreateScript();
            EditorUtility.DisplayDialog(FileName, "作成が完了しました", "OK");
        }

        private static void CreateScript()
        {
            var builder = new StringBuilder();

            builder.AppendLine("namespace Sabanishi.Common");
            builder.AppendLine("{");

            builder.Append("\t").AppendLine("/// <summary>");
            builder.Append("\t").AppendLine("/// タグ名を定数で管理するクラス");
            builder.Append("\t").AppendLine("/// </summary>");
            builder.Append("\t").AppendLine($"public static class {FileNameWithoutExtension}");
            builder.Append("\t").AppendLine("{");

            foreach (var tag in InternalEditorUtility.tags)
            {
                builder.Append("\t").Append("\t")
                    .AppendLine($"public const string {RemoveInvalidChars(tag)} = \"{tag}\";");
            }

            builder.Append("\t").AppendLine("}");
            builder.AppendLine("}");

            var directoryName = Path.GetDirectoryName(FilePath);
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            File.WriteAllText(FilePath, builder.ToString(), Encoding.UTF8);
            AssetDatabase.Refresh(ImportAssetOptions.ImportRecursive);
        }

        private static bool CanCreate()
        {
            return !EditorApplication.isPlaying && !Application.isPlaying && !EditorApplication.isCompiling;
        }

        /// <summary>
        /// 無効な文字を削除する
        /// </summary>
        private static string RemoveInvalidChars(string str)
        {
            foreach (var c in InvalidChars)
            {
                str = str.Replace(c, "");
            }

            return str;
        }
    }
}
