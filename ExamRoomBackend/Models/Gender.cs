using System.ComponentModel.DataAnnotations;

namespace ExamRoomBackend.Models
{
    public class Gender
    {
        [Key]
        public int GenderId { get; set; }
        public string GenderType { get; set; }
    }
}
