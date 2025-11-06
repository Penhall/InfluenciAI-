using InfluenciAI.Application.Content.DTOs;
using InfluenciAI.Domain.Entities;
using MediatR;

namespace InfluenciAI.Application.Content.Commands;

public record CreateContentCommand(
    string Title,
    string Body,
    ContentType Type = ContentType.Text
) : IRequest<ContentDto>;
