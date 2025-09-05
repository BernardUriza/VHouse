// VHouse Global JavaScript Functions
// Por los animales. Por la liberación. Por un mundo sin sufrimiento.

// Global variable to store current POS component reference
window.vhousePOS = null;

// Register POS component for chatbot callbacks
window.registerPOSComponent = (dotNetReference) => {
    window.vhousePOS = dotNetReference;
};

// Global function to add products to cart from chatbot
window.addProductToCart = async (productId, productName) => {
    if (window.vhousePOS) {
        try {
            await window.vhousePOS.invokeMethodAsync('AddProductFromChatbot', productId, productName);
        } catch (error) {
            console.error('Error adding product to cart:', error);
            showToast(`❌ Error agregando ${productName}`, 'error');
        }
    } else {
        console.warn('POS component not registered');
        if (confirm(`¿Agregar ${productName} al carrito?`)) {
            showToast(`${productName} se agregaría al carrito (función no disponible en esta página)`, 'info');
        }
    }
};

// Toast notification system
window.showToast = (message, type = 'success', duration = 3000) => {
    // Remove existing toasts
    const existingToasts = document.querySelectorAll('.vhouse-toast');
    existingToasts.forEach(toast => toast.remove());
    
    // Create toast element
    const toast = document.createElement('div');
    toast.className = `vhouse-toast vhouse-toast-${type}`;
    toast.innerHTML = `
        <div class="vhouse-toast-content">
            <span class="vhouse-toast-message">${message}</span>
            <button class="vhouse-toast-close" onclick="this.parentElement.parentElement.remove()">×</button>
        </div>
    `;
    
    // Add toast styles if not already added
    if (!document.getElementById('vhouse-toast-styles')) {
        const styles = document.createElement('style');
        styles.id = 'vhouse-toast-styles';
        styles.textContent = `
            .vhouse-toast {
                position: fixed;
                top: 20px;
                right: 20px;
                z-index: 9999;
                min-width: 300px;
                max-width: 500px;
                border-radius: 12px;
                box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);
                backdrop-filter: blur(20px);
                animation: vhouse-toast-slide-in 0.3s ease-out;
            }
            
            .vhouse-toast-success {
                background: rgba(16, 185, 129, 0.9);
                border: 1px solid rgba(16, 185, 129, 0.5);
            }
            
            .vhouse-toast-error {
                background: rgba(239, 68, 68, 0.9);
                border: 1px solid rgba(239, 68, 68, 0.5);
            }
            
            .vhouse-toast-info {
                background: rgba(59, 130, 246, 0.9);
                border: 1px solid rgba(59, 130, 246, 0.5);
            }
            
            .vhouse-toast-content {
                display: flex;
                align-items: center;
                justify-content: space-between;
                padding: 16px 20px;
                color: white;
                font-weight: 500;
            }
            
            .vhouse-toast-message {
                flex: 1;
                font-size: 14px;
                line-height: 1.4;
            }
            
            .vhouse-toast-close {
                background: none;
                border: none;
                color: white;
                font-size: 18px;
                font-weight: bold;
                cursor: pointer;
                margin-left: 12px;
                opacity: 0.7;
                transition: opacity 0.2s;
            }
            
            .vhouse-toast-close:hover {
                opacity: 1;
            }
            
            @keyframes vhouse-toast-slide-in {
                from {
                    transform: translateX(100%);
                    opacity: 0;
                }
                to {
                    transform: translateX(0);
                    opacity: 1;
                }
            }
            
            @keyframes vhouse-toast-slide-out {
                from {
                    transform: translateX(0);
                    opacity: 1;
                }
                to {
                    transform: translateX(100%);
                    opacity: 0;
                }
            }
        `;
        document.head.appendChild(styles);
    }
    
    // Add to page
    document.body.appendChild(toast);
    
    // Auto-remove after duration
    setTimeout(() => {
        if (toast.parentElement) {
            toast.style.animation = 'vhouse-toast-slide-out 0.3s ease-in';
            setTimeout(() => toast.remove(), 300);
        }
    }, duration);
};

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    console.log('VHouse Global JS loaded - Por los animales 🌱');
});