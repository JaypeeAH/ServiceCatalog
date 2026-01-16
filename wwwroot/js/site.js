// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Cardinal Health Service Catalog - Site JavaScript

// Cardinal Health Service Catalog - Modern JavaScript

// Custom Cursor Trail Effect (Inspired by Google Antigravity)
let cursorDot, cursorTrail;
let mouseX = 0, mouseY = 0;
let trailX = 0, trailY = 0;

function initCustomCursor() {
    // Only on desktop
    if (window.innerWidth > 768) {
        // Create cursor dot
        cursorDot = document.createElement('div');
        cursorDot.className = 'cursor-dot';
     document.body.appendChild(cursorDot);

    // Create cursor trail
  cursorTrail = document.createElement('div');
        cursorTrail.className = 'cursor-trail';
        document.body.appendChild(cursorTrail);

     // Track mouse movement
        document.addEventListener('mousemove', (e) => {
            mouseX = e.clientX;
         mouseY = e.clientY;

       // Update dot position immediately
    cursorDot.style.left = mouseX + 'px';
   cursorDot.style.top = mouseY + 'px';
            
          // Create particle effect occasionally
          if (Math.random() > 0.9) {
      createCursorParticle(mouseX, mouseY);
    }
        });

     // Animate trail with delay
        animateTrail();
      
        // Change cursor on hover over clickable elements
     document.querySelectorAll('a, button, .category-pill, .service-card').forEach(el => {
      el.addEventListener('mouseenter', () => {
           cursorTrail.style.transform = 'scale(1.5)';
            cursorTrail.style.borderColor = 'var(--robin-egg-blue)';
     });
            
 el.addEventListener('mouseleave', () => {
     cursorTrail.style.transform = 'scale(1)';
        cursorTrail.style.borderColor = 'var(--cardinal-red)';
});
        });
    }
}

function animateTrail() {
    // Smooth trailing effect
    trailX += (mouseX - trailX) * 0.15;
    trailY += (mouseY - trailY) * 0.15;
    
    try {
        if (cursorTrail && cursorTrail.style) {
            cursorTrail.style.left = (trailX - 20) + 'px';
            cursorTrail.style.top = (trailY - 20) + 'px';
        }
    } catch (error) {
        console.error('Cursor trail animation error:', error);
    }
    
    requestAnimationFrame(animateTrail);
}

function createCursorParticle(x, y) {
    const particle = document.createElement('div');
  particle.className = 'cursor-particle';
    particle.style.left = x + 'px';
    particle.style.top = y + 'px';
    
  // Random direction
    const angle = Math.random() * Math.PI * 2;
    const distance = 20 + Math.random() * 30;
    particle.style.setProperty('--tx', Math.cos(angle) * distance + 'px');
    particle.style.setProperty('--ty', Math.sin(angle) * distance + 'px');
    
    document.body.appendChild(particle);
    
    // Remove after animation
    setTimeout(() => particle.remove(), 1000);
}

// Initialize everything when DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    initCustomCursor();
    initSmoothScrolling();
  initScrollAnimations();
  initParallaxEffects();
    initInteractiveElements();
    initCategoryFilters();
    initTooltips();
    logPageView();
});

// Smooth scroll to sections
function initSmoothScrolling() {
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
}

// Animate elements on scroll with Intersection Observer
function initScrollAnimations() {
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('animate-in');
            entry.target.style.opacity = '1';
   entry.target.style.transform = 'translateY(0)';
            }
        });
    }, observerOptions);

    // Observe all animated elements
    document.querySelectorAll('.service-card, .location-card, .process-item, .category-pill').forEach(el => {
        el.style.opacity = '0';
        el.style.transform = 'translateY(20px)';
        el.style.transition = 'all 0.8s cubic-bezier(0.175, 0.885, 0.32, 1.275)';
        observer.observe(el);
  });
}

// Parallax scrolling effects
function initParallaxEffects() {
    let ticking = false;
    
    window.addEventListener('scroll', () => {
        if (!ticking) {
window.requestAnimationFrame(() => {
     const scrolled = window.pageYOffset;
    
  // Hero parallax
      const hero = document.querySelector('.hero-section');
     if (hero) {
                    hero.style.transform = `translateY(${scrolled * 0.3}px)`;
        }

                // Floating elements
  document.querySelectorAll('.particle').forEach((particle, index) => {
         const speed = 0.5 + (index * 0.1);
          particle.style.transform = `translateY(${scrolled * speed}px)`;
 });

  ticking = false;
  });
            ticking = true;
        }
    });
}

