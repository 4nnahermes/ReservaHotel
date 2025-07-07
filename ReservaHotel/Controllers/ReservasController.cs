using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ReservaHotel.Data;
using ReservaHotel.Models;
using ReservaHotel.Models.ViewModels;

namespace ReservaHotel.Controllers
{
    public class ReservasController : Controller
    {
        private readonly ReservaHotelContext _context;

        public ReservasController(ReservaHotelContext context)
        {
            _context = context;
        }

        // GET: Reservas
        public async Task<IActionResult> Index(string? search)
        {
            var reservaHotelContext = _context.Reserva
                .Include(r => r.Hospede)
                .Include(r => r.Pacote)
                .Include(r => r.Quarto)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                if (int.TryParse(search, out int reservaId))
                {
                    var reservasFiltradas = reservaHotelContext.Where(r => r.Id == reservaId);
                    var lista = await reservasFiltradas.ToListAsync();
                    if (lista.Count == 1)
                        return RedirectToAction("Details", new { id = lista[0].Id });
                    if (lista.Count == 0)
                        ViewData["Mensagem"] = "Nenhuma reserva encontrada com esse ID.";
                    else
                        reservaHotelContext = reservasFiltradas;
                }
                else
                {
                    ViewData["Mensagem"] = "Informe um ID de reserva válido.";
                }
            }

            // Total de reservas
            ViewData["TotalReservas"] = await reservaHotelContext.CountAsync();

            // Reservas por mês
            ViewData["ReservasPorMes"] = await reservaHotelContext
                .GroupBy(r => new { r.DataEntrada.Year, r.DataEntrada.Month })
                .Select(g => new { Mes = g.Key, Total = g.Count() })
                .OrderBy(g => g.Mes.Year).ThenBy(g => g.Mes.Month)
                .ToListAsync();

            // Reservas por hóspede
            ViewData["ReservasPorHospede"] = await reservaHotelContext
                .GroupBy(r => new { r.HospedeId, r.Hospede.Nome })
                .Select(g => new { g.Key.HospedeId, g.Key.Nome, Total = g.Count() })
                .OrderByDescending(g => g.Total)
                .ToListAsync();

            // Reservas por tipo de quarto
            ViewData["ReservasPorTipoQuarto"] = await reservaHotelContext
                .GroupBy(r => new { r.QuartoId, r.Quarto.Numero })
                .Select(g => new { g.Key.QuartoId, g.Key.Numero, Total = g.Count() })
                .OrderByDescending(g => g.Total)
                .ToListAsync();

            // Reservas com pacote (exceto Standard)
            ViewData["ReservasComPacote"] = await reservaHotelContext
                .CountAsync(r => r.PacoteId != null && r.Pacote.Nome != "Standard");

            // Reservas sem pacote (null ou Standard)
            ViewData["ReservasSemPacote"] = await reservaHotelContext
                .CountAsync(r => r.PacoteId == null || r.Pacote.Nome == "Standard");

            // Reservas ativas/futuras
            ViewData["ReservasAtivas"] = await reservaHotelContext
                .CountAsync(r => r.DataSaida >= DateTime.Today && r.Status != Reserva.StatusReserva.Cancelada);

            // Reservas canceladas
            ViewData["ReservasCanceladas"] = await reservaHotelContext
                .CountAsync(r => r.Status == Reserva.StatusReserva.Cancelada);

            // Média de pessoas por reserva
            ViewData["MediaPessoas"] = await reservaHotelContext.AnyAsync()
                ? await reservaHotelContext.AverageAsync(r => r.QuantidadeDePessoas)
                : 0;

            // Média de duração das reservas (em dias)
            ViewData["MediaDuracao"] = await reservaHotelContext.AnyAsync()
                ? await reservaHotelContext.AverageAsync(r => EF.Functions.DateDiffDay(r.DataEntrada, r.DataSaida))
                : 0;

            return View(await reservaHotelContext.ToListAsync());
        }

