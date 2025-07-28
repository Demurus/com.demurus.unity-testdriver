using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;

namespace UnityTestDriver.Runtime.SequenceChain
{
    public static class SequenceChainRunner
    {
        private const int CHAIN_POST_DELAY_MS = 3000;

        private static readonly StringBuilder _errorDetailsBuilder = new();

        [CanBeNull]
        private static SequenceChain _sequenceChain;

        public static IEnumerator RunFlow([NotNull] SequenceChain chain, CancellationToken cancellationToken)
        {
            var coroutine = RunFlowAsync(chain, cancellationToken).ToCoroutine();

            yield return coroutine;

            var unhandledErrors = chain.UnhandledErrors;
            var blockedByErrors = unhandledErrors.Count > 0;

            Assert.IsFalse(blockedByErrors, $"{unhandledErrors.Count} unhandled errors arised during the chain run. Errors: {GetUnhandledErrorsDetails(unhandledErrors)}");
        }

        private static async UniTask RunFlowAsync(SequenceChain chain, CancellationToken cancellationToken)
        {
            await UniTask.WaitUntil(() => _sequenceChain == null, cancellationToken: cancellationToken);

            _sequenceChain = chain;

            try
            {
                await chain.InitializeChainInternalAsync(cancellationToken);
                await UniTask.DelayFrame(1, cancellationToken: cancellationToken);
                await chain.RunChainInternalAsync(cancellationToken);
                await UniTask.Delay(CHAIN_POST_DELAY_MS, cancellationToken: cancellationToken);
            }
            finally
            {
                _sequenceChain = null;
            }
        }

        private static string GetUnhandledErrorsDetails(IReadOnlyCollection<(string logString, string stackTrace, LogType type)> unhandledErrorLogs)
        {
            _errorDetailsBuilder.Clear();

            foreach (var logStack in unhandledErrorLogs)
            {
                _errorDetailsBuilder.AppendLine($"{logStack.type}: {logStack.logString} {logStack.stackTrace}");
            }

            return _errorDetailsBuilder.ToString();
        }
    }
}
