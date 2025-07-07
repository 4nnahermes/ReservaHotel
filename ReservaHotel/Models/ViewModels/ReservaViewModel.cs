namespace ReservaHotel.Models.ViewModels
{
    public class ReservaViewModel
    {
        public Reserva Reserva { get; set; }
        public List<Hospede> Hospedes { get; set; }
        public List<Quarto> Quartos { get; set; }
        public List<Pacote> Pacotes { get; set; }


    }
}
