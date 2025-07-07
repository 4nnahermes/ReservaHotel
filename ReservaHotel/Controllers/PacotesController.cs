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
    public class PacotesController : Controller
    {
        private readonly ReservaHotelContext _context;

        public PacotesController(ReservaHotelContext context)
        {
            _context = context;
        }

        // GET: Pacotes
        public async Task<IActionResult> Index(string? search)
        {
            var pacotes = from p in _context.Pacote select p;

            if (!string.IsNullOrEmpty(search))
            {
                var pacotesFiltrados = pacotes.Where(p => p.Nome.Contains(search));
                var lista = await pacotesFiltrados.ToListAsync();
                if (lista.Count == 1)
                    return RedirectToAction("Details", new { id = lista[0].Id });
                if (lista.Count == 0)
                    ViewData["Mensagem"] = "Nenhum pacote encontrado.";
                else
                    pacotes = pacotesFiltrados;
            }

            var pacotesList = await pacotes.ToListAsync();

            return View(pacotesList);
        }

        // GET: Pacotes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pacote = await _context.Pacote
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pacote == null)
            {
                return NotFound();
            }

            var reservas = await _context.Reserva
                .Where(r => r.PacoteId == id)
                .ToListAsync();

            // Quantas vezes o pacote foi comprado
            ViewData["TotalCompras"] = reservas.Count;

            // Mês onde o pacote é mais comprado
            var mesMaisComprado = reservas
                .GroupBy(r => r.DataEntrada.Month)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();
            ViewData["MesMaisComprado"] = mesMaisComprado;

            // Quarto mais comprado com o pacote
            var quartoMaisComprado = reservas
                .GroupBy(r => r.QuartoId)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();
            var quarto = await _context.Quarto.FindAsync(quartoMaisComprado);
            ViewData["QuartoMaisComprado"] = quarto != null ? quarto.Numero.ToString() : "Pacote ainda não comprado";

            // Valor total gerado pelo pacote
            ViewData["ValorTotalGerado"] = reservas.Count * (pacote.ValorAdicional);

            return View(pacote);
        }

        // GET: Pacotes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Pacotes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nome,Descricao,ValorAdicional")] Pacote pacote)
        {
            if (ModelState.IsValid)
            {
                _context.Add(pacote);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(pacote);
        }

        // GET: Pacotes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pacote = await _context.Pacote.FindAsync(id);
            if (pacote == null)
            {
                return NotFound();
            }
            return View(pacote);
        }

        // POST: Pacotes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Descricao,ValorAdicional")] Pacote pacote)
        {
            if (id != pacote.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pacote);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PacoteExists(pacote.Id))
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
            return View(pacote);
        }

        // GET: Pacotes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pacote = await _context.Pacote
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pacote == null)
            {
                return NotFound();
            }

            return View(pacote);
        }

        // POST: Pacotes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pacote = await _context.Pacote.FindAsync(id);

            // Verifica se o pacote está vinculado a alguma reserva
            bool possuiReserva = await _context.Reserva.AnyAsync(r => r.PacoteId == id);

            if (possuiReserva)
            {
                ModelState.AddModelError("", "Não é possível excluir o pacote pois ele está vinculado a uma reserva.");
                return View(pacote);
            }

            if (pacote != null)
            {
                _context.Pacote.Remove(pacote);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PacoteExists(int id)
        {
            return _context.Pacote.Any(e => e.Id == id);
        }
    }
}
