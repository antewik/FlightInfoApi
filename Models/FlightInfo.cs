using FlightInfoApi.Models.AeroDataBox;

namespace FlightInfoApi.Models
{
    public class FlightInfo
    {
        public string FlightNumber { get; set; }
        public string Airline { get; set; }
        public FlightTimestamp TimeScheduled { get; set; }
        public FlightTimestamp TimeRevised { get; set; }
        public FlightTimestamp TimeRunway { get; set; }
        public string Airport { get; set; }
        public string Status { get; set; }
        public string Gate { get; set; }
        public string Terminal { get; set; }
        public string AircraftModel { get; set; }

        // Direction context: "Departure" or "Arrival"
        public string Direction { get; set; }
    }
}
