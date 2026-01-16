// Wormhole Gravity Effect - Follows mouse cursor
class WormholeEffect {
    constructor(canvasId) {
     this.canvas = document.getElementById(canvasId);
        if (!this.canvas) return;
  
   this.ctx = this.canvas.getContext('2d');
        this.particles = [];
this.mouseX = 0;
        this.mouseY = 0;
        this.targetX = 0;
        this.targetY = 0;
      this.wormholeX = 0;
        this.wormholeY = 0;
        
        this.init();
    }
    
    init() {
        this.resize();
        window.addEventListener('resize', () => this.resize());
        
        // Track mouse movement
        document.addEventListener('mousemove', (e) => {
            const rect = this.canvas.getBoundingClientRect();
     this.targetX = e.clientX - rect.left;
     this.targetY = e.clientY - rect.top;
        });
        
        // Create particles
 this.createParticles();

        // Start animation
  this.animate();
    }
    
    resize() {
   this.canvas.width = this.canvas.offsetWidth;
        this.canvas.height = this.canvas.offsetHeight;
     this.wormholeX = this.canvas.width / 2;
     this.wormholeY = this.canvas.height / 2;
    }
    
    createParticles() {
        const particleCount = 150;
        for (let i = 0; i < particleCount; i++) {
          this.particles.push({
          x: Math.random() * this.canvas.width,
         y: Math.random() * this.canvas.height,
          vx: (Math.random() - 0.5) * 0.5,
           vy: (Math.random() - 0.5) * 0.5,
       size: Math.random() * 2 + 1,
           opacity: Math.random() * 0.5 + 0.3
 });
        }
    }
    
    animate() {
        // Smooth wormhole movement towards mouse
  this.wormholeX += (this.targetX - this.wormholeX) * 0.05;
        this.wormholeY += (this.targetY - this.wormholeY) * 0.05;
        
        // Clear canvas
     this.ctx.clearRect(0, 0, this.canvas.width, this.canvas.height);
        
      // Draw wormhole rings
        this.drawWormhole();
        
        // Draw and update particles
 this.updateParticles();
        
  requestAnimationFrame(() => this.animate());
    }
    
    drawWormhole() {
    const rings = 8;
        const maxRadius = 150;
        
        for (let i = rings; i > 0; i--) {
    const radius = (i / rings) * maxRadius;
            const opacity = (i / rings) * 0.3;
            
 // Outer glow
      const gradient = this.ctx.createRadialGradient(
           this.wormholeX, this.wormholeY, 0,
         this.wormholeX, this.wormholeY, radius
        );
            gradient.addColorStop(0, `rgba(255, 255, 255, ${opacity})`);
            gradient.addColorStop(0.5, `rgba(255, 255, 255, ${opacity * 0.5})`);
   gradient.addColorStop(1, 'rgba(255, 255, 255, 0)');
            
   this.ctx.fillStyle = gradient;
            this.ctx.beginPath();
            this.ctx.arc(this.wormholeX, this.wormholeY, radius, 0, Math.PI * 2);
    this.ctx.fill();
            
       // Ring outline
            this.ctx.strokeStyle = `rgba(255, 255, 255, ${opacity * 0.6})`;
      this.ctx.lineWidth = 2;
            this.ctx.beginPath();
      this.ctx.arc(this.wormholeX, this.wormholeY, radius, 0, Math.PI * 2);
            this.ctx.stroke();
        }
        
   // Center core
        const coreGradient = this.ctx.createRadialGradient(
  this.wormholeX, this.wormholeY, 0,
            this.wormholeX, this.wormholeY, 30
   );
  coreGradient.addColorStop(0, 'rgba(255, 255, 255, 0.8)');
        coreGradient.addColorStop(0.5, 'rgba(255, 255, 255, 0.4)');
        coreGradient.addColorStop(1, 'rgba(255, 255, 255, 0)');
        
  this.ctx.fillStyle = coreGradient;
      this.ctx.beginPath();
        this.ctx.arc(this.wormholeX, this.wormholeY, 30, 0, Math.PI * 2);
        this.ctx.fill();
    }
    
    updateParticles() {
    this.particles.forEach(particle => {
            // Calculate distance to wormhole
   const dx = this.wormholeX - particle.x;
      const dy = this.wormholeY - particle.y;
       const distance = Math.sqrt(dx * dx + dy * dy);
      
         // Gravity effect - stronger when closer
    if (distance < 200) {
      const force = (200 - distance) / 200;
                const angle = Math.atan2(dy, dx);
  particle.vx += Math.cos(angle) * force * 0.3;
         particle.vy += Math.sin(angle) * force * 0.3;
      }
            
  // Update position
 particle.x += particle.vx;
     particle.y += particle.vy;

            // Damping
          particle.vx *= 0.95;
 particle.vy *= 0.95;
      
     // Wrap around edges
 if (particle.x < 0) particle.x = this.canvas.width;
            if (particle.x > this.canvas.width) particle.x = 0;
       if (particle.y < 0) particle.y = this.canvas.height;
          if (particle.y > this.canvas.height) particle.y = 0;
          
          // Respawn if too close to wormhole center
  if (distance < 20) {
           particle.x = Math.random() * this.canvas.width;
            particle.y = Math.random() * this.canvas.height;
 particle.vx = (Math.random() - 0.5) * 0.5;
   particle.vy = (Math.random() - 0.5) * 0.5;
          }
            
        // Draw particle
      this.ctx.fillStyle = `rgba(255, 255, 255, ${particle.opacity})`;
     this.ctx.beginPath();
        this.ctx.arc(particle.x, particle.y, particle.size, 0, Math.PI * 2);
   this.ctx.fill();
            
            // Draw connection lines to nearby particles
       this.particles.forEach(otherParticle => {
            const dx2 = particle.x - otherParticle.x;
       const dy2 = particle.y - otherParticle.y;
const dist2 = Math.sqrt(dx2 * dx2 + dy2 * dy2);
      
  if (dist2 < 80 && dist2 > 0) {
          this.ctx.strokeStyle = `rgba(255, 255, 255, ${(1 - dist2 / 80) * 0.2})`;
     this.ctx.lineWidth = 0.5;
    this.ctx.beginPath();
          this.ctx.moveTo(particle.x, particle.y);
          this.ctx.lineTo(otherParticle.x, otherParticle.y);
       this.ctx.stroke();
          }
       });
 });
    }
}

// Initialize wormhole on page load
document.addEventListener('DOMContentLoaded', () => {
    // Only initialize if wormhole canvas exists
    const canvas = document.getElementById('wormhole-canvas');
    if (canvas && window.innerWidth > 768) {
        new WormholeEffect('wormhole-canvas');
    }
});
