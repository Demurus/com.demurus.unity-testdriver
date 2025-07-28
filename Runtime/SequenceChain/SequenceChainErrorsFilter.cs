using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace UnityTestDriver.Runtime.SequenceChain
{
    internal class SequenceChainErrorsFilter
    {
        private readonly IReadOnlyCollection<string> _allowedErrorPatterns;
        private readonly List<(string logString, string stackTrace, LogType type)> _unhandledErrors = new();

        public IReadOnlyCollection<(string logString, string stackTrace, LogType type)> UnhandledErrorLogs => _unhandledErrors;
        
        public SequenceChainErrorsFilter(IReadOnlyCollection<string> allowedErrorPatterns)
        {
            _allowedErrorPatterns = allowedErrorPatterns;
            Application.logMessageReceived += LogReceived;
        }

        public void Dispose()
        {
            Application.logMessageReceived -= LogReceived;
            _unhandledErrors?.Clear();
        }

        private void LogReceived(string logString, string stackTrace, LogType type)
        {
            if (type is not (LogType.Error or LogType.Exception))
            {
                return;
            }

            if (!IsAllowedError(logString))
            {
                _unhandledErrors.Add((logString, stackTrace, type));
            }
        }

        private bool IsAllowedError(string logString)
        {
            return _allowedErrorPatterns.Any(pattern => Regex.IsMatch(logString, pattern));
        }
    }
}
