# Limpieza de Migraciones - VHouse

## Estado Actual

✅ **Completado:** Backup del esquema actual de base de datos
✅ **Completado:** Eliminación de todos los archivos de migración existentes

## Próximos Pasos

Para completar la limpieza de migraciones, necesitas ejecutar los siguientes comandos en tu entorno local con .NET instalado:

### 1. Crear Migración Inicial Limpia

```bash
cd VHouse
dotnet ef migrations add InitialMigration
```

### 2. Aplicar la Migración (si tienes una base de datos limpia)

```bash
dotnet ef database update
```

## Archivos Eliminados

Se eliminaron las siguientes migraciones:

- `20250312182602_InitPostgres.cs`
- `20250312182602_InitPostgres.Designer.cs`
- `20250313214248_FixDeliveryDateToUtc.cs`
- `20250313214248_FixDeliveryDateToUtc.Designer.cs`
- `20250313221409_AddQuantityToOrderItem.cs`
- `20250313221409_AddQuantityToOrderItem.Designer.cs`
- `20250316233658_AddCustomerInventoryRelation.cs`
- `20250316233658_AddCustomerInventoryRelation.Designer.cs`
- `20250318203230_AddInvoicesAndInventoryUpdates.cs`
- `20250318203230_AddInvoicesAndInventoryUpdates.Designer.cs`
- `20250322001521_AddNullableInvoiceIdAndGeneralInventorySupport.cs`
- `20250322001521_AddNullableInvoiceIdAndGeneralInventorySupport.Designer.cs`
- `20250323233414_FixCustomerInventoryRelation.cs`
- `20250323233414_FixCustomerInventoryRelation.Designer.cs`
- `ApplicationDbContextModelSnapshot.cs`

## Esquema Actual de Base de Datos

El esquema actual incluye las siguientes entidades:

- **Customer**: Clientes con inventario individual
- **Product**: Productos con diferentes tipos de precio
- **Order/OrderItem**: Sistema de pedidos
- **Inventory/InventoryItem**: Gestión de inventario por cliente
- **Invoice**: Facturas de proveedores

## Relaciones Configuradas

- Customer ↔ Inventory (One-to-One)
- Inventory → InventoryItem (One-to-Many)
- InventoryItem → Invoice (Many-to-One, nullable)
- Order → OrderItem (One-to-Many)
- Customer → Order (One-to-Many)

## Nota Importante

⚠️ **Advertencia**: Si tienes datos en producción, asegúrate de hacer un backup completo antes de aplicar la nueva migración inicial, ya que esto podría afectar la estructura existente de la base de datos.