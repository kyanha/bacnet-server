using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BACnetLibrary
{

    // This wrapper provides thread-safe queues for communicating between threads.

    public class myQueue<T> : Queue<T>
    {
        public void myEnqueue ( T obed )
        {
            lock (this)
            {
                base.Enqueue(obed);
            }
        }

        public T myDequeue()
        {
            lock (this)
            {
                return base.Dequeue();
            }
        }
    }
}
