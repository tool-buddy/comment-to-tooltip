using System;

namespace ToolBuddy.CommentToTooltip.Editor
{
    /// <summary>
    /// Represents a component that presents feedback to the user.
    /// </summary>
    public interface IUserNotifier : IDisposable
    {
        /// <summary>
        /// Displays the specified message.
        /// </summary>
        /// <param name="message">The message to display to the user.</param>
        void DisplayMessage(
            string message);
    }
}