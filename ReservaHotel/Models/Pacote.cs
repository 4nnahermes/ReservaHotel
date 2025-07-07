using System.ComponentModel.DataAnnotations;

namespace ReservaHotel.Models
{
    public class Pacote
    {
        public int Id { get; set; }
        [Display(Name = "Nome")]
        public string Nome { get; set; }
        [Display(Name = "Descrição")]
        public string Descricao { get; set; }
        [Display(Name = "Valor Adicional")]
        public double ValorAdicional { get; set; }
    }
}
