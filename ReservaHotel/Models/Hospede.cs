using System.ComponentModel.DataAnnotations;

namespace ReservaHotel.Models
{
    public class Hospede
    {
        public int Id { get; set; }
        [Display(Name = "Nome")]
        public string Nome { get; set; }
        [Display(Name = "CPF")]
        public string CPF { get; set; }
        [Display(Name = "Telefone")]
        public string Telefone { get; set; }
        [Display(Name = "Email")]
        public string Email { get; set; }
        [Display(Name = "Data de Nascimento")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime DataNascimento { get; set; }
        [Display(Name = "Endereço")]
        public string Endereco { get; set; }
    }
}
