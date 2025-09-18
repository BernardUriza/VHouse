// Modern Markdown Viewer JavaScript Enhancements for VHouse
// By Bernard Orozco - For Animal Liberation 🌱

window.MarkdownViewer = {
    // Initialize all markdown features
    init: function () {
        this.initMermaidZoom();
        this.initSmoothScrolling();
        this.initCodeHighlighting();
        this.initTableOfContents();
        this.initCopyButtons();
    },

    // Mermaid Zoom Functionality with Modal
    initMermaidZoom: function () {
        // Create modal container if not exists
        if (!document.getElementById('mermaid-modal')) {
            const modal = document.createElement('div');
            modal.id = 'mermaid-modal';
            modal.className = 'mermaid-modal';
            modal.innerHTML = `
                <div class="mermaid-modal-content">
                    <button class="mermaid-modal-close" onclick="MarkdownViewer.closeMermaidModal()">
                        <span>✕</span>
                    </button>
                    <button class="mermaid-zoom-in" onclick="MarkdownViewer.zoomIn()">
                        <span>🔍+</span>
                    </button>
                    <button class="mermaid-zoom-out" onclick="MarkdownViewer.zoomOut()">
                        <span>🔍-</span>
                    </button>
                    <button class="mermaid-zoom-reset" onclick="MarkdownViewer.resetZoom()">
                        <span>🔄</span>
                    </button>
                    <div class="mermaid-modal-body">
                        <div id="mermaid-modal-diagram"></div>
                    </div>
                </div>
            `;
            document.body.appendChild(modal);
        }

        // Add zoom buttons to all mermaid diagrams
        setTimeout(() => {
            document.querySelectorAll('.mermaid').forEach((diagram, index) => {
                if (!diagram.dataset.zoomEnabled) {
                    diagram.dataset.zoomEnabled = 'true';
                    diagram.dataset.diagramIndex = index;
                    
                    // Wrap diagram in container
                    const wrapper = document.createElement('div');
                    wrapper.className = 'mermaid-wrapper';
                    diagram.parentNode.insertBefore(wrapper, diagram);
                    wrapper.appendChild(diagram);
                    
                    // Add zoom button
                    const zoomBtn = document.createElement('button');
                    zoomBtn.className = 'mermaid-zoom-btn';
                    zoomBtn.innerHTML = '🔍 Expandir';
                    zoomBtn.onclick = () => this.openMermaidModal(diagram);
                    wrapper.appendChild(zoomBtn);
                    
                    // Add hover effect
                    wrapper.addEventListener('mouseenter', () => {
                        zoomBtn.style.opacity = '1';
                    });
                    wrapper.addEventListener('mouseleave', () => {
                        zoomBtn.style.opacity = '0.7';
                    });
                }
            });
        }, 1000);
    },

    currentZoom: 1,
    
    openMermaidModal: function (diagram) {
        const modal = document.getElementById('mermaid-modal');
        const modalDiagram = document.getElementById('mermaid-modal-diagram');
        
        // Clone the diagram content
        modalDiagram.innerHTML = diagram.outerHTML;
        modal.style.display = 'flex';
        document.body.style.overflow = 'hidden';
        
        // Re-render mermaid in modal
        if (window.mermaid) {
            mermaid.init(undefined, modalDiagram.querySelector('.mermaid'));
        }
        
        // Reset zoom
        this.currentZoom = 1;
        this.applyZoom();
    },
    
    closeMermaidModal: function () {
        const modal = document.getElementById('mermaid-modal');
        modal.style.display = 'none';
        document.body.style.overflow = 'auto';
    },
    
    zoomIn: function () {
        this.currentZoom = Math.min(this.currentZoom + 0.2, 3);
        this.applyZoom();
    },
    
    zoomOut: function () {
        this.currentZoom = Math.max(this.currentZoom - 0.2, 0.5);
        this.applyZoom();
    },
    
    resetZoom: function () {
        this.currentZoom = 1;
        this.applyZoom();
    },
    
    applyZoom: function () {
        const modalDiagram = document.getElementById('mermaid-modal-diagram');
        if (modalDiagram) {
            modalDiagram.style.transform = `scale(${this.currentZoom})`;
        }
    },

    // Smooth scrolling for anchor links
    initSmoothScrolling: function () {
        document.querySelectorAll('a[href^="#"]').forEach(anchor => {
            anchor.addEventListener('click', function (e) {
                e.preventDefault();
                const target = document.querySelector(this.getAttribute('href'));
                if (target) {
                    target.scrollIntoView({
                        behavior: 'smooth',
                        block: 'start'
                    });
                }
            });
        });
    },

    // Enhanced code block highlighting
    initCodeHighlighting: function () {
        document.querySelectorAll('pre code').forEach(block => {
            // Add line numbers
            const lines = block.textContent.split('\n');
            const numberedLines = lines.map((line, i) => 
                `<span class="line-number">${i + 1}</span>${line}`
            ).join('\n');
            
            // Add copy button
            const copyBtn = document.createElement('button');
            copyBtn.className = 'code-copy-btn';
            copyBtn.textContent = '📋 Copiar';
            copyBtn.onclick = () => {
                navigator.clipboard.writeText(block.textContent);
                copyBtn.textContent = '✅ Copiado!';
                setTimeout(() => {
                    copyBtn.textContent = '📋 Copiar';
                }, 2000);
            };
            
            const pre = block.parentElement;
            pre.style.position = 'relative';
            pre.appendChild(copyBtn);
        });
    },

    // Dynamic table of contents
    initTableOfContents: function () {
        const headings = document.querySelectorAll('.markdown-content h1, .markdown-content h2, .markdown-content h3');
        if (headings.length > 3) {
            const toc = document.createElement('div');
            toc.className = 'markdown-toc';
            toc.innerHTML = '<h4>📑 Tabla de Contenidos</h4><ul></ul>';
            
            headings.forEach((heading, index) => {
                heading.id = `heading-${index}`;
                const li = document.createElement('li');
                li.className = `toc-${heading.tagName.toLowerCase()}`;
                li.innerHTML = `<a href="#heading-${index}">${heading.textContent}</a>`;
                toc.querySelector('ul').appendChild(li);
            });
            
            const content = document.querySelector('.markdown-content');
            if (content && content.firstChild) {
                content.insertBefore(toc, content.firstChild);
            }
        }
    },

    // Copy buttons for code blocks
    initCopyButtons: function () {
        document.querySelectorAll('.markdown-content table').forEach(table => {
            // Wrap tables for horizontal scroll
            const wrapper = document.createElement('div');
            wrapper.className = 'table-wrapper';
            table.parentNode.insertBefore(wrapper, table);
            wrapper.appendChild(table);
        });
    },

    // Animate content on scroll
    animateOnScroll: function () {
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('animate-in');
                }
            });
        }, { threshold: 0.1 });

        document.querySelectorAll('.markdown-content > *').forEach(el => {
            observer.observe(el);
        });
    }
};