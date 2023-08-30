using System;
using System.Threading;

namespace OpenMP
{
    /// <summary>
    /// Contains relevant internal information about parallel regions, including the threads and the function to be executed.
    /// </summary>
    internal struct Region
    {
        internal Thread[] threads;
        internal object ws_lock;
        internal uint num_threads;
        internal Action omp_fn;
        internal SpinWait[] spin;

        /// <summary>
        /// Creates a specified number of threads available to the parallel region, and sets the function to be executed.
        /// Also sets other relevant data for the parallel region.
        /// </summary>
        /// <param name="num_threads">The number of threads to be created.</param>
        /// <param name="omp_fn">The function to be executed.</param>
        internal Region(uint num_threads, Action omp_fn)
        {
            threads = new Thread[num_threads];
            for (int i = 0; i < num_threads; i++)
                threads[i] = new Thread(() => omp_fn());
            ws_lock = new object();
            this.num_threads = num_threads;
            this.omp_fn = omp_fn;
            this.spin = new SpinWait[num_threads];
            for (int i = 0; i < num_threads; i++)
                this.spin[i] = new SpinWait();
        }
    }

    /// <summary>
    /// Contains the Region object and controls for creating and starting a parallel region.
    /// </summary>
    internal static class ForkedRegion
    {
        internal static Region ws;
        internal static bool in_parallel = false;

        /// <summary>
        /// Initializes the threadpool with the specified number of threads and function to be executed, as well as setting the thread names.
        /// </summary>
        /// <param name="num_threads">The number of threads to be created.</param>
        /// <param name="omp_fn">The function to be executed.</param>
        internal static void CreateThreadpool(uint num_threads, Action omp_fn)
        {
            ws = new Region(num_threads, omp_fn);
            for (int i = 0; i < num_threads; i++)
                ws.threads[i].Name = i.ToString();
        }

        /// <summary>
        /// Starts the threadpool and waits for all threads to complete before returning.
        /// </summary>
        internal static void StartThreadpool()
        {
            in_parallel = true;

            for (int i = 0; i < ws.num_threads; i++)
                ws.threads[i].Start();

            for (int i = 0; i < ws.num_threads; i++)
                ws.threads[i].Join();

            in_parallel = false;
        }
    }
}
