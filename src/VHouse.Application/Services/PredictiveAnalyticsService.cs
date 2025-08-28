using VHouse.Domain.Interfaces;
using VHouse.Domain.ValueObjects;
using VHouse.Domain.Enums;
using VHouse.Domain.Entities;
using System.Text.Json;

namespace VHouse.Application.Services
{
    /// <summary>
    /// Fase 2: Servicios de Analítica Predictiva
    /// Incluye análisis de patrones, predicción de churn y segmentación
    /// </summary>
    public class PurchasePatternAnalyzer
    {
        private readonly IAIService _aiService;

        public PurchasePatternAnalyzer(IAIService aiService)
        {
            _aiService = aiService;
        }

        public async Task<PurchasePattern> AnalyzeCustomerPatterns(int customerId, List<Order> orderHistory)
        {
            if (!orderHistory.Any()) return new PurchasePattern();

            var pattern = new PurchasePattern();
            
            // Análisis de frecuencia de pedidos
            pattern.OrderFrequency = CalculateOrderFrequency(orderHistory);
            pattern.OrderingCycle = DetermineOrderingCycle(orderHistory);
            pattern.AverageOrderValue = orderHistory.Average(o => o.TotalAmount);
            pattern.GrowthTrend = CalculateGrowthTrend(orderHistory);
            pattern.PreferredOrderingTime = AnalyzeOrderingTimePattern(orderHistory);
            pattern.PredictedNextOrderDate = PredictNextOrderDate(orderHistory, pattern.OrderFrequency);

            return pattern;
        }

        private double CalculateOrderFrequency(List<Order> orders)
        {
            if (orders.Count < 2) return 0;

            var orderedByDate = orders.OrderBy(o => o.CreatedAt).ToList();
            var timeSpans = new List<double>();

            for (int i = 1; i < orderedByDate.Count; i++)
            {
                var daysDifference = (orderedByDate[i].CreatedAt - orderedByDate[i - 1].CreatedAt).TotalDays;
                timeSpans.Add(daysDifference);
            }

            return timeSpans.Any() ? timeSpans.Average() : 0;
        }

        private string DetermineOrderingCycle(List<Order> orders)
        {
            var frequency = CalculateOrderFrequency(orders);
            
            return frequency switch
            {
                <= 7 => "Weekly",
                <= 21 => "Bi-weekly",
                <= 35 => "Monthly",
                <= 70 => "Bi-monthly",
                <= 120 => "Quarterly",
                _ => "Irregular"
            };
        }

        private double CalculateGrowthTrend(List<Order> orders)
        {
            if (orders.Count < 2) return 0;

            var orderedByDate = orders.OrderBy(o => o.CreatedAt).ToList();
            var firstHalfAvg = orderedByDate.Take(orderedByDate.Count / 2).Average(o => o.TotalAmount);
            var secondHalfAvg = orderedByDate.Skip(orderedByDate.Count / 2).Average(o => o.TotalAmount);

            return secondHalfAvg > firstHalfAvg ? 
                (double)((secondHalfAvg - firstHalfAvg) / firstHalfAvg) : 
                -((double)((firstHalfAvg - secondHalfAvg) / firstHalfAvg));
        }

        private string AnalyzeOrderingTimePattern(List<Order> orders)
        {
            var ordersByWeek = orders.GroupBy(o => GetWeekOfMonth(o.CreatedAt))
                                   .ToDictionary(g => g.Key, g => g.Count());

            var mostCommonWeek = ordersByWeek.OrderByDescending(kvp => kvp.Value).FirstOrDefault().Key;

            return mostCommonWeek switch
            {
                1 => "First Week",
                2 => "Second Week", 
                3 => "Third Week",
                4 => "Fourth Week",
                _ => "Variable"
            };
        }

        private int GetWeekOfMonth(DateTime date)
        {
            var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
            return (date.Day - 1) / 7 + 1;
        }

        private DateTime? PredictNextOrderDate(List<Order> orders, double averageFrequency)
        {
            if (!orders.Any() || averageFrequency <= 0) return null;

            var lastOrder = orders.OrderByDescending(o => o.CreatedAt).FirstOrDefault();
            return lastOrder?.CreatedAt.AddDays(averageFrequency);
        }
    }

    public class CustomerSegmentationService
    {
        private readonly IAIService _aiService;

        public CustomerSegmentationService(IAIService aiService)
        {
            _aiService = aiService;
        }

