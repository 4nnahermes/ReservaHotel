using System.ComponentModel.DataAnnotations;

namespace ReservaHotel.Models
{
    public class Reserva
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O hóspede é obrigatório.")]
        [Display(Name = "Hóspede")]
        public int HospedeId { get; set; }
        public Hospede Hospede { get; set; }

        [Required(ErrorMessage = "O quarto é obrigatório.")]
        [Display(Name = "Quarto")]
        public int QuartoId { get; set; }
        public Quarto Quarto { get; set; }

        [Display(Name = "Pacote")]
        public int? PacoteId { get; set; }
        public Pacote Pacote { get; set; }

        [Required(ErrorMessage = "A data de entrada é obrigatória.")]
        [DataType(DataType.Date)]
        [Display(Name = "Entrada")]
        public DateTime DataEntrada { get; set; }

        [Required(ErrorMessage = "A data de saída é obrigatória.")]
        [DataType(DataType.Date)]
        [Display(Name = "Saída")]
        public DateTime DataSaida { get; set; }

        [Required(ErrorMessage = "A quantidade de pessoas é obrigatória.")]
        [Range(1, 10, ErrorMessage = "A quantidade de pessoas deve ser entre 1 e 10.")]
        [Display(Name = "Nº de Hóspedes")]
        public int QuantidadeDePessoas { get; set; }

        [Required(ErrorMessage = "O status é obrigatório.")]
        [Display(Name = "Status")]
        public StatusReserva Status { get; set; }

        [Display(Name = "Observações")]
        public string? Observacoes { get; set; }

        public enum StatusReserva
        {
            Pendente = 0,
            Confirmada = 1,
            Cancelada = 2,
            Concluída = 3
        }
    }
}