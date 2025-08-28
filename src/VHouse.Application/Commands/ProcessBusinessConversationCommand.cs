using MediatR;
using VHouse.Application.DTOs;
using VHouse.Domain.Enums;

namespace VHouse.Application.Commands;

public record ProcessBusinessConversationCommand(
    string Message,
    int? CustomerId = null,
    string? CustomerContext = null,
    BusinessConversationType ConversationType = BusinessConversationType.General,
    AIProvider? PreferredProvider = null
) : IRequest<BusinessConversationResponseDto>;

public record GenerateBusinessEmailCommand(
    string EmailType,
    int CustomerId,
    object EmailData,
    AIProvider? PreferredProvider = null
) : IRequest<BusinessEmailResponseDto>;

public record ProcessComplexOrderCommand(
    string NaturalLanguageOrder,
    int CustomerId,
    BusinessContext Context,
    AIProvider? PreferredProvider = null
) : IRequest<ComplexOrderResponseDto>;