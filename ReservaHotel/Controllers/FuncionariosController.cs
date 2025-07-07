using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ReservaHotel.Data;
using ReservaHotel.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ReservaHotel.Controllers
{
    public class FuncionariosController : Controller
    {
        private readonly ReservaHotelContext _context;

        public FuncionariosController(ReservaHotelContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var funcionarios = from f in _context.Funcionario select f;

            if (!string.IsNullOrEmpty(search))
            {
                var funcionariosFiltrados = funcionarios.Where(f => f.Nome.Contains(search));
                var lista = await funcionariosFiltrados.ToListAsync();
                if (lista.Count == 1)
                    return RedirectToAction("Details", new { id = lista[0].Id });
                if (lista.Count == 0)
                    ViewData["Mensagem"] = "Nenhum funcionário encontrado.";
                else
                    funcionarios = funcionariosFiltrados;
            }

            var funcionariosList = await funcionarios.ToListAsync();

            // Turno com mais funcionários
            var turnoMaisFuncionarios = funcionariosList
                .GroupBy(f => f.Turno)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();
            ViewData["TurnoMaisFuncionarios"] = GetEnumDisplayName(turnoMaisFuncionarios);

            // Turno com menos funcionários
            var turnoMenosFuncionarios = funcionariosList
                .GroupBy(f => f.Turno)
                .OrderBy(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();
            ViewData["TurnoMenosFuncionarios"] = GetEnumDisplayName(turnoMenosFuncionarios);

            // Total de funcionários por cargo
            var totalPorCargo = funcionariosList
                .GroupBy(f => f.Cargo)
                .ToDictionary(g => g.Key, g => g.Count());
            ViewData["TotalPorCargo"] = totalPorCargo;

            // Total de funcionários
            ViewData["TotalFuncionarios"] = funcionariosList.Count;

            return View(funcionariosList);
        }

        // GET: Funcionarios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var funcionario = await _context.Funcionario
                .FirstOrDefaultAsync(m => m.Id == id);
            if (funcionario == null)
            {
                return NotFound();
            }

            // Número de check-ins realizados
            var totalCheckIns = await _context.CheckIn.CountAsync(c => c.FuncionarioId == funcionario.Id);
            ViewData["TotalCheckIns"] = totalCheckIns;

            // Número de check-outs realizados
            var totalCheckOuts = await _context.CheckOut.CountAsync(c => c.FuncionarioId == funcionario.Id);
            ViewData["TotalCheckOuts"] = totalCheckOuts;

            // Check-ins realizados
            var checkIns = await _context.CheckIn
                .Where(c => c.FuncionarioId == funcionario.Id)
                .OrderBy(c => c.DataEHora)
                .Select(c => new { c.Id, c.DataEHora })
                .ToListAsync();
            ViewData["ListaCheckIns"] = checkIns;

            // Check-outs realizados
            var checkOuts = await _context.CheckOut
                .Where(c => c.FuncionarioId == funcionario.Id)
                .OrderBy(c => c.DataEHora)
                .Select(c => new { c.Id, c.DataEHora })
                .ToListAsync();
            ViewData["ListaCheckOuts"] = checkOuts;

            return View(funcionario);
        }

        // GET: Funcionarios/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Funcionarios/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nome,CPF,Cargo,Turno")] Funcionario funcionario)
        {
            if (ModelState.IsValid)
            {
                _context.Add(funcionario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(funcionario);
        }

        // GET: Funcionarios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var funcionario = await _context.Funcionario.FindAsync(id);
            if (funcionario == null)
            {
                return NotFound();
            }
            return View(funcionario);
        }

        // POST: Funcionarios/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,CPF,Cargo,Turno")] Funcionario funcionario)
        {
            if (id != funcionario.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(funcionario);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FuncionarioExists(funcionario.Id))
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
            return View(funcionario);
        }

        // GET: Funcionarios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var funcionario = await _context.Funcionario
                .FirstOrDefaultAsync(m => m.Id == id);
            if (funcionario == null)
            {
                return NotFound();
            }

            return View(funcionario);
        }

        // POST: Funcionarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var funcionario = await _context.Funcionario.FindAsync(id);

            // Verifica se o funcionário está vinculado a algum Check-in ou Check-out
            bool possuiCheckInOuOut = await _context.CheckIn.AnyAsync(c => c.FuncionarioId == id)
                || await _context.CheckOut.AnyAsync(c => c.FuncionarioId == id);

            if (possuiCheckInOuOut)
            {
                ModelState.AddModelError("", "Não é possível excluir o funcionário pois ele está vinculado a um check-in ou check-out.");
                return View(funcionario);
            }

            if (funcionario != null)
            {
                _context.Funcionario.Remove(funcionario);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool FuncionarioExists(int id)
        {
            return _context.Funcionario.Any(e => e.Id == id);
        }

        private string GetEnumDisplayName(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var display = field.GetCustomAttribute<DisplayAttribute>();
            return display != null ? display.Name : value.ToString();
        }
    }
}