// Interactive element enhancements
function initInteractiveElements() {
    // Service card hover effects
 document.querySelectorAll('.service-card').forEach(card => {
   card.addEventListener('mouseenter', function() {
         this.style.transition = 'all 0.4s cubic-bezier(0.175, 0.885, 0.32, 1.275)';
   
        // Add glow effect
this.style.boxShadow = '0 20px 60px rgba(236, 90, 98, 0.3)';
      });

        card.addEventListener('mouseleave', function() {
     this.style.boxShadow = '0 10px 40px rgba(0, 0, 0, 0.1)';
        });

        // 3D tilt effect on mouse move
        card.addEventListener('mousemove', function(e) {
    const rect = this.getBoundingClientRect();
   const x = e.clientX - rect.left;
            const y = e.clientY - rect.top;
            
    const centerX = rect.width / 2;
   const centerY = rect.height / 2;
   
            const rotateX = (y - centerY) / 20;
       const rotateY = (centerX - x) / 20;
        
         this.style.transform = `perspective(1000px) rotateX(${rotateX}deg) rotateY(${rotateY}deg) translateY(-15px)`;
    });

      card.addEventListener('mouseleave', function() {
   this.style.transform = 'perspective(1000px) rotateX(0) rotateY(0) translateY(0)';
    });
    });

    // Button ripple effect
    document.querySelectorAll('.service-details-btn, .category-pill').forEach(btn => {
   btn.addEventListener('click', function(e) {
   const ripple = document.createElement('span');
 const rect = this.getBoundingClientRect();
    const size = Math.max(rect.width, rect.height);
            const x = e.clientX - rect.left - size / 2;
  const y = e.clientY - rect.top - size / 2;

         ripple.style.width = ripple.style.height = size + 'px';
    ripple.style.left = x + 'px';
            ripple.style.top = y + 'px';
      ripple.style.position = 'absolute';
            ripple.style.borderRadius = '50%';
   ripple.style.background = 'rgba(255, 255, 255, 0.6)';
   ripple.style.transform = 'scale(0)';
   ripple.style.animation = 'ripple 0.6s ease-out';
   ripple.style.pointerEvents = 'none';

            this.style.position = 'relative';
   this.style.overflow = 'hidden';
  this.appendChild(ripple);

     setTimeout(() => ripple.remove(), 600);
        });
    });

  // Location card hover effect
    document.querySelectorAll('.location-card').forEach(card => {
        card.addEventListener('mouseenter', function() {
      this.style.transform = 'translateY(-10px) scale(1.02)';
        });

   card.addEventListener('mouseleave', function() {
       this.style.transform = 'translateY(0) scale(1)';
        });
    });
}

// Category filter animations
function initCategoryFilters() {
    document.querySelectorAll('.category-pill').forEach(pill => {
        pill.addEventListener('click', function(e) {
// Scale animation
      this.style.transform = 'scale(0.95)';
  setTimeout(() => {
      this.style.transform = '';
}, 150);

      // Animate services grid
            const servicesGrid = document.querySelector('.services-grid');
       if (servicesGrid) {
   servicesGrid.style.opacity = '0';
    servicesGrid.style.transform = 'translateY(20px)';
    
      setTimeout(() => {
       servicesGrid.style.transition = 'all 0.6s ease';
      servicesGrid.style.opacity = '1';
              servicesGrid.style.transform = 'translateY(0)';
      }, 100);
    }
 });

        // Hover effect
        pill.addEventListener('mouseenter', function() {
this.style.transform = 'translateY(-3px)';
        });

        pill.addEventListener('mouseleave', function() {
          if (!this.classList.contains('active')) {
       this.style.transform = 'translateY(0)';
    }
      });
    });
}

// Tooltip functionality
function initTooltips() {
    document.querySelectorAll('[data-toggle="tooltip"]').forEach(element => {
     if (element.scrollWidth > element.clientWidth) {
     element.setAttribute('title', element.textContent);
        }
    });
}

// Form handling with loading states
document.querySelectorAll('form').forEach(form => {
    form.addEventListener('submit', function(e) {
        // Only add loading state, don't prevent submission
        try {
            const submitBtn = this.querySelector('[type="submit"]');
            if (submitBtn) {
                submitBtn.disabled = true;
                const originalHtml = submitBtn.innerHTML;
                submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Loading...';
                
                // Re-enable button after 5 seconds as fallback
                setTimeout(() => {
                    if (submitBtn.disabled) {
                        submitBtn.disabled = false;
                        submitBtn.innerHTML = originalHtml;
                    }
                }, 5000);
            }
        } catch (error) {
            console.error('Form submit handler error:', error);
            // Don't prevent form submission even if there's an error
        }
    });
});

// Responsive navbar handling
const navbarToggler = document.querySelector('.navbar-toggler');
if (navbarToggler) {
    navbarToggler.addEventListener('click', function() {
        this.classList.toggle('active');
        
  // Animate navbar collapse
        const navbarCollapse = document.querySelector('.navbar-collapse');
        if (navbarCollapse) {
            navbarCollapse.style.transition = 'all 0.3s ease';
        }
 });
}

