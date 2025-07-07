using System.ComponentModel.DataAnnotations;

namespace ReservaHotel.Models
{
    public class CheckIn
    {
        public int Id { get; set; }
        [Display(Name = "Reserva")]
        public int ReservaId { get; set; }
        public Reserva Reserva { get; set; }
        [Display(Name = "Funcionário")]
        public int FuncionarioId { get; set; }
        public Funcionario Funcionario { get; set; }
        [Display(Name = "Entrada")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime DataEHora { get; set; }
    }
}
