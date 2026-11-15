using Microsoft.AspNetCore.Mvc;

namespace WebApiProject.DTOs
{
    public class EventFilterParameters
    {
        [FromQuery(Name = "title")]
        public string? Title { get; set; }

        [FromQuery(Name = "from")]
        public DateTime? From { get; set; }

        [FromQuery(Name = "to")]
        public DateTime? To { get; set; }
    }
}