// Back to top functionality
function createBackToTopButton() {
 const backToTop = document.createElement('button');
    backToTop.id = 'backToTop';
    backToTop.innerHTML = '↑';
    backToTop.style.cssText = `
        position: fixed;
        bottom: 30px;
        right: 30px;
   width: 50px;
        height: 50px;
  border-radius: 50%;
background: linear-gradient(135deg, var(--cardinal-red), var(--flamingo));
        color: white;
        border: none;
        font-size: 24px;
   cursor: pointer;
        box-shadow: 0 4px 15px rgba(236, 90, 98, 0.4);
        display: none;
 z-index: 1000;
        transition: all 0.3s ease;
    `;

    document.body.appendChild(backToTop);

    window.addEventListener('scroll', () => {
  if (window.pageYOffset > 300) {
            backToTop.style.display = 'block';
        setTimeout(() => backToTop.style.opacity = '1', 10);
        } else {
       backToTop.style.opacity = '0';
        setTimeout(() => backToTop.style.display = 'none', 300);
        }
    });

    backToTop.addEventListener('click', () => {
        window.scrollTo({
 top: 0,
            behavior: 'smooth'
        });
    });

    backToTop.addEventListener('mouseenter', function() {
     this.style.transform = 'scale(1.1) translateY(-5px)';
        this.style.boxShadow = '0 8px 25px rgba(236, 90, 98, 0.6)';
    });

    backToTop.addEventListener('mouseleave', function() {
      this.style.transform = 'scale(1) translateY(0)';
     this.style.boxShadow = '0 4px 15px rgba(236, 90, 98, 0.4)';
    });
}

createBackToTopButton();

// Utility function for debouncing
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
    const later = () => {
    clearTimeout(timeout);
      func(...args);
        };
     clearTimeout(timeout);
        timeout = setTimeout(later, wait);
 };
}

// Handle window resize
const handleResize = debounce(() => {
    console.log('Window resized - adjusting layout');
    
    // Recalculate card heights
    document.querySelectorAll('.service-card').forEach(card => {
        card.style.transition = 'none';
        setTimeout(() => {
        card.style.transition = '';
        }, 100);
  });
    
    // Reinit custom cursor if needed
    if (window.innerWidth > 768 && !cursorDot) {
        initCustomCursor();
    } else if (window.innerWidth <= 768 && cursorDot) {
        cursorDot.remove();
        cursorTrail.remove();
        cursorDot = null;
      cursorTrail = null;
    }
}, 250);

window.addEventListener('resize', handleResize);

// Page visibility change handling
document.addEventListener('visibilitychange', () => {
    if (document.hidden) {
  console.log('Page hidden - pausing animations');
    } else {
  console.log('Page visible - resuming animations');
    }
});

// Keyboard navigation enhancement
document.addEventListener('keydown', (e) => {
    // ESC key to close modals or return to top
    if (e.key === 'Escape') {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }
    
    // Arrow keys for navigation
    if (e.key === 'ArrowUp' && e.ctrlKey) {
        e.preventDefault();
    window.scrollTo({ top: 0, behavior: 'smooth' });
    }
});

// CSS animations keyframes injection
const style = document.createElement('style');
style.textContent = `
    @keyframes ripple {
      to {
            transform: scale(4);
            opacity: 0;
        }
    }
    
    @keyframes fadeInUp {
        from {
            opacity: 0;
            transform: translateY(30px);
        }
        to {
     opacity: 1;
  transform: translateY(0);
  }
    }
    
    .animate-in {
      animation: fadeInUp 0.6s ease-out forwards;
    }
`;
document.head.appendChild(style);

// Log page view for analytics
function logPageView() {
    console.log('🎨 Cardinal Health Service Catalog - Modern UI with Cursor Trail Loaded');
    console.log('📍 Page:', window.location.pathname);
    console.log('⏰ Time:', new Date().toLocaleString());
    console.log('🖥️ Viewport:', `${window.innerWidth}x${window.innerHeight}`);
    console.log('🖱️ Custom Cursor:', window.innerWidth > 768 ? 'Enabled' : 'Disabled (Mobile)');
}

// Performance monitoring
if ('performance' in window) {
    window.addEventListener('load', () => {
        const perfData = performance.timing;
        const pageLoadTime = perfData.loadEventEnd - perfData.navigationStart;
        console.log(`⚡ Page load time: ${pageLoadTime}ms`);
    });
}

// Export functions for use in other scripts
window.ServiceCatalog = {
    debounce,
    initScrollAnimations,
    initParallaxEffects,
    initCustomCursor
};