        public async Task<CustomerSegment> CategorizeCustomer(int customerId, List<Order> orderHistory)
        {
            if (!orderHistory.Any())
            {
                return new CustomerSegment
                {
                    SegmentName = "New Customer",
                    Confidence = 1.0,
                    RecommendedStrategies = new List<string> { "Welcome campaign", "Product education", "First purchase incentive" },
                    PredictedLifetimeValue = 0
                };
            }

            var averageOrderValue = orderHistory.Average(o => o.TotalAmount);
            var totalSpent = orderHistory.Sum(o => o.TotalAmount);
            var orderCount = orderHistory.Count;
            var recencyDays = (DateTime.Now - orderHistory.Max(o => o.CreatedAt)).TotalDays;

            return CategorizeBySpendingBehavior(averageOrderValue, totalSpent, orderCount, recencyDays);
        }

        private CustomerSegment CategorizeBySpendingBehavior(decimal avgOrderValue, decimal totalSpent, int orderCount, double recencyDays)
        {
            // Segmentación basada en RFM (Recency, Frequency, Monetary)
            if (avgOrderValue >= 500)
            {
                return new CustomerSegment
                {
                    SegmentName = "Premium Customer",
                    Confidence = 0.9,
                    RecommendedStrategies = new List<string> 
                    { 
                        "VIP treatment", 
                        "Premium product recommendations", 
                        "Bulk discount offers",
                        "Priority customer service"
                    },
                    PredictedLifetimeValue = totalSpent * 3.5m
                };
            }
            else if (avgOrderValue >= 150 && orderCount >= 3)
            {
                return new CustomerSegment
                {
                    SegmentName = "Loyal Customer",
                    Confidence = 0.85,
                    RecommendedStrategies = new List<string> 
                    { 
                        "Loyalty rewards program", 
                        "Cross-sell campaigns", 
                        "Seasonal promotions",
                        "Referral incentives"
                    },
                    PredictedLifetimeValue = totalSpent * 2.8m
                };
            }
            else if (avgOrderValue < 150)
            {
                var segmentName = avgOrderValue < 50 ? "Price-Sensitive" : "Budget-Conscious";
                var confidence = avgOrderValue < 50 ? 0.7 : 0.8;
                
                return new CustomerSegment
                {
                    SegmentName = segmentName,
                    Confidence = confidence,
                    RecommendedStrategies = new List<string> 
                    { 
                        "Value-focused promotions", 
                        "Bundle deals", 
                        "Entry-level product recommendations",
                        "Payment plan options"
                    },
                    PredictedLifetimeValue = totalSpent * 1.8m
                };
            }
            else if (recencyDays > 90)
            {
                return new CustomerSegment
                {
                    SegmentName = "At-Risk Customer",
                    Confidence = 0.75,
                    RecommendedStrategies = new List<string> 
                    { 
                        "Win-back campaign", 
                        "Special discount offer", 
                        "Product update notifications",
                        "Personal outreach"
                    },
                    PredictedLifetimeValue = totalSpent * 1.2m
                };
            }

            return new CustomerSegment
            {
                SegmentName = "Regular Customer",
                Confidence = 0.8,
                RecommendedStrategies = new List<string> { "Regular engagement", "Product recommendations", "Seasonal offers" },
                PredictedLifetimeValue = totalSpent * 2.2m
            };
        }
    }

    public class CustomerChurnPredictor
    {
        private readonly IAIService _aiService;

        public CustomerChurnPredictor(IAIService aiService)
        {
            _aiService = aiService;
        }

