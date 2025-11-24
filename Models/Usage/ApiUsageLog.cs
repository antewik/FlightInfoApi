using System.ComponentModel.DataAnnotations.Schema;

namespace FlightInfoApi.Models.Usage
{
    public class ApiUsageLog
    {
        public int Id { get; set; }

        public int CallsThisCycle { get; set; }

        [Column(TypeName = "date")]
        public DateTime CycleStart { get; set; }

        [Column(TypeName = "date")]
        public DateTime CycleEnd { get; set; }

        [Column(TypeName = "timestamptz")]
        public DateTime LoggedAt { get; set; } = DateTime.UtcNow;
    }
}
