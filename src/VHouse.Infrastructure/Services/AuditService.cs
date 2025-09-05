using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using VHouse.Application.Services;
using VHouse.Domain.Entities;
using VHouse.Infrastructure.Data;

namespace VHouse.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly VHouseDbContext _context;
    private readonly ILogger<AuditService> _logger;

    public AuditService(VHouseDbContext context, ILogger<AuditService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task LogActionAsync(string action, string entityType, int? entityId, string userId, string userName,
                                    object? oldValues = null, object? newValues = null, string? changes = null,
                                    string severity = "INFO", string moduleName = "SYSTEM", decimal? amountInvolved = null,
                                    string? clientTenant = null, bool isSuccess = true, string? errorMessage = null,
                                    TimeSpan? executionTime = null)
    {
        try
        {
            var auditLog = new AuditLog
            {
                Action = action.Length > 50 ? action.Substring(0, 50) : action,
                EntityType = entityType.Length > 100 ? entityType.Substring(0, 100) : entityType,
                EntityId = entityId,
                UserId = userId.Length > 100 ? userId.Substring(0, 100) : userId,
                UserName = userName.Length > 200 ? userName.Substring(0, 200) : userName,
                OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
                NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
                Changes = changes?.Length > 500 ? changes.Substring(0, 500) : changes ?? string.Empty,
                Severity = severity,
                Module = moduleName.Length > 50 ? moduleName.Substring(0, 50) : moduleName,
                AmountInvolved = amountInvolved,
                ClientTenant = clientTenant?.Length > 100 ? clientTenant.Substring(0, 100) : clientTenant,
                IsSuccess = isSuccess,
                ErrorMessage = errorMessage?.Length > 500 ? errorMessage.Substring(0, 500) : errorMessage,
                ExecutionTime = executionTime,
                Timestamp = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            // Log critical events to application logger as well
            if (severity == "CRITICAL" || severity == "ERROR")
            {
                _logger.LogError("AUDIT [{Severity}] {Action} on {EntityType}:{EntityId} by {UserName} - {ErrorMessage}",
                    severity, action, entityType, entityId, userName, errorMessage ?? "No error");
            }
            else if (severity == "WARNING")
            {
                _logger.LogWarning("AUDIT [{Severity}] {Action} on {EntityType}:{EntityId} by {UserName}",
                    severity, action, entityType, entityId, userName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write audit log for action {Action} by user {UserId}", action, userId);
        }
    }

    public async Task LogBusinessEventAsync(string action, string entityType, int? entityId, string userId,
                                          string userName, decimal? amountInvolved = null, string? clientTenant = null)
    {
        var changes = amountInvolved.HasValue ? $"Amount: {amountInvolved:C}" : "Business operation";
        await LogActionAsync(action, entityType, entityId, userId, userName, null, null, changes,
                           "INFO", "BUSINESS", amountInvolved, clientTenant);
    }

    public async Task LogSecurityEventAsync(string action, string userId, string userName, string? details = null,
                                          string severity = "WARNING", bool isSuccess = true)
    {
        await LogActionAsync(action, "SECURITY", null, userId, userName, null, null, details,
                           severity, "SECURITY", null, null, isSuccess, 
                           isSuccess ? null : $"Security event failed: {details}");
    }

    public async Task LogPerformanceAsync(string operation, TimeSpan executionTime, string? moduleName = null,
                                        string? additionalData = null)
    {
        var changes = $"Execution time: {executionTime.TotalMilliseconds:F2}ms";
        if (!string.IsNullOrEmpty(additionalData))
        {
            changes += $" - {additionalData}";
        }

        await LogActionAsync(operation, "PERFORMANCE", null, "SYSTEM", "System Performance Monitor",
                           null, null, changes, "INFO", moduleName ?? "PERFORMANCE", null, null, true, null, executionTime);
    }

    public async Task<List<AuditLog>> GetAuditHistoryAsync(string? entityType = null, int? entityId = null,
                                                         DateTime? fromDate = null, DateTime? toDate = null,
                                                         string? userId = null, int pageSize = 50, int page = 1)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (!string.IsNullOrEmpty(entityType))
            query = query.Where(a => a.EntityType == entityType);

        if (entityId.HasValue)
            query = query.Where(a => a.EntityId == entityId);

        if (fromDate.HasValue)
            query = query.Where(a => a.Timestamp >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(a => a.Timestamp <= toDate.Value);

        if (!string.IsNullOrEmpty(userId))
            query = query.Where(a => a.UserId == userId);

        return await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<AuditLog>> GetSecurityEventsAsync(DateTime? fromDate = null, DateTime? toDate = null,
                                                           string severity = "WARNING", int pageSize = 50)
    {
        var query = _context.AuditLogs
            .Where(a => a.Module == "SECURITY");

        if (!string.IsNullOrEmpty(severity))
            query = query.Where(a => a.Severity == severity);

        if (fromDate.HasValue)
            query = query.Where(a => a.Timestamp >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(a => a.Timestamp <= toDate.Value);

        return await query
            .OrderByDescending(a => a.Timestamp)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<AuditLog>> GetBusinessEventsAsync(string? clientTenant = null, DateTime? fromDate = null,
                                                           DateTime? toDate = null, int pageSize = 50)
    {
        var query = _context.AuditLogs
            .Where(a => a.Module == "BUSINESS");

        if (!string.IsNullOrEmpty(clientTenant))
            query = query.Where(a => a.ClientTenant == clientTenant);

        if (fromDate.HasValue)
            query = query.Where(a => a.Timestamp >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(a => a.Timestamp <= toDate.Value);

        return await query
            .OrderByDescending(a => a.Timestamp)
            .Take(pageSize)
            .ToListAsync();
    }
}