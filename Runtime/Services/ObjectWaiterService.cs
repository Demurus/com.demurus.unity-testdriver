using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityTestDriver.Runtime.Utils;

namespace UnityTestDriver.Runtime.Services
{
    public static class ObjectWaiterService
    {
        /// <summary>
        /// Asynchronously waits up to the specified <paramref name="timeoutSeconds" /> timeout for any <paramref name="name" />
        /// game object to appear in the active scene (including on inactive GameObjects).
        /// </summary>
        /// <param name="timeoutSeconds">
        /// The maximum number of seconds to wait before giving up.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> which can be used to cancel the wait early (for example, when the caller is
        /// destroyed).
        /// </param>
        /// <returns>
        /// A <see cref="UniTask{T}" /> that completes with the first found component of type the <see cref="GameObject" /> matching
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="timeoutSeconds" /> is negative.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="name" /> is empty.
        /// </exception>
        /// <exception cref="MissingComponentException">
        /// Thrown if no component of type <see cref="GameObject" /> is found anywhere in the scene within the specified
        /// <paramref name="timeoutSeconds" />.
        /// </exception>
        public static async UniTask<GameObject> WaitForObjectAsync(string name, float timeoutSeconds, CancellationToken cancellationToken)
        {
            ArgumentVerifiers.VerifyName(name);
            ArgumentVerifiers.VerifyTimeout(timeoutSeconds);

            var timer = 0f;

            while (timer < timeoutSeconds)
            {
                var foundObject = GameObject.Find(name);

                if (foundObject != null)
                {
                    return foundObject;
                }

                timer += Time.deltaTime;
                await UniTask.Yield(cancellationToken);
            }

            throw new MissingComponentException($"{name} Object was not found by the timeout {timeoutSeconds} seconds end!");
        }

        /// <summary>
        /// Asynchronously waits up to the specified <paramref name="timeoutSeconds" /> timeout for any <typeparamref name="T" />
        /// component to appear in the active scene (including on inactive GameObjects).
        /// </summary>
        /// <typeparam name="T">
        /// The type of <see cref="Component" /> to search for. Must derive from <see cref="UnityEngine.Component" />.
        /// </typeparam>
        /// <param name="timeoutSeconds">
        /// The maximum number of seconds to wait before giving up.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken" /> which can be used to cancel the wait early (for example, when the caller is
        /// destroyed).
        /// </param>
        /// <returns>
        /// A <see cref="UniTask{T}" /> that completes with the first found component of type <typeparamref name="T" />.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="timeoutSeconds" /> is negative.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="name" /> is empty.
        /// </exception>
        /// <exception cref="MissingComponentException">
        /// Thrown if no component of type <typeparamref name="T" /> is found anywhere in the scene within the specified
        /// <paramref name="timeoutSeconds" />.
        /// </exception>
        public static async UniTask<T> WaitForObjectAsync<T>(string name,
            float timeoutSeconds,
            CancellationToken cancellationToken) where T : Component
        {
            ArgumentVerifiers.VerifyName(name);
            ArgumentVerifiers.VerifyTimeout(timeoutSeconds);

            var timer = 0f;

            while (timer < timeoutSeconds)
            {
                var component = Object.FindObjectOfType<T>(true);

                if (component != null)
                {
                    return component;
                }

                timer += Time.deltaTime;
                await UniTask.Yield(cancellationToken);
            }

            throw new MissingComponentException($"{name} Object was not found by the timeout {timeoutSeconds} seconds end!");
        }
    }
}
