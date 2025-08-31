using MediatR;
using VHouse.Application.Commands;
using VHouse.Application.DTOs;
using VHouse.Domain.Interfaces;
using VHouse.Domain.ValueObjects;
using VHouse.Domain.Enums;

namespace VHouse.Application.Handlers;

public class GenerateBusinessEmailCommandHandler : IRequestHandler<GenerateBusinessEmailCommand, BusinessEmailResponseDto>
{
    private readonly IAIService _aiService;
    private readonly IUnitOfWork _unitOfWork;

    public GenerateBusinessEmailCommandHandler(IAIService aiService, IUnitOfWork unitOfWork)
    {
        _aiService = aiService;
        _unitOfWork = unitOfWork;
    }

    public async Task<BusinessEmailResponseDto> Handle(GenerateBusinessEmailCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId);
            if (customer == null)
            {
                return new BusinessEmailResponseDto
                {
                    Subject = "Error: Cliente no encontrado",
                    Body = "No se pudo generar el email debido a que el cliente no existe en el sistema.",
                    IsSuccessful = false,
                    ErrorMessage = "Cliente no encontrado"
                };
            }

            var emailPrompt = await BuildEmailPrompt(request.EmailType, customer, request.EmailData);
            
            var aiRequest = new AIRequest
            {
                Prompt = emailPrompt,
                MaxTokens = 1000,
                Temperature = 0.5,
                PreferredProvider = request.PreferredProvider ?? AIProvider.Claude
            };

            var aiResponse = await _aiService.GenerateTextAsync(aiRequest);
            
            if (!aiResponse.IsSuccessful)
            {
                return new BusinessEmailResponseDto
                {
                    Subject = "Error generando email",
                    Body = "No se pudo generar el contenido del email en este momento.",
                    IsSuccessful = false,
                    ErrorMessage = aiResponse.ErrorMessage,
                    UsedProvider = aiResponse.UsedProvider
                };
            }

            var parsedEmail = ParseEmailResponse(aiResponse.Content);
            var isUrgent = DetermineEmailUrgency(request.EmailType, request.EmailData);
            var attachments = DetermineRequiredAttachments(request.EmailType, request.EmailData);

            return new BusinessEmailResponseDto
            {
                Subject = parsedEmail.Subject,
                Body = parsedEmail.Body,
                EmailType = request.EmailType,
                IsUrgent = isUrgent,
                RequiredAttachments = attachments,
                UsedProvider = aiResponse.UsedProvider,
                IsSuccessful = true
            };
        }
        catch (Exception ex)
        {
            return new BusinessEmailResponseDto
            {
                Subject = "Error interno del sistema",
                Body = "Ocurrió un error interno al generar el email.",
                IsSuccessful = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private Task<string> BuildEmailPrompt(string emailType, dynamic customer, object emailData)
    {
        var basePrompt = $@"
ERES EL SISTEMA DE COMUNICACIONES AUTOMATIZADAS DE VHOUSE - DISTRIBUCIÓN VEGANA B2B

CONTEXTO DEL CLIENTE:
- Nombre: {customer.CustomerName}
- Email: {customer.Email}
- Tipo: {(customer.IsVeganPreferred ? "Negocio Vegano Comprometido" : "Distribuidor Ocasional")}
- Estado: {(customer.IsActive ? "Cliente Activo" : "Requiere Reactivación")}

TIPO DE EMAIL: {emailType}
DATOS ESPECÍFICOS: {System.Text.Json.JsonSerializer.Serialize(emailData)}

INSTRUCCIONES PARA GENERACIÓN:
1. Crea un email profesional pero cálido
2. Mantén el tono apropiado para comunicación B2B vegana
3. Incluye información relevante y accionable
4. Personaliza según el historial del cliente
5. Usa formato HTML básico para estructura
6. Incluye call-to-action claro cuando sea apropiado

ESTRUCTURA REQUERIDA:
ASUNTO: [Línea de asunto clara y específica]
CUERPO: [Contenido del email en HTML básico]

TIPOS DE EMAIL DISPONIBLES:
- confirmacion_pedido: Confirma detalles de pedido recibido
- actualizacion_entrega: Estado de envío y seguimiento  
- recordatorio_pago: Gestión de pagos pendientes
- alerta_producto: Notificaciones de disponibilidad
- oferta_promocional: Descuentos y promociones especiales
- campana_marketing: Comunicación de marca y valores
- actualizacion_negocio: Noticias y actualizaciones corporativas
- notificacion_tecnica: Problemas o mantenimientos del sistema

Genera el email ahora:";

        return Task.FromResult(basePrompt);
    }

    private (string Subject, string Body) ParseEmailResponse(string aiContent)
    {
        var lines = aiContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        string subject = "";
        string body = "";
        bool foundBody = false;

        foreach (var line in lines)
        {
            if (line.StartsWith("ASUNTO:", StringComparison.OrdinalIgnoreCase))
            {
                subject = line.Substring(7).Trim();
            }
            else if (line.StartsWith("CUERPO:", StringComparison.OrdinalIgnoreCase))
            {
                foundBody = true;
                continue;
            }
            else if (foundBody)
            {
                body += line + "\n";
            }
        }

        if (string.IsNullOrEmpty(subject))
        {
            subject = "Comunicación VHouse - Distribución Vegana";
        }

        if (string.IsNullOrEmpty(body))
        {
            body = aiContent;
        }

        return (subject, body.Trim());
    }

    private bool DetermineEmailUrgency(string emailType, object emailData)
    {
        var urgentTypes = new[] { "recordatorio_pago", "alerta_producto", "notificacion_tecnica" };
        
        if (urgentTypes.Contains(emailType.ToLower()))
            return true;

        var dataJson = System.Text.Json.JsonSerializer.Serialize(emailData).ToLower();
        return dataJson.Contains("urgent") || dataJson.Contains("inmediato") || 
               dataJson.Contains("crítico") || dataJson.Contains("emergencia");
    }

    private List<string> DetermineRequiredAttachments(string emailType, object emailData)
    {
        var attachments = new List<string>();

        switch (emailType.ToLower())
        {
            case "confirmacion_pedido":
                attachments.AddRange(new[] { "orden_compra.pdf", "terminos_condiciones.pdf" });
                break;
            case "actualizacion_entrega":
                attachments.Add("guia_seguimiento.pdf");
                break;
            case "recordatorio_pago":
                attachments.AddRange(new[] { "factura.pdf", "estado_cuenta.pdf" });
                break;
            case "oferta_promocional":
                attachments.Add("catalogo_promociones.pdf");
                break;
            case "campana_marketing":
                attachments.AddRange(new[] { "catalogo_productos.pdf", "hoja_valores_veganos.pdf" });
                break;
        }

        return attachments;
    }
}