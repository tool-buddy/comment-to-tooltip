using ToolBuddy.CommentToTooltip.Editor.Settings;
using ToolBuddy.CommentToTooltip.FileProcessing;
using ToolBuddy.CommentToTooltip.TextProcessing;
using UnityEditor;
using VContainer;

namespace ToolBuddy.CommentToTooltip.Editor
{
    [InitializeOnLoad]
    internal static class EditorCompositionRoot
    {
        private static readonly IObjectResolver _resolver;

        static EditorCompositionRoot()
        {
            ContainerBuilder builder = new();

            builder.Register<ISettingsService, EditoPrefsSettingsService>(Lifetime.Singleton);
            builder.Register<IFileEncodingDetector, UdeFileEncodingDetector>(Lifetime.Singleton);
            builder.Register<ITextProcessor, RegexTextProcessor>(Lifetime.Singleton);
            builder.Register<IFileProcessor, FileProcessor>(Lifetime.Singleton);
            builder.Register<IUserNotifier, EditorUserNotifier>(Lifetime.Singleton);
            _resolver = builder.Build();

            // Eagerly create the notifier so it subscribes to IFileProcessor events
            Resolve<IUserNotifier>();

            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
        }

        private static void OnBeforeAssemblyReload() =>
            _resolver.Dispose();

        public static T Resolve<T>() =>
            _resolver.Resolve<T>();
    }
}