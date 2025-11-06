using InfluenciAI.Application.Content.DTOs;
using MediatR;

namespace InfluenciAI.Application.Content.Queries;

public record ListContentQuery : IRequest<List<ContentDto>>;
