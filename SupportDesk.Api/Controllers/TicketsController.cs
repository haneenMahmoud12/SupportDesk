using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportDesk.Application.Constants;
using SupportDesk.Application.DTOs;
using SupportDesk.Application.Models;
using SupportDesk.Application.Services.TicketService;

namespace SupportDesk.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tickets")]
public sealed class TicketsController(ITicketService ticketService) : ControllerBase
{
    [HttpGet("ticket/{id:long}")]
    public async Task<IActionResult> Get(long id, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
            return Unauthorized(Failure("The authenticated user ID is missing."));

        var ticket = await ticketService.GetTicketByIdAsync(
            id, userId, IsAdmin(), cancellationToken);
        return ticket is null
            ? NotFound(Failure($"Ticket with ID {id} was not found."))
            : Ok(new ResponseModel<TicketDTO> { Succeeded = true, Data = ticket });
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpGet("tickets")]
    public async Task<IActionResult> GetAll(
        [FromQuery] PagedRequestDTO request, CancellationToken cancellationToken)
    {
        return Ok(await ticketService.GetAllTicketsAsync(request, cancellationToken));
    }

    [HttpGet("userTickets")]
    public async Task<IActionResult> GetAllUserTickets(
        [FromQuery] PagedRequestDTO request, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
            return Unauthorized(Failure("The authenticated user ID is missing."));

        return Ok(await ticketService.GetAllUserTicketsAsync(
            request, userId, cancellationToken));
    }

    [HttpPost("saveTicket")]
    public async Task<IActionResult> Save(
        SaveTicketDTO ticket, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
            return Unauthorized(Failure("The authenticated user ID is missing."));

        return Ok(await ticketService.SaveTicketAsync(
            ticket, userId, IsAdmin(), cancellationToken));
    }

    [HttpDelete("delete/{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
            return Unauthorized(Failure("The authenticated user ID is missing."));

        return Ok(await ticketService.DeleteTicketAsync(
            id, userId, IsAdmin(), cancellationToken));
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPatch("{id:long}/status")]
    public async Task<IActionResult> UpdateStatus(
        long id, UpdateTicketStatusDTO request, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
            return Unauthorized(Failure("The authenticated user ID is missing."));

        return Ok(await ticketService.UpdateTicketStatusAsync(
            id, request.Status, userId, cancellationToken));
    }

    private string? CurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);
    private bool IsAdmin() => User.IsInRole(RoleNames.Admin);
    private static ResponseModel Failure(string message) => new()
    {
        Succeeded = false,
        Errors = [message]
    };
}
