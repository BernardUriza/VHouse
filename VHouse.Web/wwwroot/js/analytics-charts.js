// Phase 7: Analytics Dashboard Chart Functionality

// Chart.js initialization and rendering functions
window.analyticsCharts = {
    charts: {},
    
    // Initialize Chart.js
    init: function() {
        if (typeof Chart === 'undefined') {
            console.warn('Chart.js not loaded. Loading from CDN...');
            const script = document.createElement('script');
            script.src = 'https://cdn.jsdelivr.net/npm/chart.js@4.2.1/dist/chart.umd.js';
            script.onload = () => {
                console.log('Chart.js loaded successfully');
            };
            document.head.appendChild(script);
        }
    },
    
    // Render chart with given configuration
    renderChart: function(canvasId, type, data, options) {
        try {
            const ctx = document.getElementById(canvasId);
            if (!ctx) {
                console.error(`Canvas element ${canvasId} not found`);
                return;
            }
            
            // Destroy existing chart if it exists
            if (this.charts[canvasId]) {
                this.charts[canvasId].destroy();
            }
            
            // Default options
            const defaultOptions = {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: true,
                        position: 'top',
                        labels: {
                            usePointStyle: true,
                            padding: 20
                        }
                    },
                    tooltip: {
                        backgroundColor: 'rgba(0, 0, 0, 0.8)',
                        titleColor: '#fff',
                        bodyColor: '#fff',
                        borderColor: '#667eea',
                        borderWidth: 1,
                        cornerRadius: 8,
                        displayColors: true,
                        mode: 'index',
                        intersect: false
                    }
                },
                scales: this.getScaleConfig(type),
                animation: {
                    duration: 1500,
                    easing: 'easeInOutQuart'
                }
            };
            
            // Merge options
            const finalOptions = this.mergeDeep(defaultOptions, options || {});
            
            // Create new chart
            this.charts[canvasId] = new Chart(ctx, {
                type: type,
                data: data,
                options: finalOptions
            });
            
            console.log(`Chart ${canvasId} rendered successfully`);
            
        } catch (error) {
            console.error(`Error rendering chart ${canvasId}:`, error);
        }
    },
    
    // Render gauge chart
    renderGauge: function(containerId, value, max) {
        try {
            const container = document.getElementById(containerId);
            if (!container) {
                console.error(`Container ${containerId} not found`);
                return;
            }
            
            const percentage = (value / max) * 100;
            let color = '#48bb78'; // Green
            
            if (percentage < 30) color = '#f56565'; // Red
            else if (percentage < 70) color = '#ed8936'; // Orange
            
            // Create gauge using CSS conic-gradient
            const degree = (percentage / 100) * 360;
            container.style.background = `conic-gradient(
                ${color} 0deg ${degree}deg,
                #e2e8f0 ${degree}deg 360deg
            )`;
            
            // Update value display
            const valueElement = container.querySelector('.gauge-value');
            if (valueElement) {
                valueElement.textContent = `${value}%`;
            }
            
            console.log(`Gauge ${containerId} rendered successfully`);
            
        } catch (error) {
            console.error(`Error rendering gauge ${containerId}:`, error);
        }
    },
    
    // Get scale configuration based on chart type
    getScaleConfig: function(type) {
        switch (type) {
            case 'line':
            case 'bar':
                return {
                    x: {
                        grid: {
                            display: true,
                            color: 'rgba(0, 0, 0, 0.05)'
                        },
                        ticks: {
                            color: '#6c757d'
                        }
                    },
                    y: {
                        beginAtZero: true,
                        grid: {
                            display: true,
                            color: 'rgba(0, 0, 0, 0.05)'
                        },
                        ticks: {
                            color: '#6c757d'
                        }
                    }
                };
            case 'pie':
            case 'doughnut':
                return {};
            default:
                return {};
        }
    },
    
    // Deep merge objects
    mergeDeep: function(target, source) {
        const output = Object.assign({}, target);
        if (this.isObject(target) && this.isObject(source)) {
            Object.keys(source).forEach(key => {
                if (this.isObject(source[key])) {
                    if (!(key in target))
                        Object.assign(output, { [key]: source[key] });
                    else
                        output[key] = this.mergeDeep(target[key], source[key]);
                } else {
                    Object.assign(output, { [key]: source[key] });
                }
            });
        }
        return output;
    },
    
    // Check if value is object
    isObject: function(item) {
        return item && typeof item === 'object' && !Array.isArray(item);
    },
    
    // Update chart data
    updateChart: function(canvasId, newData) {
        try {
            const chart = this.charts[canvasId];
            if (chart) {
                chart.data = newData;
                chart.update('active');
                console.log(`Chart ${canvasId} updated successfully`);
            } else {
                console.error(`Chart ${canvasId} not found for update`);
            }
        } catch (error) {
            console.error(`Error updating chart ${canvasId}:`, error);
        }
    },
    
    // Destroy chart
    destroyChart: function(canvasId) {
        try {
            if (this.charts[canvasId]) {
                this.charts[canvasId].destroy();
                delete this.charts[canvasId];
                console.log(`Chart ${canvasId} destroyed successfully`);
            }
        } catch (error) {
            console.error(`Error destroying chart ${canvasId}:`, error);
        }
    },
    
    // Resize all charts
    resizeCharts: function() {
        Object.values(this.charts).forEach(chart => {
            if (chart && typeof chart.resize === 'function') {
                chart.resize();
            }
        });
    }
};

