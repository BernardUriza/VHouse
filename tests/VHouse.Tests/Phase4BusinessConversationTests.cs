using Xunit;
using VHouse.Domain.Entities;
using VHouse.Domain.Enums;
using VHouse.Domain.ValueObjects;
using VHouse.Application.Handlers;
using VHouse.Application.Commands;
using VHouse.Application.DTOs;
using VHouse.Domain.Interfaces;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace VHouse.Tests
{
    /// <summary>
    /// Unit Tests para Fase 4: Chat Inteligente B2B
    /// Test-Driven Development (TDD) para funcionalidades avanzadas de IA
    /// </summary>
    public class Phase4BusinessConversationTests
    {
        private readonly Mock<IAIService> _mockAIService;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IRepository<Customer>> _mockCustomerRepo;
        private readonly ProcessBusinessConversationCommandHandler _handler;

        public Phase4BusinessConversationTests()
        {
            _mockAIService = new Mock<IAIService>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockCustomerRepo = new Mock<IRepository<Customer>>();
            
            _mockUnitOfWork.Setup(x => x.Customers).Returns(_mockCustomerRepo.Object);
            
            _handler = new ProcessBusinessConversationCommandHandler(
                _mockAIService.Object,
                _mockUnitOfWork.Object
            );
        }

        [Fact]
        public async Task ProcessBusinessConversation_ValidOrderInquiry_ShouldReturnSuccess()
        {
            // Arrange - TDD: Define expected behavior first
            var command = new ProcessBusinessConversationCommand(
                "Necesito 50 cajas de leche de avena orgánica para entrega la próxima semana",
                customerId: 1,
                conversationType: BusinessConversationType.OrderInquiry
            );

            var mockCustomer = new Customer
            {
                Id = 1,
                CustomerName = "VegaMart Distribuidora",
                Email = "pedidos@vegamart.com",
                IsVeganPreferred = true,
                IsActive = true
            };

            var aiResponse = new AIResponse
            {
                Content = "Perfecto, tenemos disponibilidad de 50 cajas de leche de avena orgánica. Precio por caja: $25 MXN. Podemos hacer la entrega la próxima semana sin problemas.",
                IsSuccessful = true,
                UsedProvider = AIProvider.Claude,
                UsedModel = AIModel.Claude35Sonnet
            };

            _mockCustomerRepo.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(mockCustomer);
            _mockAIService.Setup(x => x.GenerateTextAsync(It.IsAny<AIRequest>()))
                .ReturnsAsync(aiResponse);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.Contains("50 cajas", result.Response);
            Assert.Contains("leche de avena", result.Response);
            Assert.Equal(AIProvider.Claude, result.UsedProvider);
            Assert.Equal(BusinessConversationType.OrderInquiry.ToString(), result.ConversationContext);
            Assert.NotEmpty(result.ProductRecommendations);
            Assert.True(result.ResponseTimeMs > 0);
        }

        [Fact]
        public async Task GenerateBusinessEmail_ConfirmacionPedido_ShouldCreateProperEmail()
        {
            // Arrange - TDD for email generation
            var mockCustomer = new Customer
            {
                Id = 2,
                CustomerName = "EcoTienda Verde",
                Email = "administracion@ecotienda.com",
                IsVeganPreferred = true,
                IsActive = true
            };

            var emailData = new { pedidoId = 12345, productos = new[] { "Leche Avena", "Queso Cashew" } };

            var aiResponse = new AIResponse
            {
                Content = @"ASUNTO: Confirmación Pedido #12345 - EcoTienda Verde
CUERPO: <h3>Estimados EcoTienda Verde,</h3>
<p>Confirmamos la recepción de su pedido #12345 con los siguientes productos:</p>
<ul><li>Leche Avena</li><li>Queso Cashew</li></ul>
<p>Su pedido será procesado y entregado según los términos acordados.</p>",
                IsSuccessful = true,
                UsedProvider = AIProvider.Claude,
                UsedModel = AIModel.Claude35Sonnet
            };

            _mockCustomerRepo.Setup(x => x.GetByIdAsync(2))
                .ReturnsAsync(mockCustomer);
            _mockAIService.Setup(x => x.GenerateTextAsync(It.IsAny<AIRequest>()))
                .ReturnsAsync(aiResponse);

            // Act
            var result = await _handler.GenerateBusinessEmail("confirmacion_pedido", 2, emailData);

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.Contains("Confirmación Pedido #12345", result.Subject);
            Assert.Contains("EcoTienda Verde", result.Body);
            Assert.Contains("Leche Avena", result.Body);
            Assert.Contains("Queso Cashew", result.Body);
            Assert.Equal("confirmacion_pedido", result.EmailType);
            Assert.Contains("orden_compra.pdf", result.RequiredAttachments);
            Assert.Contains("terminos_condiciones.pdf", result.RequiredAttachments);
        }

        [Fact]
        public async Task ProcessComplexOrder_NaturalLanguage_ShouldExtractStructuredData()
        {
            // Arrange - TDD for complex order processing
            var naturalOrder = "Necesito para el restaurante 20 litros de leche de almendra, 15 quesos veganos para pizza y 10 kilos de tofu orgánico. Todo para el lunes que viene, pago a 30 días como siempre.";
            
            var mockCustomer = new Customer
            {
                Id = 3,
                CustomerName = "Restaurante Verde Gourmet",
                Email = "chef@restauranteverde.com",
                IsVeganPreferred = true,
                IsActive = true
            };

            var businessContext = new BusinessContext
            {
                CustomerType = "Restaurant",
                TypicalOrderValue = 4500,
                RecentOrderHistory = new List<string> 
                { 
                    "Pedido anterior: Leches vegetales y quesos",
                    "Pedido anterior: Tofu y tempeh orgánico" 
                }
            };

            var aiResponse = new AIResponse
            {
                Content = @"{
                    ""items"": [
                        {""product"": ""Leche de Almendra Orgánica"", ""quantity"": 20, ""notes"": ""Litros para restaurante""},
                        {""product"": ""Queso Vegano Pizza"", ""quantity"": 15, ""notes"": ""Especial para pizza""},
                        {""product"": ""Tofu Orgánico"", ""quantity"": 10, ""notes"": ""Kilos de tofu firme""}
                    ],
                    ""delivery_date"": ""lunes próximo"",
                    ""payment_terms"": ""30 días"",
                    ""special_requests"": ""Entrega temprana para preparación""
                }",
                IsSuccessful = true,
                UsedProvider = AIProvider.Claude,
                UsedModel = AIModel.Claude35Sonnet
            };

            _mockCustomerRepo.Setup(x => x.GetByIdAsync(3))
                .ReturnsAsync(mockCustomer);
            _mockAIService.Setup(x => x.GenerateTextAsync(It.IsAny<AIRequest>()))
                .ReturnsAsync(aiResponse);

            // Act
            var result = await _handler.ProcessComplexOrder(naturalOrder, 3, businessContext);

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.Equal(3, result.ExtractedItems.Count);
            
            // Verify extracted items
            Assert.Contains(result.ExtractedItems, i => i.ProductName.Contains("Leche de Almendra"));
            Assert.Contains(result.ExtractedItems, i => i.ProductName.Contains("Queso Vegano"));
            Assert.Contains(result.ExtractedItems, i => i.ProductName.Contains("Tofu"));
            
            // Verify quantities
            var lecheItem = result.ExtractedItems.First(i => i.ProductName.Contains("Leche"));
            Assert.Equal(20, lecheItem.Quantity);
            
            // Verify financial calculations
            Assert.True(result.OrderSummary.EstimatedTotal > 0);
            Assert.Equal("MXN", result.OrderSummary.Currency);
            Assert.True(result.OrderSummary.EstimatedTax > 0); // 16% IVA
            
            // Verify delivery date extraction
            Assert.NotNull(result.RequestedDeliveryDate);
            
            // Verify payment terms
            Assert.NotNull(result.PaymentTerms);
            Assert.Equal(30, result.PaymentTerms.DaysNet);
        }

        [Theory]
        [InlineData(BusinessConversationType.Complaint, "urgente", BusinessPriority.High)]
        [InlineData(BusinessConversationType.BulkOrder, "normal", BusinessPriority.High)]
        [InlineData(BusinessConversationType.General, "inmediato", BusinessPriority.Urgent)]
        [InlineData(BusinessConversationType.OrderInquiry, "normal", BusinessPriority.Medium)]
        public void DetermineConversationPriority_ShouldAssignCorrectPriority(
            BusinessConversationType type, string content, BusinessPriority expectedPriority)
        {
            // Arrange & Act - Using reflection to test private method
            var method = typeof(ProcessBusinessConversationCommandHandler)
                .GetMethod("DetermineConversationPriority", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            var result = (BusinessPriority)method!.Invoke(_handler, new object[] { type, content });

            // Assert
            Assert.Equal(expectedPriority, result);
        }

        [Fact]
        public void CalculateOrderSummary_ShouldCalculateCorrectTotals()
        {
            // Arrange
            var items = new List<OrderItem>
            {
                new OrderItem
                {
                    ProductName = "Leche de Avena Premium",
                    Quantity = 10,
                    UnitPrice = 28.50m
                },
                new OrderItem
                {
                    ProductName = "Queso Vegano Gourmet",
                    Quantity = 5,
                    UnitPrice = 65.00m
                },
                new OrderItem
                {
                    ProductName = "Yogurt Coco Orgánico",
                    Quantity = 8,
                    UnitPrice = 35.75m
                }
            };

            // Act - Using reflection to test private method
            var method = typeof(ProcessBusinessConversationCommandHandler)
                .GetMethod("CalculateOrderSummary",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            var summary = (OrderSummary)method!.Invoke(_handler, new object[] { items });

            // Assert
            Assert.Equal(23, summary.TotalItems); // 10 + 5 + 8
            Assert.Equal(896.00m, summary.SubTotal); // (10*28.5) + (5*65) + (8*35.75) = 285 + 325 + 286
            Assert.Equal(143.36m, summary.EstimatedTax); // 896 * 0.16
            Assert.Equal(1039.36m, summary.EstimatedTotal); // 896 + 143.36
            Assert.Equal("MXN", summary.Currency);
        }

        [Theory]
        [InlineData("recordatorio_pago", true)]
        [InlineData("alerta_producto", true)]
        [InlineData("notificacion_tecnica", true)]
        [InlineData("confirmacion_pedido", false)]
        [InlineData("campana_marketing", false)]
        public void DetermineEmailUrgency_ShouldIdentifyUrgentTypes(string emailType, bool expectedUrgent)
        {
            // Arrange & Act
            var method = typeof(ProcessBusinessConversationCommandHandler)
                .GetMethod("DetermineEmailUrgency",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            var isUrgent = (bool)method!.Invoke(_handler, new object[] { emailType, new { } });

            // Assert
            Assert.Equal(expectedUrgent, isUrgent);
        }

        [Theory]
        [InlineData("confirmacion_pedido", new[] { "orden_compra.pdf", "terminos_condiciones.pdf" })]
        [InlineData("actualizacion_entrega", new[] { "guia_seguimiento.pdf" })]
        [InlineData("recordatorio_pago", new[] { "factura.pdf", "estado_cuenta.pdf" })]
        [InlineData("oferta_promocional", new[] { "catalogo_promociones.pdf" })]
        [InlineData("campana_marketing", new[] { "catalogo_productos.pdf", "hoja_valores_veganos.pdf" })]
        public void DetermineRequiredAttachments_ShouldReturnCorrectAttachments(
            string emailType, string[] expectedAttachments)
        {
            // Arrange & Act
            var method = typeof(ProcessBusinessConversationCommandHandler)
                .GetMethod("DetermineRequiredAttachments",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            var attachments = (List<string>)method!.Invoke(_handler, new object[] { emailType, new { } });

            // Assert
            Assert.Equal(expectedAttachments.Length, attachments.Count);
            foreach (var expected in expectedAttachments)
            {
                Assert.Contains(expected, attachments);
            }
        }

        [Fact]
        public void GenerateBusinessAlerts_LargeOrder_ShouldCreateAlert()
        {
            // Arrange
            var items = new List<OrderItem>
            {
                new OrderItem
                {
                    ProductName = "Bulk Vegan Products",
                    Quantity = 100,
                    UnitPrice = 85.00m
                }
            };

            var context = new BusinessContext
            {
                TypicalOrderValue = 3000m,
                CustomerType = "Distributor"
            };

            // Act
            var method = typeof(ProcessBusinessConversationCommandHandler)
                .GetMethod("GenerateBusinessAlerts",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            var alerts = (List<BusinessAlert>)method!.Invoke(_handler, new object[] { items, context });

            // Assert
            Assert.NotEmpty(alerts);
            Assert.Contains(alerts, a => a.AlertType == "PEDIDO_GRANDE");
            Assert.Contains(alerts, a => a.Priority == BusinessPriority.Medium);
            Assert.Contains(alerts, a => a.Message.Contains("excede valor típico"));
            Assert.Contains(alerts, a => a.SuggestedActions.Contains("Confirmar inventario"));
        }

        [Theory]
        [InlineData("hoy", 0)]
        [InlineData("mañana", 1)]
        [InlineData("próxima semana", 7)]
        public void ExtractRequestedDeliveryDate_ShouldParseCorrectly(string dateText, int expectedDaysFromToday)
        {
            // Arrange
            var content = $"Necesito productos para {dateText} por favor";

            // Act
            var method = typeof(ProcessBusinessConversationCommandHandler)
                .GetMethod("ExtractRequestedDeliveryDate",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            var result = (DateTime?)method!.Invoke(_handler, new object[] { content });

            // Assert
            if (result.HasValue)
            {
                var expectedDate = DateTime.Today.AddDays(expectedDaysFromToday);
                Assert.Equal(expectedDate, result.Value);
            }
        }

        [Theory]
        [InlineData("pago de contado", 0, 2.0, "Pago de contado con 2% descuento")]
        [InlineData("pago a 30 días", 30, 0.0, "Net 30 días")]
        [InlineData("pago a crédito", 30, 0.0, "Net 30 días")]
        public void ExtractPaymentTerms_ShouldIdentifyTermsCorrectly(
            string paymentText, int expectedDays, double expectedDiscount, string expectedDescription)
        {
            // Arrange
            var content = $"Quiero hacer el pedido con {paymentText}";
            var context = new BusinessContext();

            // Act
            var method = typeof(ProcessBusinessConversationCommandHandler)
                .GetMethod("ExtractPaymentTerms",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            var result = (PaymentTerms?)method!.Invoke(_handler, new object[] { content, context });

            // Assert
            if (result != null)
            {
                Assert.Equal(expectedDays, result.DaysNet);
                Assert.Equal((decimal)expectedDiscount, result.DiscountPercentage);
                Assert.Equal(expectedDescription, result.TermsDescription);
            }
        }

        [Fact]
        public async Task ProcessBusinessConversation_CustomerNotFound_ShouldReturnError()
        {
            // Arrange
            var command = new ProcessBusinessConversationCommand(
                "Test message",
                customerId: 999 // Non-existent customer
            );

            _mockCustomerRepo.Setup(x => x.GetByIdAsync(999))
                .ReturnsAsync((Customer?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Contains("Lo siento", result.Response);
            Assert.NotNull(result.ErrorMessage);
        }

        [Fact]
        public async Task GenerateBusinessEmail_AIServiceFails_ShouldHandleGracefully()
        {
            // Arrange
            var mockCustomer = new Customer
            {
                Id = 1,
                CustomerName = "Test Customer",
                Email = "test@customer.com",
                IsActive = true
            };

            _mockCustomerRepo.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(mockCustomer);
            _mockAIService.Setup(x => x.GenerateTextAsync(It.IsAny<AIRequest>()))
                .ReturnsAsync(new AIResponse
                {
                    IsSuccessful = false,
                    ErrorMessage = "AI service temporarily unavailable",
                    UsedProvider = AIProvider.Claude
                });

            // Act
            var result = await _handler.GenerateBusinessEmail("confirmacion_pedido", 1, new { });

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal("Error generando email", result.Subject);
            Assert.Contains("No se pudo generar", result.Body);
            Assert.Equal("AI service temporarily unavailable", result.ErrorMessage);
        }
    }
}