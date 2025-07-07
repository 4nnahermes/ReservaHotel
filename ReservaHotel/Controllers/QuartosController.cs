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
    public class QuartosController : Controller
    {
        private readonly ReservaHotelContext _context;

        public QuartosController(ReservaHotelContext context)
        {
            _context = context;
        }

        // GET: Quartos
        public async Task<IActionResult> Index(string? search)
        {
            var quartos = from q in _context.Quarto select q;

            if (!string.IsNullOrEmpty(search))
            {
                if (int.TryParse(search, out int numero))
                {
                    var quartosFiltrados = quartos.Where(q => q.Numero == numero);
                    var lista = await quartosFiltrados.ToListAsync();
                    if (lista.Count == 1)
                        return RedirectToAction("Details", new { id = lista[0].Id });
                    if (lista.Count == 0)
                        ViewData["Mensagem"] = "Nenhum quarto encontrado com esse número.";
                    else
                        quartos = quartosFiltrados;
                }
                else
                {
                    ViewData["Mensagem"] = "Informe um número de quarto válido.";
                }
            }

            var quartosList = await quartos.ToListAsync();
            return View(quartosList);
        }

        // GET: Quartos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quarto = await _context.Quarto
                .FirstOrDefaultAsync(m => m.Id == id);
            if (quarto == null)
            {
                return NotFound();
            }

            var reservas = await _context.Reserva
                .Where(r => r.QuartoId == id)
                .ToListAsync();

            // Total de reservas
            ViewData["TotalReservas"] = reservas.Count;

            // Mês com maior número de reservas
            var mesMaisReservado = reservas
                .GroupBy(r => r.DataEntrada.Month)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();
            ViewData["MesMaisReservado"] = mesMaisReservado;

            // Pacote mais vendido junto com o quarto
            var pacoteMaisVendidoId = reservas
                .Where(r => r.PacoteId != null)
                .GroupBy(r => r.PacoteId)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();

            var pacoteMaisVendido = pacoteMaisVendidoId != null
                ? await _context.Pacote.FindAsync(pacoteMaisVendidoId)
                : null;
            ViewData["PacoteMaisVendido"] = pacoteMaisVendido != null ? pacoteMaisVendido.Nome : "Nenhum";

            // Receita total gerada pelo quarto
            double receitaTotal = reservas.Sum(r =>
                (r.DataSaida - r.DataEntrada).Days * quarto.VelorDiaria
            );
            ViewData["ReceitaTotal"] = receitaTotal;

            // Média de ocupação/pessoas do quarto (arredondado)
            double mediaOcupacao = reservas.Any() ? reservas.Average(r => r.QuantidadeDePessoas) : 0;
            ViewData["MediaOcupacao"] = Math.Round(mediaOcupacao, 0);

            return View(quarto);
        }

        // GET: Quartos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Quartos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Numero,Capacidade,VelorDiaria,Status,Categoria")] Quarto quarto)
        {
            if (quarto.Numero <= 0)
            {
                ModelState.AddModelError("Numero", "O número do quarto deve ser maior que 0.");
            }
            if (await _context.Quarto.AnyAsync(q => q.Numero == quarto.Numero))
            {
                ModelState.AddModelError("Numero", "Já existe um quarto com este número.");
            }
            if (ModelState.IsValid)
            {
                _context.Add(quarto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(quarto);
        }

        // GET: Quartos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quarto = await _context.Quarto.FindAsync(id);
            if (quarto == null)
            {
                return NotFound();
            }
            return View(quarto);
        }

        // POST: Quartos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Numero,Capacidade,VelorDiaria,Status,Categoria")] Quarto quarto)
        {
            if (id != quarto.Id)
            {
                return NotFound();
            }

            // Validação: número maior que zero
            if (quarto.Numero <= 0)
            {
                ModelState.AddModelError("Numero", "O número do quarto deve ser maior que 0.");
            }
            // Validação: número único (desconsiderando o próprio quarto)
            if (await _context.Quarto.AnyAsync(q => q.Numero == quarto.Numero && q.Id != quarto.Id))
            {
                ModelState.AddModelError("Numero", "Já existe um quarto com este número.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(quarto);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuartoExists(quarto.Id))
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
            return View(quarto);
        }

        // GET: Quartos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quarto = await _context.Quarto
                .FirstOrDefaultAsync(m => m.Id == id);
            if (quarto == null)
            {
                return NotFound();
            }

            return View(quarto);
        }

        // POST: Quartos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var quarto = await _context.Quarto.FindAsync(id);

            // Não permite deletar quarto que tenha reserva ativa ou futura
            var reservasAtivas = await _context.Reserva
                .AnyAsync(r => r.QuartoId == id &&
                    (r.Status == Reserva.StatusReserva.Confirmada || r.Status == Reserva.StatusReserva.Pendente) &&
                    r.DataSaida >= DateTime.Today);

            if (reservasAtivas)
            {
                ModelState.AddModelError("", "Não é possível excluir o quarto pois existem reservas futuras ou ativas associadas.");
                return View(quarto);
            }

            if (quarto != null)
            {
                _context.Quarto.Remove(quarto);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool QuartoExists(int id)
        {
            return _context.Quarto.Any(e => e.Id == id);
        }
    }
}
