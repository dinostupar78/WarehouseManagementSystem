using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WarehouseManagementSystem.Web.Services.AI;
using static WarehouseManagementSystem.Web.Models.AI.AiSuggestionsModel;
using static WarehouseManagementSystem.Web.Models.AI.EntityAiSuggestionsModel;

namespace WarehouseManagementSystem.Web.Controllers
{
    [Route("ai")]
    public class AiSuggestionController : Controller
    {
        private readonly GroqAiService _groqAiService;
        private readonly AiReferenceMatcher _referenceMatcher;
        private readonly ILogger<AiSuggestionController> _logger;

        public AiSuggestionController(GroqAiService groqAiService, AiReferenceMatcher referenceMatcher, ILogger<AiSuggestionController> logger)
        {
            _groqAiService = groqAiService;
            _referenceMatcher = referenceMatcher;
            _logger = logger;
        }

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        [HttpPost("suggest")]
        [Authorize(Roles = "Admin,Operator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Suggest([FromBody] AiSuggestionRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Entity) ||
                string.IsNullOrWhiteSpace(request.Prompt))
            {
                return BadRequest(new AiSuggestionResponse
                {
                    Success = false,
                    Message = "Entity and prompt are required."
                });
            }

            var entity = request.Entity.Trim().ToLower();

            var json = await _groqAiService.SuggestAsync(entity, request.Prompt);

            if (string.IsNullOrWhiteSpace(json))
            {
                return Ok(new AiSuggestionResponse
                {
                    Success = false,
                    Entity = entity,
                    Message = "AI suggestion could not be generated."
                });
            }

            return entity switch
            {
                "category" => SuggestCategory(json),
                "warehouse" => SuggestWarehouse(json),
                "supplier" => SuggestSupplier(json),
                "product" => await SuggestProduct(json),
                "location" => await SuggestLocation(json),
                "inventory" => await SuggestInventory(json),
                "purchaseorder" => await SuggestPurchaseOrder(json),
                "purchaseorderitem" => await SuggestPurchaseOrderItem(json),
                _ => BadRequest(new AiSuggestionResponse
                {
                    Success = false,
                    Entity = entity,
                    Message = "Unsupported entity."
                })
            };
        }

        private IActionResult SuggestCategory(string json)
        {
            var data = JsonSerializer.Deserialize<CategoryAiSuggestion>(json, JsonOptions);

            return Ok(BuildResponse("category", data));
        }

        private IActionResult SuggestWarehouse(string json)
        {
            var data = JsonSerializer.Deserialize<WarehouseAiSuggestion>(json, JsonOptions);

            return Ok(BuildResponse("warehouse", data));
        }

        private IActionResult SuggestSupplier(string json)
        {
            var data = JsonSerializer.Deserialize<SupplierAiSuggestion>(json, JsonOptions);

            return Ok(BuildResponse("supplier", data));
        }
        private async Task<IActionResult> SuggestProduct(string json)
        {
            var data = JsonSerializer.Deserialize<ProductAiSuggestion>(json, JsonOptions);

            if (data != null)
            {
                await _referenceMatcher.MatchProductAsync(data);
            }

            return Ok(BuildResponse("product", data, data?.Message));
        }
        private async Task<IActionResult> SuggestLocation(string json)
        {
            var data = JsonSerializer.Deserialize<LocationAiSuggestion>(json, JsonOptions);

            if (data != null)
            {
                await _referenceMatcher.MatchLocationAsync(data);
            }

            return Ok(BuildResponse("location", data, data?.Message));
        }
        private async Task<IActionResult> SuggestInventory(string json)
        {
            var data = JsonSerializer.Deserialize<InventoryAiSuggestion>(json, JsonOptions);

            if (data != null)
            {
                await _referenceMatcher.MatchInventoryAsync(data);
            }

            return Ok(BuildResponse("inventory", data, data?.Message));
        }
        private async Task<IActionResult> SuggestPurchaseOrder(string json)
        {
            var data = JsonSerializer.Deserialize<PurchaseOrderAiSuggestion>(json, JsonOptions);

            if (data != null)
            {
                await _referenceMatcher.MatchPurchaseOrderAsync(data);
            }

            return Ok(BuildResponse("purchaseorder", data, data?.Message));
        }
        private async Task<IActionResult> SuggestPurchaseOrderItem(string json)
        {
            var data = JsonSerializer.Deserialize<PurchaseOrderItemAiSuggestion>(json, JsonOptions);

            if (data != null)
            {
                await _referenceMatcher.MatchPurchaseOrderItemAsync(data);
            }

            return Ok(BuildResponse("purchaseorderitem", data, data?.Message));
        }
        private static AiSuggestionResponse BuildResponse(string entity, object? data, string? message = null)
        {
            if (data == null)
            {
                return new AiSuggestionResponse
                {
                    Success = false,
                    Entity = entity,
                    Message = "AI suggestion could not be parsed."
                };
            }

            return new AiSuggestionResponse
            {
                Success = true,
                Entity = entity,
                Data = data,
                Message = message ?? "Form was filled with AI suggestion. Please review before saving."
            };
        }
    }

}
