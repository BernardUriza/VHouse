# 🎨 VHouse CSS Architecture
## Por los animales. Por la liberación. Por un mundo sin sufrimiento.

### 📁 Estructura Modular

```
css/
├── main.css                 # Entry point - importa todo en orden correcto
├── base/
│   ├── variables.css        # Variables CSS globales (colores, espaciado, etc.)
│   ├── reset.css           # Reset básico
│   └── typography.css      # Sistema tipográfico
├── layout/
│   ├── grid.css           # Sistema de layout (.page, .content, etc.)
│   ├── navigation.css     # Navegación y menú
│   └── sidebar.css        # Sidebar específico
├── components/
│   ├── buttons.css        # Botones (.btn, .btn-primary, etc.)
│   ├── forms.css          # Formularios (.form-control, .form-label, etc.)
│   ├── cards.css          # Tarjetas (.card, .card-header, etc.)
│   ├── tables.css         # Tablas (.table, etc.)
│   ├── alerts.css         # Alertas (.alert-success, etc.)
│   └── modals.css         # Modales (.modal-content, etc.)
├── pages/
│   ├── pos.css           # POS futurista (.pos-page .futuristic-pos)
│   ├── orders.css        # Orders alto contraste (.orders-page)
│   └── suppliers.css     # Suppliers (.suppliers-page)
├── themes/
│   └── dark-mode.css     # Sistema de modo oscuro [data-theme="dark"]
└── utilities/
    ├── spacing.css       # Utilities de espaciado (.m-*, .p-*, etc.)
    └── responsive.css    # Media queries y responsive
```

### 🎯 Orden de Carga (Especificidad)

1. **Variables** - Más baja prioridad
2. **Reset & Base** - Fundación
3. **Layout** - Estructura
4. **Components** - Componentes reutilizables
5. **Pages** - Estilos específicos (más alta prioridad)
6. **Themes** - Overrides de tema
7. **Utilities** - Última palabra

### 🔧 Sistema de Especificidad

#### ✅ CORRECTO - Sin Sobreescrituras:
```css
/* Components/buttons.css */
.btn { ... }
.btn-primary { ... }

/* Pages/pos.css - Prefijo para especificidad */
.pos-page .btn-primary {
  /* Overrides específicos para POS */
}

/* Pages/orders.css - Prefijo diferente */  
.orders-page .btn-primary {
  /* Overrides específicos para Orders */
}
```

#### ❌ INCORRECTO - Sobreescrituras conflictivas:
```css
.btn-primary { background: blue; }    /* Base */
.btn-primary { background: red; }     /* Conflicto! */
```

### 🌙 Sistema de Modo Oscuro

Usa variables CSS que cambian automáticamente:

```css
/* Variables que cambian según tema */
:root {
  --bg-primary: #ffffff;    /* Modo claro */
  --text-primary: #111827;
}

[data-theme="dark"] {
  --bg-primary: #000000;    /* Modo oscuro */
  --text-primary: #ffffff;
}

/* Componentes usan las variables */
.card {
  background: var(--bg-primary);  /* Automático! */
  color: var(--text-primary);
}
```

### 🎨 Variables Veganas

```css
:root {
  /* Colores temáticos */
  --vegan-green: #16a34a;
  --plant-green: #10b981;
  --vegan-green-light: #bbf7d0;
  
  /* Espaciado consistente */
  --space-xs: 0.25rem;
  --space-sm: 0.5rem;
  --space-md: 1rem;
  --space-lg: 1.5rem;
  --space-xl: 2rem;
  
  /* Tipografía */
  --font-family: 'Inter', system-ui;
  --font-mono: 'JetBrains Mono', monospace;
}
```

### 📱 Sistema Responsive

Los breakpoints están definidos en `utilities/responsive.css`:

```css
@media (max-width: 768px) {
  .pos-page .pos-grid {
    grid-template-columns: 1fr !important;
  }
}
```

### 🚀 Cómo Agregar Nuevos Estilos

#### 1. Para Componente Nuevo:
- Crear `components/mi-componente.css`
- Agregar `@import` en `main.css`
- Usar variables de `base/variables.css`

#### 2. Para Página Nueva:
- Crear `pages/mi-pagina.css`
- Usar prefijo `.mi-pagina` para especificidad
- Agregar clases específicas solo si es necesario

#### 3. Para Override Específico:
```css
/* ✅ Correcto */
.orders-page .btn-primary {
  /* Override específico para orders */
}

/* ❌ Incorrecto */
.btn-primary {
  /* Afecta TODA la aplicación */
}
```

### 🎯 Mejores Prácticas

1. **Siempre usar variables** - No hardcodear valores
2. **Prefijos para especificidad** - `.page-name .component`
3. **Mobile first** - Diseñar para móvil primero
4. **Modo oscuro by design** - Usar variables que cambien automáticamente
5. **Transiciones suaves** - `var(--transition-base)` para UX fluida

### 🧪 Testing de Estilos

Para verificar que no hay conflictos:

1. **Probar modo claro/oscuro** - Toggle debe funcionar perfectamente
2. **Probar en mobile** - Responsive en todas las páginas
3. **Verificar especificidad** - No debería haber `!important` innecesarios
4. **Componentes aislados** - Cada página debe verse correcta

### 🌱 Filosofía del Sistema

Este CSS está diseñado para **escalabilidad sin conflictos**:
- **Modular**: Cada archivo tiene una responsabilidad
- **Predecible**: El orden de carga previene sobreescrituras
- **Mantenible**: Fácil agregar features sin romper existente
- **Vegano**: Colores y diseño alineados con la misión

---

*"Por los animales. Por la liberación. Por un mundo sin sufrimiento."* 🌱