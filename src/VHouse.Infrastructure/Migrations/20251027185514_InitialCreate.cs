using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VHouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Albums",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Albums", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Action = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    EntityType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    EntityId = table.Column<int>(type: "INTEGER", nullable: true),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    IPAddress = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    UserAgent = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    OldValues = table.Column<string>(type: "TEXT", nullable: true),
                    NewValues = table.Column<string>(type: "TEXT", nullable: true),
                    Changes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Severity = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Module = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    AmountInvolved = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ClientTenant = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    IsSuccess = table.Column<bool>(type: "INTEGER", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ExecutionTime = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BusinessAlerts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AlertType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Severity = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ClientTenant = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    RelatedEntity = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    RelatedEntityId = table.Column<int>(type: "INTEGER", nullable: true),
                    AmountInvolved = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsResolved = table.Column<bool>(type: "INTEGER", nullable: false),
                    ResolvedBy = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    ResolutionNotes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ActionData = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessAlerts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClientTenants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TenantName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TenantCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    BusinessName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ContactPerson = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    LoginUsername = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    LoginPasswordHash = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientTenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CustomerName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    IsVeganPreferred = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PriceLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    IsDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceLists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Emoji = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    PriceCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriceRetail = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriceSuggested = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PricePublic = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    StockQuantity = table.Column<int>(type: "INTEGER", nullable: false),
                    IsVegan = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Score = table.Column<double>(type: "REAL", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ContactName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    PostalCode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Country = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsVeganCertified = table.Column<bool>(type: "INTEGER", nullable: false),
                    CertificationNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CertificationExpiry = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PaymentTerms = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    MinimumOrderAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemMetrics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MetricName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    MetricType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Unit = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ClientTenant = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Metadata = table.Column<string>(type: "TEXT", nullable: true),
                    Severity = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    RequiresAction = table.Column<bool>(type: "INTEGER", nullable: false),
                    ActionRequired = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemMetrics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Photos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AlbumId = table.Column<int>(type: "INTEGER", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    OriginalName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    SizeBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    UploadedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Caption = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ThumbnailPath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Photos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Photos_Albums_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Albums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Consignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ConsignmentNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ClientTenantId = table.Column<int>(type: "INTEGER", nullable: false),
                    ConsignmentDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Terms = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    TotalValueAtCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalValueAtRetail = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StorePercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    BernardPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    TotalSold = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountDueToBernard = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountDueToStore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Consignments_ClientTenants_ClientTenantId",
                        column: x => x.ClientTenantId,
                        principalTable: "ClientTenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CustomerId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClientTenantId = table.Column<int>(type: "INTEGER", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    OrderDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_ClientTenants_ClientTenantId",
                        column: x => x.ClientTenantId,
                        principalTable: "ClientTenants",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Orders_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ClientTenantPriceLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClientTenantId = table.Column<int>(type: "INTEGER", nullable: false),
                    PriceListId = table.Column<int>(type: "INTEGER", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientTenantPriceLists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientTenantPriceLists_ClientTenants_ClientTenantId",
                        column: x => x.ClientTenantId,
                        principalTable: "ClientTenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientTenantPriceLists_PriceLists_PriceListId",
                        column: x => x.PriceListId,
                        principalTable: "PriceLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClientTenantId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    CustomPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinOrderQuantity = table.Column<int>(type: "INTEGER", nullable: false),
                    IsAvailable = table.Column<bool>(type: "INTEGER", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientProducts_ClientTenants_ClientTenantId",
                        column: x => x.ClientTenantId,
                        principalTable: "ClientTenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientProducts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PriceListItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PriceListId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    CustomPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    MinOrderQuantity = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceListItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceListItems_PriceLists_PriceListId",
                        column: x => x.PriceListId,
                        principalTable: "PriceLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PriceListItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConsignmentItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ConsignmentId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    QuantityConsigned = table.Column<int>(type: "INTEGER", nullable: false),
                    QuantitySold = table.Column<int>(type: "INTEGER", nullable: false),
                    QuantityReturned = table.Column<int>(type: "INTEGER", nullable: false),
                    CostPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RetailPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsignmentItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsignmentItems_Consignments_ConsignmentId",
                        column: x => x.ConsignmentId,
                        principalTable: "Consignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConsignmentItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Deliveries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrderId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClientTenantId = table.Column<int>(type: "INTEGER", nullable: false),
                    DeliveryNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PlannedDeliveryDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DeliveryAddress = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ContactPerson = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ContactPhone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DeliveryNotes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    InternalNotes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DepartureTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ArrivalTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletionTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReceivedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    DeliveryFeedback = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DeliveryLatitude = table.Column<decimal>(type: "decimal(10,8)", nullable: true),
                    DeliveryLongitude = table.Column<decimal>(type: "decimal(11,8)", nullable: true),
                    DeliveryPhotoUrls = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deliveries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Deliveries_ClientTenants_ClientTenantId",
                        column: x => x.ClientTenantId,
                        principalTable: "ClientTenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Deliveries_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrderId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConsignmentSales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ConsignmentId = table.Column<int>(type: "INTEGER", nullable: false),
                    ConsignmentItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    SaleDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    QuantitySold = table.Column<int>(type: "INTEGER", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalSaleAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StoreAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BernardAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SaleReference = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsignmentSales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsignmentSales_ConsignmentItems_ConsignmentItemId",
                        column: x => x.ConsignmentItemId,
                        principalTable: "ConsignmentItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConsignmentSales_Consignments_ConsignmentId",
                        column: x => x.ConsignmentId,
                        principalTable: "Consignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DeliveryId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    QuantityDelivered = table.Column<int>(type: "INTEGER", nullable: false),
                    QuantityOrdered = table.Column<int>(type: "INTEGER", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryItems_Deliveries_DeliveryId",
                        column: x => x.DeliveryId,
                        principalTable: "Deliveries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeliveryItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Albums",
                columns: new[] { "Id", "CreatedAt", "Description", "Name", "Slug", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 10, 27, 18, 55, 13, 773, DateTimeKind.Utc).AddTicks(8500), "Product catalog photos", "Products", "products", null },
                    { 2, new DateTime(2025, 10, 27, 18, 55, 13, 773, DateTimeKind.Utc).AddTicks(8660), "Customer sales receipts", "Sales Receipts", "sales-receipts", null },
                    { 3, new DateTime(2025, 10, 27, 18, 55, 13, 773, DateTimeKind.Utc).AddTicks(8660), "Supplier purchase receipts", "Purchase Receipts", "purchase-receipts", null },
                    { 4, new DateTime(2025, 10, 27, 18, 55, 13, 773, DateTimeKind.Utc).AddTicks(8660), "Client invoices and documentation", "Invoices", "invoices", null },
                    { 5, new DateTime(2025, 10, 27, 18, 55, 13, 773, DateTimeKind.Utc).AddTicks(8660), "Supplier documentation and photos", "Suppliers", "suppliers", null },
                    { 6, new DateTime(2025, 10, 27, 18, 55, 13, 773, DateTimeKind.Utc).AddTicks(8660), "Customer documentation and photos", "Customers", "customers", null },
                    { 7, new DateTime(2025, 10, 27, 18, 55, 13, 773, DateTimeKind.Utc).AddTicks(8660), "Miscellaneous photos and documents", "Misc", "misc", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Albums_Slug",
                table: "Albums",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityType",
                table: "AuditLogs",
                column: "EntityType");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Module",
                table: "AuditLogs",
                column: "Module");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Severity",
                table: "AuditLogs",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Timestamp",
                table: "AuditLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessAlerts_AlertType",
                table: "BusinessAlerts",
                column: "AlertType");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessAlerts_ClientTenant",
                table: "BusinessAlerts",
                column: "ClientTenant");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessAlerts_CreatedAt",
                table: "BusinessAlerts",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessAlerts_IsResolved",
                table: "BusinessAlerts",
                column: "IsResolved");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessAlerts_Severity",
                table: "BusinessAlerts",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_ClientProducts_ClientTenantId_ProductId",
                table: "ClientProducts",
                columns: new[] { "ClientTenantId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientProducts_ProductId",
                table: "ClientProducts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientTenantPriceLists_ClientTenantId_PriceListId",
                table: "ClientTenantPriceLists",
                columns: new[] { "ClientTenantId", "PriceListId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientTenantPriceLists_PriceListId",
                table: "ClientTenantPriceLists",
                column: "PriceListId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientTenants_LoginUsername",
                table: "ClientTenants",
                column: "LoginUsername",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientTenants_TenantCode",
                table: "ClientTenants",
                column: "TenantCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConsignmentItems_ConsignmentId",
                table: "ConsignmentItems",
                column: "ConsignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsignmentItems_ProductId",
                table: "ConsignmentItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Consignments_ClientTenantId",
                table: "Consignments",
                column: "ClientTenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Consignments_ConsignmentDate",
                table: "Consignments",
                column: "ConsignmentDate");

            migrationBuilder.CreateIndex(
                name: "IX_Consignments_ConsignmentNumber",
                table: "Consignments",
                column: "ConsignmentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Consignments_Status",
                table: "Consignments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ConsignmentSales_ConsignmentId",
                table: "ConsignmentSales",
                column: "ConsignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsignmentSales_ConsignmentItemId",
                table: "ConsignmentSales",
                column: "ConsignmentItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsignmentSales_SaleDate",
                table: "ConsignmentSales",
                column: "SaleDate");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Email",
                table: "Customers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_ClientTenantId",
                table: "Deliveries",
                column: "ClientTenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_DeliveryDate",
                table: "Deliveries",
                column: "DeliveryDate");

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_DeliveryNumber",
                table: "Deliveries",
                column: "DeliveryNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_OrderId",
                table: "Deliveries",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_Status",
                table: "Deliveries",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryItems_DeliveryId",
                table: "DeliveryItems",
                column: "DeliveryId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryItems_ProductId",
                table: "DeliveryItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductId",
                table: "OrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ClientTenantId",
                table: "Orders",
                column: "ClientTenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerId",
                table: "Orders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Photos_AlbumId",
                table: "Photos",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_Photos_UploadedUtc",
                table: "Photos",
                column: "UploadedUtc");

            migrationBuilder.CreateIndex(
                name: "IX_PriceListItems_PriceListId_ProductId",
                table: "PriceListItems",
                columns: new[] { "PriceListId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PriceListItems_ProductId",
                table: "PriceListItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceLists_IsActive",
                table: "PriceLists",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PriceLists_IsDefault",
                table: "PriceLists",
                column: "IsDefault");

            migrationBuilder.CreateIndex(
                name: "IX_SystemMetrics_ClientTenant",
                table: "SystemMetrics",
                column: "ClientTenant");

            migrationBuilder.CreateIndex(
                name: "IX_SystemMetrics_MetricName",
                table: "SystemMetrics",
                column: "MetricName");

            migrationBuilder.CreateIndex(
                name: "IX_SystemMetrics_MetricType",
                table: "SystemMetrics",
                column: "MetricType");

            migrationBuilder.CreateIndex(
                name: "IX_SystemMetrics_Severity",
                table: "SystemMetrics",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_SystemMetrics_Timestamp",
                table: "SystemMetrics",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "BusinessAlerts");

            migrationBuilder.DropTable(
                name: "ClientProducts");

            migrationBuilder.DropTable(
                name: "ClientTenantPriceLists");

            migrationBuilder.DropTable(
                name: "ConsignmentSales");

            migrationBuilder.DropTable(
                name: "DeliveryItems");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Photos");

            migrationBuilder.DropTable(
                name: "PriceListItems");

            migrationBuilder.DropTable(
                name: "Suppliers");

            migrationBuilder.DropTable(
                name: "SystemMetrics");

            migrationBuilder.DropTable(
                name: "ConsignmentItems");

            migrationBuilder.DropTable(
                name: "Deliveries");

            migrationBuilder.DropTable(
                name: "Albums");

            migrationBuilder.DropTable(
                name: "PriceLists");

            migrationBuilder.DropTable(
                name: "Consignments");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "ClientTenants");

            migrationBuilder.DropTable(
                name: "Customers");
        }
    }
}
