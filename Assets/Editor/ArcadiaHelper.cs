using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Arcadia;
using UnityEditor;
using BuildPipeline = Arcadia.BuildPipeline;

public class ArcadiaHelper
{
    [MenuItem("Clojure/PreCompile")]
    public static void CompileExport()
    {
        BuildPipeline.PrepareExport();
        RestoreDllImportSettings();
        GenerateLinkXml();
    }
    
    [MenuItem("Clojure/FixDllImportSetting")]
    public static void FixDllImportSetting()
    {
        RestoreDllImportSettings();
    }

    [MenuItem("Clojure/Clean")]
    public static void CleanExport()
    {
        AssetDatabase.DeleteAsset(BuildPipeline.ExportAssetsFolder);
        AssetDatabase.Refresh();
        RestoreDllImportSettings();
    }

    [MenuItem("Clojure/Generate Link.xml")]
    public static void GenerateLinkXml() {
        GenerateLinkXml(BuildPipeline.ExportAssetsFolder);
        GenerateLinkXml(BasicPaths.ClojureDllFolder);
        AssetDatabase.Refresh();
    }

    private static void RestoreDllImportSettings() {
        //change import settings for exported dlls which also exists in Infrastructure folder
        var infs = AssetDatabase.FindAssets(string.Empty, new[] {BasicPaths.ClojureDllFolder}).Select(AssetDatabase.GUIDToAssetPath);
        var exports =
            AssetDatabase.FindAssets(string.Empty, new[] {BuildPipeline.ExportAssetsFolder}).Select(AssetDatabase.GUIDToAssetPath);
        var comparer = new FileNameEqualityComparer();
        PluginImporter Path2PluginImporter(string path) =>
            AssetImporter.GetAtPath(path) as PluginImporter;
        bool NotNull<T>(T obj) => obj != null;

        exports.Intersect(infs, comparer).Select(Path2PluginImporter).Where(NotNull)
            .Select(i => {
                i.ClearSettings();
                i.SetCompatibleWithAnyPlatform(true);
                i.SetExcludeEditorFromAnyPlatform(true);
                return i;
            })
            .Concat(infs.Except(exports, comparer).Select(Path2PluginImporter).Where(NotNull)
                .Select(i => {
                    i.ClearSettings();
                    return i;
                }))
            .Concat(infs.Intersect(exports, comparer).Select(Path2PluginImporter).Where(NotNull)
                .Select(
                    i => {
                        i.ClearSettings();
                        i.SetCompatibleWithAnyPlatform(false);
                        i.SetCompatibleWithEditor(true);
                        return i;
                    }))
            .ForEach(i => i.SaveAndReimport());
        AssetDatabase.Refresh();
    }

    private static void GenerateLinkXml(string path) {
        var asms = AssetDatabase.FindAssets(string.Empty, new[] {path})
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(p => AssetImporter.GetAtPath(p) is PluginImporter)
            .Select(p => new XElement("assembly",
                new XAttribute("fullname", Path.GetFileNameWithoutExtension(p)),
                new XAttribute("preserve", "all")))
            ;
        
        new XDocument(
            new XElement("linker", asms)).Save(Path.Combine(path, "link.xml"));
    }

    private class FileNameEqualityComparer : IEqualityComparer<string> {

        public bool Equals(string x, string y) {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                return false;
            return Path.GetFileName(x) == Path.GetFileName(y);
        }

        public int GetHashCode(string obj) {
            return Path.GetFileName(obj).GetHashCode();
        }

    }
}

static class Extension
{
    public static void ForEach<T>(this IEnumerable<T> enumerable, System.Action<T> action)
    {
        foreach (var item in enumerable)
        {
            action(item);
        }
    }
}
