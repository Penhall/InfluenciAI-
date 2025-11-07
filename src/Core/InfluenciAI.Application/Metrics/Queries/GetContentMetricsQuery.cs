using InfluenciAI.Application.Metrics.DTOs;
using MediatR;

namespace InfluenciAI.Application.Metrics.Queries;

public record GetContentMetricsQuery(Guid ContentId) : IRequest<ContentMetricsDto>;
