using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using VHouse.Application.Commands;
using VHouse.Application.DTOs;
using VHouse.Domain.Enums;
using VHouse.Web.Models;

namespace VHouse.Web.Components.Common;

public class UniversalChatbotBase : ComponentBase
{
    [Inject] public IMediator Mediator { get; set; } = null!;

    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public string Title { get; set; } = "Asistente IA";
    [Parameter] public string Subtitle { get; set; } = "VHouse";
    [Parameter] public string HeaderIcon { get; set; } = "🤖";
    [Parameter] public string WelcomeMessage { get; set; } = "¡Hola! ¿En qué puedo ayudarte?";
    [Parameter] public string PlaceholderText { get; set; } = "Escribe tu mensaje...";
    [Parameter] public List<QuickSuggestion> QuickSuggestions { get; set; } = new();
    [Parameter] public Func<string> ContextBuilder { get; set; } = () => string.Empty;
    [Parameter] public BusinessConversationType ConversationType { get; set; } = BusinessConversationType.General;
    [Parameter] public int? CustomerId { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public EventCallback<string> OnMessageSent { get; set; }
    [Parameter] public EventCallback<BusinessConversationResponseDto> OnResponseReceived { get; set; }

    protected List<UniversalChatMessage> Messages { get; set; } = new();
    protected string InputMessage { get; set; } = "";
    protected bool IsProcessing { get; set; } = false;

    protected async Task HandleKeyPress(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !IsProcessing && !string.IsNullOrWhiteSpace(InputMessage))
        {
            await SendCurrentMessage();
        }
    }
    
    protected async Task SendCurrentMessage()
    {
        if (string.IsNullOrWhiteSpace(InputMessage) || IsProcessing) return;

        var message = InputMessage.Trim();
        InputMessage = "";
        await SendMessage(message);
    }
    
    protected async Task SendMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message) || IsProcessing) return;
        
        // Add user message
        Messages.Add(new UniversalChatMessage
        {
            Content = message,
            IsUser = true,
            Timestamp = DateTime.Now
        });
        
        // Notify parent that message was sent
        await OnMessageSent.InvokeAsync(message);
        
        IsProcessing = true;
        StateHasChanged();
        
        try
        {
            // Build context using the provided builder
            var context = ContextBuilder?.Invoke() ?? string.Empty;
            
            var command = new ProcessBusinessConversationCommand(
                Message: message,
                CustomerId: CustomerId,
                CustomerContext: context,
                ConversationType: ConversationType,
                PreferredProvider: AIProvider.Claude
            );
            
            var response = await Mediator.Send(command);
            
            // Add AI response
            var aiContent = response.IsSuccessful ? response.Response : "Lo siento, no pude procesar tu consulta. ¿Podrías reformularla?";
            
            // Handle product recommendations if any
            if (response.ProductRecommendations?.Any() == true)
            {
                aiContent += "<br><br><strong>📦 Productos recomendados:</strong><br>";
                foreach (var rec in response.ProductRecommendations.Take(3))
                {
                    aiContent += $"• {rec.ProductName} - {rec.Price:C} ";
                    aiContent += $"<button class='ai-add-product-btn' onclick='window.addProductToCart({rec.ProductId}, \"{rec.ProductName.Replace("\"", "\\\"")}\")'>➕ Agregar</button><br>";
                }
            }
            
            // Handle suggested actions if any
            if (response.SuggestedActions?.Any() == true)
            {
                aiContent += "<br><strong>💡 Acciones sugeridas:</strong><br>";
                foreach (var action in response.SuggestedActions.Take(2))
                {
                    aiContent += $"• {action.Description}<br>";
                }
            }
            
            Messages.Add(new UniversalChatMessage
            {
                Content = aiContent,
                IsUser = false,
                Timestamp = DateTime.Now
            });
            
            // Notify parent of response
            await OnResponseReceived.InvokeAsync(response);
        }
        catch (Exception ex)
        {
            Messages.Add(new UniversalChatMessage
            {
                Content = $"Error procesando consulta: {ex.Message}",
                IsUser = false,
                Timestamp = DateTime.Now
            });
        }
        finally
        {
            IsProcessing = false;
            StateHasChanged();
        }
    }

    protected async Task CloseChatbot()
    {
        await OnClose.InvokeAsync();
    }

    public void ClearMessages()
    {
        Messages.Clear();
        StateHasChanged();
    }

    public void AddMessage(string content, bool isFromUser = false)
    {
        Messages.Add(new UniversalChatMessage
        {
            Content = content,
            IsUser = isFromUser,
            Timestamp = DateTime.Now
        });
        StateHasChanged();
    }
}