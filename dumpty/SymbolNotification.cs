using Microsoft.Diagnostics.Runtime;
using System;
using System.IO;

namespace Dumpty
{
    /// <summary>
    /// Represents notification information about symbols.
    /// </summary>
    public sealed class SymbolNotification : ISymbolNotification
    {
        private int _left, _top;

        public SymbolNotification()
        {
            _left = Console.CursorLeft;
            _top = Console.CursorTop;
        }

        private void Reset()
        {
            Console.SetCursorPosition(_left, _top);
        }

        private void Complete()
        {
            var remainder = Console.BufferWidth - Console.CursorLeft;
            if (remainder > 0) Console.Write(new string(' ', remainder));
        }

        public void DecompressionComplete(string localPath)
        {
            DownloadComplete(localPath, false);
        }

        public void DownloadComplete(string localPath, bool requiresDecompression)
        {
            Reset();
            if (requiresDecompression)
                Console.WriteLine("Decompressing:" + Path.GetFileName(localPath));
            else
                Console.WriteLine("Symbols downloaded:" + Path.GetFileName(localPath));
        }

        public void DownloadProgress(int bytesDownloaded)
        {
            Reset();
            Console.WriteLine("Downloading...");
        }

        public void FoundSymbolInCache(string localPath)
        {
        }

        public void FoundSymbolOnPath(string url)
        {
        }

        public void ProbeFailed(string url)
        {
        }
    }
}