        public async Task<ChurnRisk> PredictChurnRisk(int customerId, Customer customer, List<Order> recentOrders)
        {
            var riskFactors = new List<string>();
            var churnProbability = 0.0;

            // Análisis de recencia (cuándo fue el último pedido)
            if (recentOrders.Any())
            {
                var daysSinceLastOrder = (DateTime.Now - recentOrders.Max(o => o.CreatedAt)).TotalDays;
                if (daysSinceLastOrder > 60)
                {
                    riskFactors.Add($"No ha realizado pedidos en {daysSinceLastOrder:F0} días");
                    churnProbability += 0.3;
                }
                if (daysSinceLastOrder > 90)
                {
                    riskFactors.Add("Período extendido sin actividad");
                    churnProbability += 0.2;
                }
            }
            else
            {
                riskFactors.Add("No tiene pedidos recientes");
                churnProbability += 0.5;
            }

            // Análisis de frecuencia (tendencia decreciente en pedidos)
            if (recentOrders.Count >= 3)
            {
                var ordersDescending = recentOrders.OrderByDescending(o => o.CreatedAt).Take(3).ToList();
                var isDecreasingFrequency = IsDecreasingPattern(ordersDescending.Select(o => o.TotalAmount).ToList());
                
                if (isDecreasingFrequency)
                {
                    riskFactors.Add("Tendencia decreciente en valor de pedidos");
                    churnProbability += 0.25;
                }
            }

            // Análisis de valor monetario (órdenes de menor valor)
            if (recentOrders.Any())
            {
                var avgRecentValue = recentOrders.Average(o => o.TotalAmount);
                if (avgRecentValue < 100) // Umbral para órdenes de bajo valor
                {
                    riskFactors.Add("Valor promedio de pedidos por debajo del umbral óptimo");
                    churnProbability += 0.15;
                }
            }

            // Análisis de antigüedad del cliente vs actividad
            var customerAge = (DateTime.Now - customer.CreatedAt).TotalDays;
            if (customerAge > 365 && recentOrders.Count < 2) // Cliente antiguo con poca actividad
            {
                riskFactors.Add("Cliente de larga data con baja actividad reciente");
                churnProbability += 0.2;
            }

            var riskLevel = DetermineRiskLevel(churnProbability);
            var retentionStrategies = GenerateRetentionStrategies(riskLevel, riskFactors);

            return new ChurnRisk
            {
                ChurnProbability = Math.Min(churnProbability, 1.0), // Cap at 100%
                RiskLevel = riskLevel,
                RiskFactors = riskFactors,
                RetentionStrategies = retentionStrategies
            };
        }

        private bool IsDecreasingPattern(List<decimal> values)
        {
            if (values.Count < 2) return false;

            for (int i = 1; i < values.Count; i++)
            {
                if (values[i] >= values[i - 1])
                    return false;
            }
            return true;
        }

        private string DetermineRiskLevel(double churnProbability)
        {
            return churnProbability switch
            {
                >= 0.7 => "High",
                >= 0.5 => "Medium-High",
                >= 0.3 => "Medium",
                >= 0.1 => "Low-Medium",
                _ => "Low"
            };
        }

        private List<string> GenerateRetentionStrategies(string riskLevel, List<string> riskFactors)
        {
            var strategies = new List<string>();

            switch (riskLevel)
            {
                case "High":
                    strategies.Add("Contacto directo inmediato del equipo de cuenta");
                    strategies.Add("Oferta especial personalizada con 20-30% descuento");
                    strategies.Add("Revisión de necesidades y catálogo actualizado");
                    strategies.Add("Propuesta de términos de pago flexibles");
                    break;
                
                case "Medium-High":
                case "Medium":
                    strategies.Add("Email personalizado con productos recomendados");
                    strategies.Add("Descuento del 15% en próximo pedido");
                    strategies.Add("Invitación a webinar o evento exclusivo");
                    strategies.Add("Survey de satisfacción con incentivo");
                    break;
                
                default:
                    strategies.Add("Campaña de email con novedades");
                    strategies.Add("Newsletter con contenido de valor");
                    strategies.Add("Recordatorio de productos favoritos");
                    break;
            }

            // Estrategias específicas según factores de riesgo
            if (riskFactors.Any(f => f.Contains("pedidos")))
            {
                strategies.Add("Programa de pedidos automáticos con beneficios");
            }
            
            if (riskFactors.Any(f => f.Contains("valor")))
            {
                strategies.Add("Propuesta de productos de mayor margen con beneficios");
            }

            return strategies.Distinct().ToList();
        }
    }

    public class SeasonalityAnalyzer
    {
        private readonly IAIService _aiService;

        public SeasonalityAnalyzer(IAIService aiService)
        {
            _aiService = aiService;
        }

