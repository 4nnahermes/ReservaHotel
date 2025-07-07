using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ReservaHotel.Data;
using ReservaHotel.Models;

namespace ReservaHotel.Controllers
{
    public class HospedesController : Controller
    {
        private readonly ReservaHotelContext _context;

        public HospedesController(ReservaHotelContext context)
        {
            _context = context;
        }

        // GET: Hospedes
        public async Task<IActionResult> Index(string? search)
        {
            var hospedes = from h in _context.Hospede select h;

            if (!string.IsNullOrEmpty(search))
            {
                var hospedesFiltrados = hospedes.Where(h => h.Nome.Contains(search));
                var lista = await hospedesFiltrados.ToListAsync();
                if (lista.Count == 1)
                    return RedirectToAction("Details", new { id = lista[0].Id });
                if (lista.Count == 0)
                    ViewData["Mensagem"] = "Nenhum hóspede encontrado.";
                else
                    hospedes = hospedesFiltrados;
            }

            var hospedesList = await hospedes.ToListAsync();

            return View(hospedesList);
        }

        // GET: Hospedes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hospede = await _context.Hospede
                .FirstOrDefaultAsync(m => m.Id == id);
            if (hospede == null)
            {
                return NotFound();
            }

            var reservas = await _context.Reserva
    .Where(r => r.HospedeId == id)
    .ToListAsync();

            // Total de reservas realizadas
            ViewData["TotalReservas"] = reservas.Count;

            // Quarto mais reservado
            var quartoMaisReservadoId = reservas
                .GroupBy(r => r.QuartoId)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();
            var quartoMaisReservado = quartoMaisReservadoId != 0
                ? await _context.Quarto.FindAsync(quartoMaisReservadoId)
                : null;
            ViewData["QuartoMaisReservado"] = quartoMaisReservado != null ? quartoMaisReservado.Numero.ToString() : "Nenhum";

            // Último quarto reservado
            var ultimaReserva = reservas
                .OrderByDescending(r => r.DataEntrada)
                .FirstOrDefault();
            var ultimoQuarto = ultimaReserva != null
                ? await _context.Quarto.FindAsync(ultimaReserva.QuartoId)
                : null;
            ViewData["UltimoQuartoReservado"] = ultimoQuarto != null ? ultimoQuarto.Numero.ToString() : "Nenhum";

            // Pacote mais comprado
            var pacoteMaisCompradoId = reservas
                .Where(r => r.PacoteId != null)
                .GroupBy(r => r.PacoteId)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();
            var pacoteMaisComprado = pacoteMaisCompradoId != null
                ? await _context.Pacote.FindAsync(pacoteMaisCompradoId)
                : null;
            ViewData["PacoteMaisComprado"] = pacoteMaisComprado != null ? pacoteMaisComprado.Nome : "Nenhum";

            // Mês mais reservado
            var mesMaisReservado = reservas
                .GroupBy(r => r.DataEntrada.Month)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();
            ViewData["MesMaisReservado"] = mesMaisReservado;

            // Total de diárias acumuladas ($$)
            double totalDiariasValor = 0;
            int totalDiarias = 0;
            foreach (var r in reservas)
            {
                var dias = Math.Max(1, (r.DataSaida - r.DataEntrada).Days);
                totalDiarias += dias;
                var quarto = await _context.Quarto.FindAsync(r.QuartoId);
                if (quarto != null)
                    totalDiariasValor += dias * quarto.VelorDiaria;
            }
            ViewData["TotalDiarias"] = totalDiarias;
            ViewData["TotalDiariasValor"] = totalDiariasValor;

            // Total gasto em pacotes
            double totalGastoPacotes = 0;
            int reservasSemPacoteAdicional = 0;
            foreach (var r in reservas)
            {
                if (r.PacoteId != null)
                {
                    var pacote = await _context.Pacote.FindAsync(r.PacoteId);
                    if (pacote != null)
                    {
                        if (pacote.ValorAdicional > 0)
                        {
                            totalGastoPacotes += pacote.ValorAdicional;
                        }
                        else
                        {
                            reservasSemPacoteAdicional++;
                        }
                    }
                }
            }
            ViewData["TotalGastoPacotes"] = totalGastoPacotes;
            ViewData["ReservasSemPacoteAdicional"] = reservasSemPacoteAdicional;

            // Data da última reserva
            ViewData["DataUltimaReserva"] = ultimaReserva != null ? ultimaReserva.DataEntrada.ToString("dd/MM/yyyy") : "Nenhuma";

            // Média de pessoas por reserva
            double mediaPessoas = reservas.Any() ? reservas.Average(r => r.QuantidadeDePessoas) : 0;
            ViewData["MediaPessoas"] = Math.Round(mediaPessoas, 1);

            // Primeira reserva realizada
            var primeiraReserva = reservas
                .OrderBy(r => r.DataEntrada)
                .FirstOrDefault();
            ViewData["PrimeiraReserva"] = primeiraReserva != null ? primeiraReserva.DataEntrada.ToString("dd/MM/yyyy") : "Nenhuma";

            // Reservas ativas/futuras
            var reservasAtivas = reservas
                .Where(r => r.DataSaida >= DateTime.Today)
                .Select(r => new
                {
                    r.Id,
                    r.DataEntrada,
                    r.DataSaida
                })
                .ToList();

            ViewData["ReservasAtivas"] = reservasAtivas;

            return View(hospede);
        }

        // GET: Hospedes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Hospedes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nome,CPF,Telefone,Email,DataNascimento,Endereco")] Hospede hospede)
        {
            if (ModelState.IsValid)
            {
                _context.Add(hospede);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(hospede);
        }

        // GET: Hospedes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hospede = await _context.Hospede.FindAsync(id);
            if (hospede == null)
            {
                return NotFound();
            }
            return View(hospede);
        }

        // POST: Hospedes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,CPF,Telefone,Email,DataNascimento,Endereco")] Hospede hospede)
        {
            if (id != hospede.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(hospede);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HospedeExists(hospede.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(hospede);
        }

        // GET: Hospedes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hospede = await _context.Hospede
                .FirstOrDefaultAsync(m => m.Id == id);
            if (hospede == null)
            {
                return NotFound();
            }

            return View(hospede);
        }

        // POST: Hospedes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hospede = await _context.Hospede.FindAsync(id);

            // Verifica se o hóspede possui reservas futuras ou em andamento
            var possuiReservas = await _context.Reserva.AnyAsync(r =>
                r.HospedeId == id &&
                (r.Status == Reserva.StatusReserva.Pendente ||
                 r.Status == Reserva.StatusReserva.Confirmada ||
                 r.Status == Reserva.StatusReserva.Concluída) &&
                r.DataSaida >= DateTime.Today
            );

            if (possuiReservas)
            {
                ModelState.AddModelError("", "Não é possível excluir o hóspede pois ele possui reservas futuras ou em andamento.");
                return View(hospede);
            }

            if (hospede != null)
            {
                _context.Hospede.Remove(hospede);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool HospedeExists(int id)
        {
            return _context.Hospede.Any(e => e.Id == id);
        }
    }
}
