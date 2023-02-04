using System;

namespace W12Dash
{
    internal class Timer
    {
        private DateTime _start;
        private int _time;

        public Timer()
        {
            _time = 0;
            _start = DateTime.Now;
        }

        public void Start(int time)
        {
            _time = time;
            _start = DateTime.Now;
        }

        public bool Q => (DateTime.Now - _start).TotalMilliseconds >= _time;
    }
}