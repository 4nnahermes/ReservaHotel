using System.ComponentModel.DataAnnotations;
namespace ReservaHotel.Models
{
    public class Quarto
    {
        public int Id { get; set; }
        [Display(Name = "Número")]
        public int Numero { get; set; }
        [Display(Name = "Capacidade")]
        public CapacidadeQuarto Capacidade { get; set; }
        [Display(Name = "Valor da Diária")]
        public double VelorDiaria { get; set; }
        [Display(Name = "Status")]
        public StatusQuarto Status { get; set; }
        [Display(Name = "Categoria")]
        public CategoriaQuarto Categoria { get; set; }

        public enum StatusQuarto
        {
            [Display(Name = "Livre")]
            Livre = 0,
            [Display(Name = "Reservado")]
            Reservado = 1,
            [Display(Name = "Manutenção")]
            Manutencao = 2,
            [Display(Name = "Bloqueado")]
            Bloqueado = 3
        }

        public enum CategoriaQuarto
        {
            [Display(Name = "Standard")]
            Standard = 0,
            [Display(Name = "Luxo")]
            Luxo = 1,
            [Display(Name = "Super Luxo")]
            SuperLuxo = 2,
            [Display(Name = "Suíte Master")]
            SuiteMaster = 3
        }

        public enum CapacidadeQuarto
        {
            [Display(Name = "Individual")]
            Individual = 1,
            [Display(Name = "Duplo")]
            Duplo = 2,
            [Display(Name = "Triplo")]
            Triplo = 3,
            [Display(Name = "Quádruplo")]
            Quadruplo = 4
        }
    }
}
