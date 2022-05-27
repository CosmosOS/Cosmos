namespace Cosmos.VS.DebugEngine.Utilities
{
    public static class VS
    {
        private static MessageBox? _messageBox;
        /// <summary>Shows message boxes.</summary>
        public static MessageBox MessageBox => _messageBox ??= new();
    }
}
