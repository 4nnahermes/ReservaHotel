namespace ReservaHotel.Models.ViewModels
{
    public class CheckInViewModel
    {
        public CheckIn CheckIn { get; set; }
        public List<Reserva> Reservas { get; set; }
        public List<Funcionario> Funcionarios { get; set; }
    }
}