        // GET: Reservas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reserva
                .Include(r => r.Hospede)
                .Include(r => r.Pacote)
                .Include(r => r.Quarto)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reserva == null)
            {
                return NotFound();
            }

            return View(reserva);
        }

        // GET: Reservas/Create
        public IActionResult Create()
        {
            var viewModel = new ReservaViewModel();
            viewModel.Hospedes = _context.Hospede.ToList();
            viewModel.Quartos = _context.Quarto.ToList();
            viewModel.Pacotes = _context.Pacote.ToList();
            return View(viewModel);
        }

        // POST: Reservas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReservaViewModel rvm)
        {
            if (rvm.Reserva == null)
            {
                return BadRequest("Não é possível criar a reserva.");
            }

            // Datas da reserva não podem ser no passado
            if (rvm.Reserva.DataEntrada.Date < DateTime.Today)
            {
                ModelState.AddModelError("", "A data de entrada não pode ser no passado.");
            }
            if (rvm.Reserva.DataSaida.Date < DateTime.Today)
            {
                ModelState.AddModelError("", "A data de saída não pode ser no passado.");
            }

            // Data de entrada não pode ser menor que da saída
            if (rvm.Reserva.DataEntrada >= rvm.Reserva.DataSaida)
            {
                ModelState.AddModelError("", "A data de entrada deve ser menor que a data de saída.");
            }

            // Capacidade do quarto x quantidade de pessoas da reserva
            var quarto = await _context.Quarto.FindAsync(rvm.Reserva.QuartoId);
            if (quarto != null && rvm.Reserva.QuantidadeDePessoas > (int)quarto.Capacidade)
            {
                ModelState.AddModelError("", "Quantidade de pessoas excede a capacidade do quarto.");
            }

            // Disponibilidade do quarto para a data
            bool quartoOcupado = await _context.Reserva.AnyAsync(r =>
                r.QuartoId == rvm.Reserva.QuartoId &&
                r.Id != rvm.Reserva.Id &&
                r.DataEntrada < rvm.Reserva.DataSaida &&
                rvm.Reserva.DataEntrada < r.DataSaida &&
                r.Status != Reserva.StatusReserva.Cancelada
            );
            if (quartoOcupado)
            {
                ModelState.AddModelError("", "O quarto já está reservado para o período selecionado.");
            }

            // Não pode reservar quarto bloqueado ou em manutenção
            if (quarto != null && (quarto.Status == Quarto.StatusQuarto.Bloqueado || quarto.Status == Quarto.StatusQuarto.Manutencao))
            {
                ModelState.AddModelError("", "O quarto selecionado está indisponível.");
            }

            // Comentar esta verificação caso queira fazer ou editar uma reserva            
            if (!ModelState.IsValid)
            {
                var viewModel = new ReservaViewModel
                {
                    Reserva = rvm.Reserva,
                    Hospedes = _context.Hospede.ToList(),
                    Quartos = _context.Quarto.ToList(),
                    Pacotes = _context.Pacote.ToList()
                };
                return View(viewModel);
            }

            _context.Add(rvm.Reserva);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // GET: Reservas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Reserva == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reserva.FindAsync(id);
            if (reserva == null)
            {
                return NotFound();
            }
            
            var viewModel = new ReservaViewModel
            {
                Reserva = reserva,
                Hospedes = _context.Hospede.ToList(),
                Quartos = _context.Quarto.ToList(),
                Pacotes = _context.Pacote.ToList()
            };

            return View(viewModel);
        }

        // POST: Reservas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Reserva reserva)
        {
            if (id != reserva.Id)
            {
                return NotFound();
            }
            _context.Update(reserva);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET: Reservas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reserva
                .Include(r => r.Hospede)
                .Include(r => r.Pacote)
                .Include(r => r.Quarto)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reserva == null)
            {
                return NotFound();
            }

            return View(reserva);
        }

        // POST: Reservas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reserva = await _context.Reserva.FindAsync(id);
            if (reserva != null)
            {
                _context.Reserva.Remove(reserva);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReservaExists(int id)
        {
            return _context.Reserva.Any(e => e.Id == id);
        }
    }
}
