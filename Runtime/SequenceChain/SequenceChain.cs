using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UnityTestDriver.Runtime.SequenceChain
{
    public abstract class SequenceChain : IUniTaskAsyncDisposable
    {
        private readonly List<SequenceChain> _previousAttributeChains;

        private SequenceChainErrorsFilter _errorsFilter;

        protected SequenceChain(List<SequenceChain> previousAttributeChains = null)
        {
            _previousAttributeChains = previousAttributeChains ?? new List<SequenceChain>();
        }

        protected virtual IReadOnlyCollection<string> AllowedErrorPatterns => Array.Empty<string>();
        public IReadOnlyCollection<(string logString, string stackTrace, LogType type)> UnhandledErrors => _errorsFilter?.UnhandledErrorLogs ?? new List<(string, string, LogType)>();

        public async UniTask DisposeAsync()
        {
            foreach (var previousChain in _previousAttributeChains)
            {
                await previousChain.DisposeAsync();
            }

            _errorsFilter?.Dispose();
            await DoDisposeAsync(CancellationToken.None);
        }

        internal async UniTask InitializeChainInternalAsync(CancellationToken cancellationToken)
        {
            _errorsFilter = new SequenceChainErrorsFilter(AllowedErrorPatterns);

            foreach (var previousChain in _previousAttributeChains)
            {
                await previousChain.InitializeChainInternalAsync(cancellationToken);
                await previousChain.RunChainInternalAsync(cancellationToken);
            }

            await InitializeAsync(cancellationToken);
        }

        internal async UniTask RunChainInternalAsync(CancellationToken cancellationToken)
        {
            await RunAsync(cancellationToken);
        }

        protected virtual UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }

        protected virtual UniTask RunAsync(CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }

        protected virtual UniTask DoDisposeAsync(CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }
    }
}
