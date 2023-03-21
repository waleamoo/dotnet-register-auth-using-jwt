using System.ComponentModel.DataAnnotations.Schema;

namespace ExamRoomBackend.Models
{
    public class State
    {
        public int StateId { get; set; }
        public string StateName { get; set; }
        [ForeignKey("CountryId")]
        public int CountryId { get; set; }
        public virtual Country Country { get; set; }
        public List<City> City { get; set; }    
    }
}
