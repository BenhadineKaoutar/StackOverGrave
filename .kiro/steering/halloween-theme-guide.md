# Halloween Theme Guide

## Color Palette

### Primary Colors
- Background Dark: `#0a0a0a`
- Background Medium: `#1a1a1a`
- Background Light: `#2a2a2a`
- Accent Green (Phosphor): `#00ff41`
- Accent Green Dark: `#00cc33`

### Status Colors
- Dead (Gray): `#666666`
- Resurrecting (Green): `#00ff41`
- Alive (Success): `#00ff41`
- Failed (Red): `#ff4444`

### Text Colors
- Primary Text: `#ffffff`
- Secondary Text: `#cccccc`
- Muted Text: `#888888`

## Typography

### Fonts
- Headers: `'Nosifer', 'Creepster', cursive`
- Body: `'Inter', 'Roboto', sans-serif`
- Code: `'Fira Code', 'JetBrains Mono', monospace`

### Font Sizes
- H1: 48px (desktop), 32px (mobile)
- H2: 36px (desktop), 24px (mobile)
- H3: 24px (desktop), 18px (mobile)
- Body: 16px
- Small: 14px

## Animation Guidelines

### Performance
- Use `transform` and `opacity` only (GPU accelerated)
- Avoid animating `width`, `height`, `top`, `left`
- Use `will-change` sparingly
- Respect `prefers-reduced-motion`

### Timing
- Quick interactions: 200-300ms
- Standard transitions: 400-600ms
- Dramatic effects: 800-1200ms
- Easing: `cubic-bezier(0.4, 0.0, 0.2, 1)` or `ease-out`

### Effects to Use
- Fade in/out
- Scale up/down
- Slide up/down
- Glow (box-shadow)
- Crack/break effects
- Floating/drifting

## Spooky Elements

### Fog Effect
- Subtle, not distracting
- Slow movement (20s+ animation)
- Low opacity (0.03-0.05)

### Particles
- Small, sparse (20-30 max)
- Slow upward drift
- Random horizontal movement
- Very low opacity (0.1-0.3)

### Sound Effects
- Background music: Low volume (10-20%), loopable
- UI sounds: Short (< 1s), not annoying
- Always provide mute button
- Respect user's audio preferences

## Component Styling

### Cards
```scss
.card {
  background: #2a2a2a;
  border: 2px solid #444;
  border-radius: 8px;
  box-shadow: 0 8px 16px rgba(0, 255, 65, 0.1);
  transition: transform 300ms ease-out;
  
  &:hover {
    transform: translateY(-8px);
    box-shadow: 0 12px 24px rgba(0, 255, 65, 0.2);
  }
}
```

### Buttons
```scss
.button-primary {
  background: #00ff41;
  color: #0a0a0a;
  border: none;
  padding: 12px 24px;
  font-weight: 600;
  cursor: pointer;
  transition: all 200ms ease-out;
  
  &:hover {
    background: #00cc33;
    box-shadow: 0 0 20px rgba(0, 255, 65, 0.5);
  }
}
```

## Accessibility

### Contrast Ratios
- Normal text: 4.5:1 minimum
- Large text: 3:1 minimum
- UI components: 3:1 minimum

### Focus States
- Always visible
- High contrast outline
- Don't remove default focus styles without replacement

### Motion
```scss
@media (prefers-reduced-motion: reduce) {
  * {
    animation-duration: 0.01ms !important;
    animation-iteration-count: 1 !important;
    transition-duration: 0.01ms !important;
  }
}
```
