using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetKraft
{
    class Mutex
    {
        private SemaphoreSlim _lock = new SemaphoreSlim(0);
        public void Lock()
        {
            _lock.Wait();
        }
        public void Unlock()
        {
            if (_lock.CurrentCount <= 0)
                _lock.Release();
        }
    }
}
