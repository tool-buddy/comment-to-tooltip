using System;

namespace ToolBuddy.CommentToTooltip.Editor
{
    public interface IUserNotifier : IDisposable
    {
        void DisplayDialogBoxMessage(
            string message);
    }
}