using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rest.API.Translator
{
    /// <summary>
    /// Async Extensions
    /// </summary>
    public static class AsyncExtension
    {
        private static readonly TaskFactory _myTaskFactory = new TaskFactory(CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);

        /// <summary>
        /// Await the result
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <returns></returns>
        public static T Await<T>(this Task<T> task)
        {
            T result = default;
            if (task == null)
                return result;
            _myTaskFactory.StartNew(new Func<Task>(async () =>
            {
                result = await task; // Simulates a method that returns a task and
                                     // inside it is possible that there
                                     // async keywords or anothers tasks
            })).Unwrap().GetAwaiter().GetResult();
            return result;
        }

        /// <summary>
        /// Await the result
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <returns></returns>
        public static T Await<T>(this Func<Task<T>> task)
        {
            return _myTaskFactory.StartNew(task).Unwrap().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Await the result
        /// </summary>
        /// <param name="task"></param>
        public static void Await(this Task task)
        {
            task.GetAwaiter().GetResult();
        }

        /// <summary>
        /// Await the result
        /// </summary>
        /// <param name="task"></param>
        public static void Await(this Action task)
        {
            _myTaskFactory.StartNew(task).ConfigureAwait(true).GetAwaiter().GetResult();
        }
    }
}
