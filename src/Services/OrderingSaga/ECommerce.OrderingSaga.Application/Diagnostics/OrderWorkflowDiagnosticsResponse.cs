using ECommerce.OrderingSaga.Domain;

namespace ECommerce.OrderingSaga.Application.Diagnostics;

public sealed record OrderWorkflowDiagnosticsResponse(
    Guid OrderId,
    OrderWorkflowStatus Status,
    DateTimeOffset? StepDeadlineAt,
    DateTimeOffset? TimeoutHandledAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    bool IsOverdue);
