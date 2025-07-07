using System.ComponentModel.DataAnnotations;

namespace ReservaHotel.Models
{
    public class Funcionario
    {
        public int Id { get; set; }
        [Display(Name = "Nome")]
        public string Nome { get; set; }
        [Display(Name = "CPF")]
        public string CPF { get; set; }
        [Display(Name = "Cargo")]
        public CargoFuncionario Cargo { get; set; }
        [Display(Name = "Turno")]
        public TurnoFuncionario Turno { get; set; }

        public enum CargoFuncionario
        {
            [Display(Name = "Recepcionista")]
            Recepcionista = 0,
            [Display(Name = "Gerente")]
            Gerente = 1,
            [Display(Name = "Camareiro(a)")]
            Camareiro = 2,
            [Display(Name = "Segurança")]
            Seguranca = 3,
            [Display(Name = "Serviços Gerais")]
            ServicosGerais = 4
        }

        public enum TurnoFuncionario
        {
            [Display(Name = "06h às 14h")]
            Manha = 0,
            [Display(Name = "14h às 22h")]
            Tarde = 1,
            [Display(Name = "22h às 06h")]
            Noturno = 2,
            [Display(Name = "Escala Variável")]
            EscalaVariavel = 3

        }
    }
}
