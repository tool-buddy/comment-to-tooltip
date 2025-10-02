using ToolBuddy.CommentToTooltip.Editor.Settings;
using ToolBuddy.CommentToTooltip.FileProcessing;
using ToolBuddy.CommentToTooltip.TextProcessing;
using UnityEditor;
using VContainer;

namespace ToolBuddy.CommentToTooltip.Editor
{
    /// <summary>
    /// Configures and hosts the dependency injection container.
    /// </summary>
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

        /// <summary>
        /// Cleans up container-managed instances prior to Unity reloading assemblies.
        /// </summary>
        private static void OnBeforeAssemblyReload() =>
            _resolver.Dispose();

        /// <summary>
        /// Resolves an instance of the requested type from the editor container.
        /// </summary>
        /// <typeparam name="T">The service type to resolve.</typeparam>
        /// <returns>An instance of <typeparamref name="T"/>.</returns>
        public static T Resolve<T>() =>
            _resolver.Resolve<T>();
    }
}