namespace FlightInfoApi.Models.Usage
{
    public class UsageInfo
    {
        public int CallsThisCycle { get; set; }
        public DateTime CycleStart { get; set; }
        public DateTime CycleEnd { get; set; }
    }
}
