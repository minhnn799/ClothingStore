using System.Security.Claims;
using _02._Application.Contracts;
using _02._Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace _01.API.Controllers;

[ApiController, Authorize, Route("api")]
public sealed class ShoppingController(IStoreService store) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();
    [HttpGet("cart")] public async Task<IActionResult> Cart() => Ok(await store.CartAsync(UserId));
    [HttpPut("cart/items")] public async Task<IActionResult> SetItem(CartItemRequest r) => Ok(await store.SetCartItemAsync(UserId, r));
    [HttpDelete("cart/items/{variantId:guid}")] public async Task<IActionResult> RemoveItem(Guid variantId) { await store.RemoveCartItemAsync(UserId, variantId); return NoContent(); }
    [HttpPost("orders")] public async Task<IActionResult> Checkout(CheckoutRequest r) { var x = await store.CheckoutAsync(UserId, r); return CreatedAtAction(nameof(Order), new { id = x.Id }, x); }
    [HttpGet("orders")] public async Task<IActionResult> Orders(int page = 1, int pageSize = 20) => Ok(await store.OrdersAsync(UserId, page, pageSize));
    [HttpGet("orders/{id:guid}")] public async Task<IActionResult> Order(Guid id) => Ok(await store.OrderAsync(id, UserId));
    [HttpPost("orders/{id:guid}/cancel")] public async Task<IActionResult> Cancel(Guid id) => Ok(await store.ChangeOrderStatusAsync(id, "Cancelled", UserId));
    [Authorize(Roles = "Admin"), HttpGet("admin/orders")] public async Task<IActionResult> AllOrders(int page = 1, int pageSize = 20) => Ok(await store.OrdersAsync(null, page, pageSize));
    [Authorize(Roles = "Admin"), HttpGet("admin/orders/{id:guid}")] public async Task<IActionResult> AdminOrder(Guid id) => Ok(await store.OrderAsync(id, null));
    [Authorize(Roles = "Admin"), HttpPut("admin/orders/{id:guid}/status")] public async Task<IActionResult> Status(Guid id, OrderStatusRequest r) => Ok(await store.ChangeOrderStatusAsync(id, r.Status));
}
