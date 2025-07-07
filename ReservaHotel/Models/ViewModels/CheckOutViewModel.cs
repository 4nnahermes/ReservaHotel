namespace ReservaHotel.Models.ViewModels
{
    public class CheckOutViewModel
    {
        public CheckOut CheckOut { get; set; }
        public List<Reserva> Reservas { get; set; }
        public List<Funcionario> Funcionarios { get; set; }
    }
}
