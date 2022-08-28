using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharpcord_bot_library
{
    public class TimeoutCallbackEvent
    {
        private bool set = false;
        public bool Wait(int mstimeout)
        {
            for (int i = 0; i < mstimeout; i+=10)
            {
                if (set)
                {
                    return true;
                }
                Thread.Sleep(10);
            }
            return false;
        }
        public void Set()
        {
            set = true;
        }
        public void Reset()
        {
            set = false;
        }
    }
}
