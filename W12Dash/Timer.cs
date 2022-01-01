using System;

namespace W12Dash
{
    class Timer
    {
        DateTime start;
        int time;

        public Timer()
        {
            this.time = 0;
            start = DateTime.Now;
        }

        public void Start(int time)
        {
            this.time = time;
            start = DateTime.Now;
        }

        public bool Q
        {
            get
            {
                return (DateTime.Now - start).TotalMilliseconds >= time;
            }
        }
    }
}