// Global functions for Blazor interop
window.renderChart = function(canvasId, type, data, options) {
    window.analyticsCharts.renderChart(canvasId, type, data, options);
};

window.renderGauge = function(containerId, value, max) {
    window.analyticsCharts.renderGauge(containerId, value, max || 100);
};

window.updateChart = function(canvasId, newData) {
    window.analyticsCharts.updateChart(canvasId, newData);
};

window.destroyChart = function(canvasId) {
    window.analyticsCharts.destroyChart(canvasId);
};

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    window.analyticsCharts.init();
    
    // Handle window resize
    window.addEventListener('resize', function() {
        window.analyticsCharts.resizeCharts();
    });
});

// Analytics Dashboard specific functionality
window.analyticsDashboard = {
    // Auto-refresh functionality
    autoRefreshInterval: null,
    
    startAutoRefresh: function(intervalSeconds, refreshCallback) {
        this.stopAutoRefresh();
        this.autoRefreshInterval = setInterval(() => {
            if (typeof refreshCallback === 'function') {
                refreshCallback();
            }
        }, intervalSeconds * 1000);
        console.log(`Auto-refresh started with ${intervalSeconds}s interval`);
    },
    
    stopAutoRefresh: function() {
        if (this.autoRefreshInterval) {
            clearInterval(this.autoRefreshInterval);
            this.autoRefreshInterval = null;
            console.log('Auto-refresh stopped');
        }
    },
    
    // Export dashboard as image
    exportDashboard: function(filename) {
        try {
            const dashboard = document.querySelector('.analytics-dashboard');
            if (!dashboard) {
                console.error('Dashboard element not found');
                return;
            }
            
            // Use html2canvas if available
            if (typeof html2canvas !== 'undefined') {
                html2canvas(dashboard).then(canvas => {
                    const link = document.createElement('a');
                    link.download = filename || 'dashboard-export.png';
                    link.href = canvas.toDataURL();
                    link.click();
                });
            } else {
                console.warn('html2canvas not available for export functionality');
                // Fallback: print dialog
                window.print();
            }
        } catch (error) {
            console.error('Error exporting dashboard:', error);
        }
    },
    
    // Toggle fullscreen mode
    toggleFullscreen: function(elementId) {
        try {
            const element = elementId ? document.getElementById(elementId) : document.documentElement;
            
            if (!document.fullscreenElement) {
                element.requestFullscreen().then(() => {
                    console.log('Entered fullscreen mode');
                }).catch(err => {
                    console.error('Error entering fullscreen:', err);
                });
            } else {
                document.exitFullscreen().then(() => {
                    console.log('Exited fullscreen mode');
                }).catch(err => {
                    console.error('Error exiting fullscreen:', err);
                });
            }
        } catch (error) {
            console.error('Fullscreen not supported:', error);
        }
    },
    
    // Format numbers for display
    formatNumber: function(value, type = 'number') {
        try {
            const num = parseFloat(value);
            if (isNaN(num)) return value;
            
            switch (type) {
                case 'currency':
                    return new Intl.NumberFormat('en-US', {
                        style: 'currency',
                        currency: 'USD'
                    }).format(num);
                case 'percentage':
                    return new Intl.NumberFormat('en-US', {
                        style: 'percent',
                        minimumFractionDigits: 1
                    }).format(num / 100);
                case 'compact':
                    return new Intl.NumberFormat('en-US', {
                        notation: 'compact',
                        compactDisplay: 'short'
                    }).format(num);
                default:
                    return new Intl.NumberFormat('en-US').format(num);
            }
        } catch (error) {
            console.error('Error formatting number:', error);
            return value;
        }
    },
    
    // Generate color palette
    generateColors: function(count, opacity = 1) {
        const colors = [];
        const baseColors = [
            [54, 162, 235],   // Blue
            [255, 99, 132],   // Red
            [255, 206, 86],   // Yellow
            [75, 192, 192],   // Green
            [153, 102, 255],  // Purple
            [255, 159, 64],   // Orange
            [199, 199, 199],  // Grey
            [83, 102, 255],   // Indigo
            [255, 99, 255],   // Pink
            [99, 255, 132]    // Lime
        ];
        
        for (let i = 0; i < count; i++) {
            const colorIndex = i % baseColors.length;
            const [r, g, b] = baseColors[colorIndex];
            colors.push(`rgba(${r}, ${g}, ${b}, ${opacity})`);
        }
        
        return colors;
    }
};

// Expose global functions
window.startAutoRefresh = function(intervalSeconds, refreshCallback) {
    window.analyticsDashboard.startAutoRefresh(intervalSeconds, refreshCallback);
};

window.stopAutoRefresh = function() {
    window.analyticsDashboard.stopAutoRefresh();
};

window.exportDashboard = function(filename) {
    window.analyticsDashboard.exportDashboard(filename);
};

window.toggleFullscreen = function(elementId) {
    window.analyticsDashboard.toggleFullscreen(elementId);
};

window.formatNumber = function(value, type) {
    return window.analyticsDashboard.formatNumber(value, type);
};

console.log('Analytics dashboard JavaScript loaded successfully');