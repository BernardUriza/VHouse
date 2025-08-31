-- Script para poblar base de datos con cliente Mona la Dona y sus productos
-- Por los animales. Por la liberación. Por un mundo sin sufrimiento.

-- Insertar cliente Mona la Dona
INSERT INTO ClientTenants (TenantName, TenantCode, BusinessName, Description, ContactPerson, Email, Phone, LoginUsername, LoginPassword, IsActive, CreatedAt)
VALUES (
    'Mona la Dona',
    'MONA_DONA',
    'Mona la Dona - Donas Veganas Premium',
    'Especialistas en donas veganas artesanales que derriten corazones y no lastiman animales 🍩',
    'Ana Sofía González',
    'ana@monaladorona.com.mx',
    '+52 33 1234 5678',
    'monadona',
    'vegandoñas2024#',
    1,
    datetime('now')
);

-- Obtener productos existentes e insertar productos específicos para Mona la Dona
-- 1. Leche de Avena
INSERT OR IGNORE INTO Products (ProductName, Description, PriceCost, PriceRetail, PriceSuggested, PricePublic, CategoryName, Brand, Weight, ExpirationDate, SKU, ImageUrl, Emoji) 
VALUES ('Leche de Avena Premium 1L', 'Leche de avena cremosa perfecta para donas esponjosas sin lácteos', 25.50, 45.00, 42.00, 38.00, 'Lácteos Veganos', 'Oat Dreams', '1L', date('now', '+45 days'), 'OATMILK-1L-001', '/images/oat-milk.jpg', '🥛');

-- 2. Mantequilla Vegana
INSERT OR IGNORE INTO Products (ProductName, Description, PriceCost, PriceRetail, PriceSuggested, PricePublic, CategoryName, Brand, Weight, ExpirationDate, SKU, ImageUrl, Emoji) 
VALUES ('Mantequilla Vegana Natural 250g', 'Mantequilla 100% vegetal ideal para masa de donas', 35.00, 62.00, 58.00, 52.00, 'Grasas Vegetales', 'Plant Butter Co.', '250g', date('now', '+60 days'), 'VBUTTER-250G-001', '/images/vegan-butter.jpg', '🧈');

-- 3. Queso Crema Vegano Q Foods
INSERT OR IGNORE INTO Products (ProductName, Description, PriceCost, PriceRetail, PriceSuggested, PricePublic, CategoryName, Brand, Weight, ExpirationDate, SKU, ImageUrl, Emoji) 
VALUES ('Queso Crema Q Foods 200g', 'Queso crema vegano Q Foods para rellenos cremosos', 42.00, 72.00, 68.00, 62.00, 'Quesos Veganos', 'Q Foods', '200g', date('now', '+30 days'), 'QFOODS-CREAM-200G', '/images/q-foods-cream.jpg', '🧀');

-- 4. Queso Amarillo Q Foods  
INSERT OR IGNORE INTO Products (ProductName, Description, PriceCost, PriceRetail, PriceSuggested, PricePublic, CategoryName, Brand, Weight, ExpirationDate, SKU, ImageUrl, Emoji) 
VALUES ('Queso Amarillo Q Foods 200g', 'Queso amarillo vegano Q Foods perfecto para donas saladas', 38.00, 68.00, 64.00, 58.00, 'Quesos Veganos', 'Q Foods', '200g', date('now', '+30 days'), 'QFOODS-YELLOW-200G', '/images/q-foods-yellow.jpg', '🟡');

-- 5. Salchicha Vegana
INSERT OR IGNORE INTO Products (ProductName, Description, PriceCost, PriceRetail, PriceSuggested, PricePublic, CategoryName, Brand, Weight, ExpirationDate, SKU, ImageUrl, Emoji) 
VALUES ('Salchicha Vegana Ahumada 200g', 'Salchicha vegana ahumada para donas gourmet saladas', 48.00, 82.00, 78.00, 72.00, 'Embutidos Veganos', 'VeganSausage Co.', '200g', date('now', '+25 days'), 'VSAUSAGE-SMOKED-200G', '/images/vegan-sausage.jpg', '🌭');