        public async Task<SeasonalPattern> AnalyzeProductSeasonality(int productId, List<MonthlySales> salesData)
        {
            if (salesData.Count < 4) // Necesitamos al menos 4 meses de data
            {
                return new SeasonalPattern
                {
                    PeakSeason = "Insufficient Data",
                    LowSeason = "Insufficient Data",
                    SeasonalityStrength = 0.0,
                    PredictedDemand = new List<SeasonalPrediction>()
                };
            }

            var pattern = new SeasonalPattern();
            
            // Análisis de estacionalidad
            var monthlyAverages = CalculateMonthlyAverages(salesData);
            var peakMonth = monthlyAverages.OrderByDescending(kvp => kvp.Value).First();
            var lowMonth = monthlyAverages.OrderBy(kvp => kvp.Value).First();
            
            pattern.PeakSeason = GetSeasonFromMonth(peakMonth.Key);
            pattern.LowSeason = GetSeasonFromMonth(lowMonth.Key);
            pattern.SeasonalityStrength = CalculateSeasonalityStrength(monthlyAverages.Values);
            
            // Generar predicciones para los próximos 12 meses
            pattern.PredictedDemand = GenerateSeasonalPredictions(monthlyAverages, salesData);

            return pattern;
        }

        private Dictionary<int, double> CalculateMonthlyAverages(List<MonthlySales> salesData)
        {
            return salesData.GroupBy(s => s.Month)
                           .ToDictionary(g => g.Key, g => g.Average(s => s.Quantity));
        }

        private string GetSeasonFromMonth(int month)
        {
            return month switch
            {
                12 or 1 or 2 => "Winter",
                3 or 4 or 5 => "Spring",
                6 or 7 or 8 => "Summer",
                9 or 10 or 11 => "Fall",
                _ => "Unknown"
            };
        }

        private double CalculateSeasonalityStrength(IEnumerable<double> monthlyAverages)
        {
            var values = monthlyAverages.ToList();
            if (values.Count < 2) return 0;

            var mean = values.Average();
            var variance = values.Select(v => Math.Pow(v - mean, 2)).Average();
            var stdDev = Math.Sqrt(variance);
            
            // Coeficiente de variación como medida de estacionalidad
            return mean > 0 ? stdDev / mean : 0;
        }

        private List<SeasonalPrediction> GenerateSeasonalPredictions(Dictionary<int, double> monthlyAverages, List<MonthlySales> historicalData)
        {
            var predictions = new List<SeasonalPrediction>();
            var currentYear = DateTime.Now.Year;
            var trendFactor = CalculateTrendFactor(historicalData);

            for (int month = 1; month <= 12; month++)
            {
                var baseQuantity = monthlyAverages.ContainsKey(month) ? monthlyAverages[month] : monthlyAverages.Values.Average();
                var adjustedQuantity = baseQuantity * (1 + trendFactor);

                predictions.Add(new SeasonalPrediction
                {
                    Month = month,
                    Year = currentYear,
                    EstimatedQuantity = Math.Max(1, (int)Math.Round(adjustedQuantity))
                });
            }

            return predictions;
        }

        private double CalculateTrendFactor(List<MonthlySales> historicalData)
        {
            if (historicalData.Count < 6) return 0; // No hay suficiente data para tendencia

            var orderedData = historicalData.OrderBy(s => s.Year).ThenBy(s => s.Month).ToList();
            var firstHalf = orderedData.Take(orderedData.Count / 2);
            var secondHalf = orderedData.Skip(orderedData.Count / 2);

            var firstHalfAvg = firstHalf.Average(s => s.Quantity);
            var secondHalfAvg = secondHalf.Average(s => s.Quantity);

            return firstHalfAvg > 0 ? (secondHalfAvg - firstHalfAvg) / firstHalfAvg : 0;
        }
    }

    // DTOs para los servicios
    public class PurchasePattern
    {
        public double OrderFrequency { get; set; }
        public string OrderingCycle { get; set; } = string.Empty;
        public decimal AverageOrderValue { get; set; }
        public double GrowthTrend { get; set; }
        public string PreferredOrderingTime { get; set; } = string.Empty;
        public DateTime? PredictedNextOrderDate { get; set; }
    }

    public class CustomerSegment
    {
        public string SegmentName { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public List<string> RecommendedStrategies { get; set; } = new();
        public decimal PredictedLifetimeValue { get; set; }
    }

    public class ChurnRisk
    {
        public double ChurnProbability { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public List<string> RiskFactors { get; set; } = new();
        public List<string> RetentionStrategies { get; set; } = new();
    }

    public class SeasonalPattern
    {
        public string PeakSeason { get; set; } = string.Empty;
        public string LowSeason { get; set; } = string.Empty;
        public double SeasonalityStrength { get; set; }
        public List<SeasonalPrediction> PredictedDemand { get; set; } = new();
    }

    public class SeasonalPrediction
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int EstimatedQuantity { get; set; }
    }

    public class MonthlySales
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int Quantity { get; set; }
    }
}