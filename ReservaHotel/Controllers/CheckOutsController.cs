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
    public class CheckOutsController : Controller
    {
        private readonly ReservaHotelContext _context;

        public CheckOutsController(ReservaHotelContext context)
        {
            _context = context;
        }

        // GET: CheckOuts
        public async Task<IActionResult> Index(string? search)
        {
            var checkOuts = _context.CheckOut
                .Include(c => c.Funcionario)
                .Include(c => c.Reserva)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                if (int.TryParse(search, out int reservaId))
                {
                    var checkOutsFiltrados = checkOuts.Where(c => c.ReservaId == reservaId);
                    var lista = await checkOutsFiltrados.ToListAsync();
                    if (lista.Count == 1)
                        return RedirectToAction("Details", new { id = lista[0].Id });
                    if (lista.Count == 0)
                        ViewData["Mensagem"] = "Nenhum check-out encontrado para esse número de reserva.";
                    else
                        checkOuts = checkOutsFiltrados;
                }
                else
                {
                    ViewData["Mensagem"] = "Informe um número de reserva válido.";
                }
            }

            var checkOutsList = await checkOuts.ToListAsync();

            // Por funcionário
            ViewData["PorFuncionario"] = await _context.CheckOut
                .Include(c => c.Funcionario)
                .GroupBy(c => new { c.FuncionarioId, c.Funcionario.Nome })
                .Select(g => new { g.Key.FuncionarioId, g.Key.Nome, Total = g.Count() })
                .OrderByDescending(g => g.Total)
                .ToListAsync();

            // Por mês
            ViewData["PorMes"] = await _context.CheckOut
                .GroupBy(c => new { c.DataEHora.Year, c.DataEHora.Month })
                .Select(g => new { Mes = g.Key, Total = g.Count() })
                .OrderBy(g => g.Mes.Year).ThenBy(g => g.Mes.Month)
                .ToListAsync();

            // Por quarto (número do quarto com mais check-outs)
            ViewData["PorQuarto"] = await _context.CheckOut
                .Include(c => c.Reserva)
                .ThenInclude(r => r.Quarto)
                .GroupBy(c => new { c.Reserva.QuartoId, c.Reserva.Quarto.Numero })
                .Select(g => new { g.Key.QuartoId, g.Key.Numero, Total = g.Count() })
                .OrderByDescending(g => g.Total)
                .ToListAsync();

            return View(checkOutsList);
        }

        // GET: CheckOuts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var checkOut = await _context.CheckOut
                .Include(c => c.Funcionario)
                .Include(c => c.Reserva)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (checkOut == null)
            {
                return NotFound();
            }

            return View(checkOut);
        }

        // GET: CheckOuts/Create
        public IActionResult Create()
        {
            var viewModel = new CheckOutViewModel();
            viewModel.Reservas = _context.Reserva.ToList();
            viewModel.Funcionarios = _context.Funcionario.ToList();
            return View(viewModel);
        }

        // POST: CheckOuts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CheckOut checkOut)
        {
            if (checkOut == null)
            {
                return BadRequest("Não é possível criar o checkOut");
            }

            _context.Add(checkOut);
            await _context.SaveChangesAsync();

            // Muda o status do quarto para livre ao fazer o check-out
            var reserva = await _context.Reserva.FindAsync(checkOut.ReservaId);
            if (reserva != null)
            {
                var quarto = await _context.Quarto.FindAsync(reserva.QuartoId);
                if (quarto != null)
                {
                    quarto.Status = Quarto.StatusQuarto.Livre;
                    _context.Update(quarto);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction("Index");
        }

        // GET: CheckOuts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var checkOut = await _context.CheckOut.FindAsync(id);
            if (checkOut == null)
            {
                return NotFound();
            }

            var viewModel = new CheckOutViewModel
            {
                CheckOut = checkOut,
                Reservas = _context.Reserva.ToList(),
                Funcionarios = _context.Funcionario.ToList()
            };

            return View(viewModel);
        }

        // POST: CheckOuts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CheckOut checkOut)
        {
            if (id != checkOut.Id)
            {
                return NotFound();
            }

            _context.Update(checkOut);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET: CheckOuts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var checkOut = await _context.CheckOut
                .Include(c => c.Funcionario)
                .Include(c => c.Reserva)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (checkOut == null)
            {
                return NotFound();
            }

            return View(checkOut);
        }

        // POST: CheckOuts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var checkOut = await _context.CheckOut.FindAsync(id);
            if (checkOut != null)
            {
                _context.CheckOut.Remove(checkOut);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CheckOutExists(int id)
        {
            return _context.CheckOut.Any(e => e.Id == id);
        }
    }
}