-- 6. Jamón Meathical
INSERT OR IGNORE INTO Products (ProductName, Description, PriceCost, PriceRetail, PriceSuggested, PricePublic, CategoryName, Brand, Weight, ExpirationDate, SKU, ImageUrl, Emoji) 
VALUES ('Jamón Vegano Meathical 150g', 'Jamón vegano Meathical premium para donas de desayuno', 52.00, 88.00, 84.00, 78.00, 'Embutidos Veganos', 'Meathical', '150g', date('now', '+20 days'), 'MEATHICAL-HAM-150G', '/images/meathical-ham.jpg', '🥓');

-- 7. Cajeta sin Azúcar
INSERT OR IGNORE INTO Products (ProductName, Description, PriceCost, PriceRetail, PriceSuggested, PricePublic, CategoryName, Brand, Weight, ExpirationDate, SKU, ImageUrl, Emoji) 
VALUES ('Cajeta Vegana Sin Azúcar 300ml', 'Cajeta vegana sin azúcar refinada para glaseados naturales', 55.00, 92.00, 88.00, 82.00, 'Dulces Veganos', 'Sweet Vegan', '300ml', date('now', '+90 days'), 'VCAJETA-NOSUGAR-300ML', '/images/vegan-cajeta.jpg', '🍮');

-- 8. Setas Premium
INSERT OR IGNORE INTO Products (ProductName, Description, PriceCost, PriceRetail, PriceSuggested, PricePublic, CategoryName, Brand, Weight, ExpirationDate, SKU, ImageUrl, Emoji) 
VALUES ('Setas Mixtas Premium 250g', 'Mezcla de setas frescas para donas saladas gourmet', 28.00, 48.00, 45.00, 42.00, 'Hongos Frescos', 'Fungi Fresh', '250g', date('now', '+7 days'), 'MUSHROOMS-MIX-250G', '/images/premium-mushrooms.jpg', '🍄');

-- 9. Kombucha LAffectuosyta
INSERT OR IGNORE INTO Products (ProductName, Description, PriceCost, PriceRetail, PriceSuggested, PricePublic, CategoryName, Brand, Weight, ExpirationDate, SKU, ImageUrl, Emoji) 
VALUES ('Kombucha LAffectuosyta 500ml', 'Kombucha probiótica artesanal perfecta para acompañar donas', 45.00, 75.00, 72.00, 68.00, 'Bebidas Fermentadas', 'LAffectuosyta', '500ml', date('now', '+21 days'), 'KOMBUCHA-LAFF-500ML', '/images/kombucha-laff.jpg', '🫧');

-- Asignar todos los productos a Mona la Dona con precios especiales B2B
INSERT INTO ClientProducts (ClientTenantId, ProductId, CustomPrice, MinOrderQuantity, IsAvailable, AssignedAt)
SELECT 
    ct.Id as ClientTenantId,
    p.Id as ProductId,
    p.PricePublic as CustomPrice, -- Usando precio público como precio B2B especial
    CASE 
        WHEN p.ProductName LIKE '%Leche%' THEN 2
        WHEN p.ProductName LIKE '%Mantequilla%' THEN 3
        WHEN p.ProductName LIKE '%Queso%' THEN 2
        WHEN p.ProductName LIKE '%Salchicha%' THEN 1
        WHEN p.ProductName LIKE '%Jamón%' THEN 1
        WHEN p.ProductName LIKE '%Cajeta%' THEN 1
        WHEN p.ProductName LIKE '%Setas%' THEN 2
        WHEN p.ProductName LIKE '%Kombucha%' THEN 3
        ELSE 1
    END as MinOrderQuantity,
    1 as IsAvailable,
    datetime('now') as AssignedAt
FROM ClientTenants ct
CROSS JOIN Products p
WHERE ct.TenantCode = 'MONA_DONA'
AND p.ProductName IN (
    'Leche de Avena Premium 1L',
    'Mantequilla Vegana Natural 250g', 
    'Queso Crema Q Foods 200g',
    'Queso Amarillo Q Foods 200g',
    'Salchicha Vegana Ahumada 200g',
    'Jamón Vegano Meathical 150g',
    'Cajeta Vegana Sin Azúcar 300ml',
    'Setas Mixtas Premium 250g',
    'Kombucha LAffectuosyta 500ml'
);