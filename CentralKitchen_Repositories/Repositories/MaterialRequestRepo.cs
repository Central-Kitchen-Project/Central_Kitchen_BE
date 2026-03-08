using CentralKitchen_Repositories.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CentralKitchen_Repositories.Repositories
{
    public class MaterialRequestRepo
    {
        private readonly CentralKitchenDBContext _context;

        public MaterialRequestRepo(CentralKitchenDBContext context)
        {
            _context = context;
        }

        public async Task<List<MaterialRequest>> GetAllAsync()
        {
            return await _context.MaterialRequests
                .Include(mr => mr.Order)
                .Include(mr => mr.RequestedByUser)
                .Include(mr => mr.MaterialRequestLines)
                    .ThenInclude(l => l.Item)
                .OrderByDescending(mr => mr.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<MaterialRequest>> GetByOrderIdAsync(int orderId)
        {
            return await _context.MaterialRequests
                .Include(mr => mr.Order)
                .Include(mr => mr.RequestedByUser)
                .Include(mr => mr.MaterialRequestLines)
                    .ThenInclude(l => l.Item)
                .Where(mr => mr.OrderId == orderId)
                .OrderByDescending(mr => mr.CreatedAt)
                .ToListAsync();
        }

        public async Task<MaterialRequest?> GetByIdAsync(int id)
        {
            return await _context.MaterialRequests
                .Include(mr => mr.Order)
                .Include(mr => mr.RequestedByUser)
                .Include(mr => mr.MaterialRequestLines)
                    .ThenInclude(l => l.Item)
                .FirstOrDefaultAsync(mr => mr.Id == id);
        }

        public async Task<MaterialRequest> CreateAsync(MaterialRequest request)
        {
            _context.MaterialRequests.Add(request);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(request.Id);
        }

        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
            var request = await _context.MaterialRequests.FindAsync(id);
            if (request == null) return false;

            request.Status = status;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ApproveAndUpdateInventoryAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var request = await _context.MaterialRequests
                    .Include(mr => mr.MaterialRequestLines)
                    .FirstOrDefaultAsync(mr => mr.Id == id);

                if (request == null || request.Status != "Pending")
                    return false;

                request.Status = "Approved";

                foreach (var line in request.MaterialRequestLines)
                {
                    var inventory = await _context.Inventories
                        .FirstOrDefaultAsync(inv => inv.ItemId == line.ItemId);

                    if (inventory == null)
                        continue;

                    inventory.Quantity = (inventory.Quantity ?? 0) + line.RequestedQuantity;

                    _context.InventoryTransactions.Add(new InventoryTransaction
                    {
                        InventoryId = inventory.Id,
                        TxType = "MaterialRequest",
                        Quantity = line.RequestedQuantity,
                        ReferenceType = "MaterialRequest",
                        ReferenceId = request.Id,
                        CreatedAt = DateTime.Now
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        /// <summary>
        /// Get ingredients needed for an order (from recipes) with current stock
        /// </summary>
        public async Task<List<OrderMaterialInfo>> GetOrderMaterialsAsync(int orderId)
        {
            // Get order lines with items and their recipes
            var orderLines = await _context.OrderLines
                .Include(ol => ol.Item)
                    .ThenInclude(i => i.RecipeFinishedItems)
                        .ThenInclude(r => r.IngredientItem)
                .Where(ol => ol.OrderId == orderId)
                .ToListAsync();

            // Collect all ingredient items needed (ingredient_item_id -> total qty needed)
            var ingredientNeeds = new Dictionary<int, decimal>();
            foreach (var ol in orderLines)
            {
                var orderQty = ol.Quantity ?? 0;
                foreach (var recipe in ol.Item.RecipeFinishedItems)
                {
                    var ingredientId = recipe.IngredientItemId;
                    var qtyPerUnit = recipe.Quantity;
                    var totalNeeded = qtyPerUnit * orderQty;

                    if (ingredientNeeds.ContainsKey(ingredientId))
                        ingredientNeeds[ingredientId] += totalNeeded;
                    else
                        ingredientNeeds[ingredientId] = totalNeeded;
                }
            }

            if (ingredientNeeds.Count == 0)
                return new List<OrderMaterialInfo>();

            // Get ingredient item details
            var ingredientIds = ingredientNeeds.Keys.ToList();
            var items = await _context.Items
                .Where(i => ingredientIds.Contains(i.Id))
                .ToDictionaryAsync(i => i.Id, i => i);

            // Get current stock (sum across all locations)
            var stockMap = await _context.Inventories
                .Where(inv => ingredientIds.Contains(inv.ItemId))
                .GroupBy(inv => inv.ItemId)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.Sum(inv => inv.Quantity ?? 0)
                );

            return ingredientIds.Select(id => new OrderMaterialInfo
            {
                ItemId = id,
                ItemName = items.ContainsKey(id) ? items[id].ItemName : "",
                Unit = items.ContainsKey(id) ? items[id].Unit : "",
                QuantityNeeded = ingredientNeeds[id],
                CurrentStock = stockMap.ContainsKey(id) ? stockMap[id] : 0
            }).ToList();
        }
    }

    public class OrderMaterialInfo
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public string Unit { get; set; }
        public decimal QuantityNeeded { get; set; }
        public decimal CurrentStock { get; set; }
    }
}
