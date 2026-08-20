using ECommerce.OrderingSaga.Domain;

namespace ECommerce.OrderingSaga.Application.Diagnostics;

public sealed record OrderWorkflowDiagnosticsCriteria(
    int PageNumber,
    int PageSize,
    Guid? OrderId,
    OrderWorkflowStatus? Status,
    bool OverdueOnly);
