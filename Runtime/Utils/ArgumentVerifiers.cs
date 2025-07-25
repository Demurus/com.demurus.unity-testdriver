using System;
using Object = UnityEngine.Object;

namespace UnityTestDriver.Runtime.Utils
{
    internal class ArgumentVerifiers
    {
        public static void VerifyName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Object name can not be empty.");
            }
        }
        
        public static void VerifyParent(Object parent)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent), "Parent GameObject can not be null.");
            }
        }

        public static void VerifyTimeout(float timeout)
        {
            if (timeout < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be above 0.");
            }
        }
    }
}
