
namespace ExamRoomBackend.Models
{
    public class Country
    {
        public int CountryId { get; set; }
        public string CountryName { get; set; } = string.Empty;
        public List<State> State { get; set; }
    }
}
