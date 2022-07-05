using System;

namespace EFPerformance.Dto
{
    public class Benchmark
    {
        public Benchmark(string title)
        {
            this.Title = title;
        }

        public string Title { get; set; }
        public DateTime StartingTime { get; set; }
        public DateTime FinishingTime { get; set; }
        public double ElapsedTime { get; set; }

        public void SetFinishingTime()
        {
            FinishingTime = DateTime.Now;
            ElapsedTime = (FinishingTime - StartingTime).TotalMilliseconds;
        }

        public void SetStartingTime()
        {
            StartingTime = DateTime.Now;
        }
    }
}