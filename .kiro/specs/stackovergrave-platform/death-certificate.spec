# Death Certificate Modal Spec

## Overview
Modal component displaying technology information in an official "death certificate" format with spooky styling.

## Visual Design

### Modal Appearance
- Parchment texture background
- Torn edges effect
- Gothic border
- Red "DECEASED" stamp overlay (rotated -15deg)
- Fade-in animation on open

## Certificate Content

### Header
```
╔═══════════════════════════════════════╗
║   OFFICIAL TECHNOLOGY DEATH CERTIFICATE   ║
╚═══════════════════════════════════════╝
```

### Body Sections
1. Technology Name & Icon
2. Date of Death (deprecated date)
3. Cause of Death (humorous message)
4. File Statistics
5. Last Known Location (filename)
6. Survived By (modern replacement)

## Component Structure

```typescript
@Component({
  selector: 'app-death-certificate',
  standalone: true,
  template: `
    <div class="modal-overlay" (click)="close()">
      <div class="certificate" (click)="$event.stopPropagation()">
        <div class="stamp">DECEASED</div>
        <!-- Certificate content -->
      </div>
    </div>
  `
})
export class DeathCertificateComponent {
  @Input() certificate!: DeathCertificate;
  @Output() closed = new EventEmitter<void>();
}
```

## Styling

```scss
.certificate {
  background: url('/assets/parchment.jpg');
  border: 8px solid #3a2a1a;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.8);
  font-family: 'Courier New', monospace;
}

.stamp {
  position: absolute;
  top: 50%;
  right: 10%;
  transform: rotate(-15deg);
  color: #8b0000;
  font-size: 48px;
  font-weight: bold;
  opacity: 0.7;
}
```
