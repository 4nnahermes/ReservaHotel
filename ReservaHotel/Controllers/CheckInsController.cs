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
    public class CheckInsController : Controller
    {
        private readonly ReservaHotelContext _context;

        public CheckInsController(ReservaHotelContext context)
        {
            _context = context;
        }

        // GET: CheckIns
        public async Task<IActionResult> Index(string? search)
        {
            var checkIns = _context.CheckIn
                .Include(c => c.Funcionario)
                .Include(c => c.Reserva)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                if (int.TryParse(search, out int reservaId))
                {
                    var checkInsFiltrados = checkIns.Where(c => c.ReservaId == reservaId);
                    var lista = await checkInsFiltrados.ToListAsync();
                    if (lista.Count == 1)
                        return RedirectToAction("Details", new { id = lista[0].Id });
                    if (lista.Count == 0)
                        ViewData["Mensagem"] = "Nenhum check-in encontrado para esse número de reserva.";
                    else
                        checkIns = checkInsFiltrados;
                }
                else
                {
                    ViewData["Mensagem"] = "Informe um número de reserva válido.";
                }
            }

            var checkInsList = await checkIns.ToListAsync();

            // Total realizado
            ViewData["Total"] = await _context.CheckIn.CountAsync();

            // Por funcionário
            ViewData["PorFuncionario"] = await _context.CheckIn
                .Include(c => c.Funcionario)
                .GroupBy(c => new { c.FuncionarioId, c.Funcionario.Nome })
                .Select(g => new { g.Key.FuncionarioId, g.Key.Nome, Total = g.Count() })
                .OrderByDescending(g => g.Total)
                .ToListAsync();

            // Por mês
            ViewData["PorMes"] = await _context.CheckIn
                .GroupBy(c => new { c.DataEHora.Year, c.DataEHora.Month })
                .Select(g => new { Mes = g.Key, Total = g.Count() })
                .OrderBy(g => g.Mes.Year).ThenBy(g => g.Mes.Month)
                .ToListAsync();

            // Por quarto
            ViewData["PorQuarto"] = await _context.CheckIn
                .Include(c => c.Reserva)
                .ThenInclude(r => r.Quarto)
                .GroupBy(c => new { c.Reserva.QuartoId, c.Reserva.Quarto.Numero })
                .Select(g => new { g.Key.QuartoId, g.Key.Numero, Total = g.Count() })
                .OrderByDescending(g => g.Total)
                .ToListAsync();

            return View(checkInsList);
        }

        // GET: CheckIns/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var checkIn = await _context.CheckIn
                .Include(c => c.Funcionario)
                .Include(c => c.Reserva)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (checkIn == null)
            {
                return NotFound();
            }

            return View(checkIn);
        }

        // GET: CheckIns/Create
        public IActionResult Create()
        {
            var viewModel = new CheckInViewModel();
            viewModel.Reservas = _context.Reserva.ToList();
            viewModel.Funcionarios = _context.Funcionario.ToList();
            return View(viewModel);
        }

        // POST: CheckIns/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CheckIn checkIn)
        {
            if (checkIn == null)
            {
                return BadRequest("Não é possível criar o checkIn");
            }

            _context.Add(checkIn);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: CheckIns/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var checkIn = await _context.CheckIn.FindAsync(id);
            if (checkIn == null)
            {
                return NotFound();
            }

            var viewModel = new CheckInViewModel
            {
                CheckIn = checkIn,
                Reservas = _context.Reserva.ToList(),
                Funcionarios = _context.Funcionario.ToList()
            };

            return View(viewModel);
        }

        // POST: CheckIns/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CheckIn checkIn)
        {
            if (id != checkIn.Id)
            {
                return NotFound();
            }

            _context.Update(checkIn);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET: CheckIns/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var checkIn = await _context.CheckIn
                .Include(c => c.Funcionario)
                .Include(c => c.Reserva)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (checkIn == null)
            {
                return NotFound();
            }

            return View(checkIn);
        }

        // POST: CheckIns/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var checkIn = await _context.CheckIn.FindAsync(id);
            if (checkIn != null)
            {
                _context.CheckIn.Remove(checkIn);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CheckInExists(int id)
        {
            return _context.CheckIn.Any(e => e.Id == id);
        }
    }
}
