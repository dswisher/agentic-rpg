using System;
using System.Threading;

namespace MeaiPoc.Cli
{
    public sealed class ConsoleCancellation : IDisposable
    {
        private readonly Lock gate = new();
        private CancellationTokenSource? current;

        public ConsoleCancellation()
        {
            Console.CancelKeyPress += OnCancelKeyPress;
        }


        public CancellationToken BeginRequest()
        {
            lock (gate)
            {
                current?.Dispose();
                current = new CancellationTokenSource();
                return current.Token;
            }
        }


        public void EndRequest()
        {
            lock (gate)
            {
                current?.Dispose();
                current = null;
            }
        }


        private void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            lock (gate)
            {
                if (current is null)
                {
                    return;   // idle: let Ctrl+C exit
                }

                e.Cancel = true;
                current.Cancel();
            }
        }


        public void Dispose()
        {
            Console.CancelKeyPress -= OnCancelKeyPress;
            EndRequest();
        }
    }
}
