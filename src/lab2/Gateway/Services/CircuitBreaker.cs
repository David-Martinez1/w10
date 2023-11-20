using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;

namespace Gateway.Services
{

    public class CircuitBreaker
    {
        private static int failureCount = 0;
        private static bool IsStateOpened = false;
        private static int N = 3;
        private static volatile CircuitBreaker instance = null;
        private static object syncRoot = new object();
        
        

        public static CircuitBreaker Instance
        {

            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new CircuitBreaker();
                        }
                    }
                }
                return instance;
            }
        }

        private CircuitBreaker() {}
        public bool IsOpened()
        {
            return IsStateOpened;
        }
        

        public void ResetFailureCount()
        {
            failureCount = 0;
            IsStateOpened = false;
        }
        public void IncrementFailureCount()
        {
            failureCount++;
            IsStateOpened = failureCount >= N ? true : false;
        }

    }
}
