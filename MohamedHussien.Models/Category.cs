using MohamedHussien.Models; 
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;


namespace MohamedHussien.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [DisplayName("Category Name ")]
        public string Name { get; set; }

        [Range(1, 500, ErrorMessage = "state must be between 1-500")]
        [DisplayName("Category State ")]
        public int CategoryState { get; set; }
       
    }
}
