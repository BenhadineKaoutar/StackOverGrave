# StackOverGrave Visual Reference

## Color Palette

```
Background Colors:
█ #0a0a0a - Dark (main background)
█ #1a1a1a - Medium (gradient)
█ #2a2a2a - Light (cards)

Accent Colors:
█ #00ff41 - Phosphor Green (primary)
█ #00cc33 - Green Dark (hover)

Text Colors:
█ #ffffff - Primary
█ #cccccc - Secondary
█ #888888 - Muted

Status Colors:
█ #666666 - Dead (gray)
█ #00ff41 - Resurrecting/Alive (green)
█ #ff4444 - Failed (red)
█ #ffa500 - Warning (orange)
```

## Component Layouts

### Graveyard Dashboard
```
┌─────────────────────────────────────────────────┐
│              StackOverGrave 🪦                  │
│        Where Legacy Code Rests in Peace         │
└─────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────┐
│  Drop Your Dead Code Here 🪦                    │
│  or click to browse                             │
│  Supports: .vb, .as, .xaml, .cs, .csproj       │
│  Max 5MB                                        │
│  [Choose File]                                  │
└─────────────────────────────────────────────────┘

[All (3)] [🔷 VB6 (1)] [⚡ Flash (1)] [🌙 Silverlight (1)] [🔵 .NET 4.x (0)]

┌──────────┐  ┌──────────┐  ┌──────────┐
│    🔷    │  │    ⚡    │  │    🌙    │
│          │  │          │  │          │
│Calculator│  │ Game.as  │  │MainPage  │
│   .vb    │  │          │  │  .xaml   │
│RIP 2024  │  │RIP 2024  │  │RIP 2024  │
│          │  │          │  │          │
│💀 Dead   │  │⚡Resurrec│  │✅ Alive  │
│          │  │  ting    │  │          │
│📄 2KB    │  │📄 4KB    │  │📄 1.5KB  │
│150 LOC   │  │320 LOC   │  │85 LOC    │
└──────────┘  └──────────┘  └──────────┘
```

### Code Viewer
```
┌─────────────────────────────────────────────────┐
│ [← Back]  Calculator.vb → CalculatorService.cs │
│           VB6 → C# .NET 8              [📦 Download] │
└─────────────────────────────────────────────────┘

┌──────────────────┬─┬──────────────────────────┐
│ 💀 Original (VB6)│ │ ✨ Converted (C# .NET 8) │
├──────────────────┤ ├──────────────────────────┤
│                  │█│                          │
│ Attribute VB_Name│█│ using Microsoft.Ext...   │
│ Option Explicit  │█│                          │
│                  │█│ namespace Calculator;    │
│ Private Sub...   │█│                          │
│   Dim num1...    │█│ public class Calculator  │
│   On Error GoTo  │█│ {                        │
│   ...            │█│   private readonly...    │
│                  │█│   try {                  │
│                  │█│     var result = ...     │
│                  │█│   } catch (Exception ex) │
└──────────────────┴─┴──────────────────────────┘

┌─────────────────────────────────────────────────┐
│           📜 Autopsy Report                     │
├─────────────────────────────────────────────────┤
│ [📝 Notes (6)] [📦 Deps (2)] [⚠️ Breaking (3)] [🚨 Warnings (2)] │
├─────────────────────────────────────────────────┤
│ ▌ Replaced VB6 error handling with try-catch   │
│ ▌ Converted synchronous methods to async/await │
│ ▌ Added dependency injection for ILogger       │
│ ▌ Replaced MsgBox with proper logging          │
│ ▌ Used modern C# naming conventions            │
│ ▌ Added XML documentation comments             │
└─────────────────────────────────────────────────┘
```

### Death Certificate Modal
```
        ┌─────────────────────────────┐
        │  ⚰️ OFFICIAL TECHNOLOGY    │
        │    DEATH CERTIFICATE ⚰️     │
        │                             │
        │  Technology Name:           │
        │  ▌ VB6                      │
        │                             │
        │  Date of Death:             │
        │  ▌ April 8, 2008            │
        │                             │
        │  Cause of Death:            │
        │  ▌ Abandoned by Microsoft   │
        │    in favor of .NET         │
        │                             │
        │  Last Known Location:       │
        │  ▌ Calculator.vb            │
        │                             │
        │  File Statistics:           │
        │  ▌ Lines of Code: 150       │
        │  ▌ File Size: 2 KB          │
        │  ▌ Complexity: Low          │
        │                             │
        │  Survived By:               │
        │  ▌ C# .NET 8                │
        │                             │
        │  Signed: The Tech Reaper 💀 │
        └─────────────────────────────┘
              DECEASED
           (rotated stamp)
```

## Animation States

### Tombstone Card States

**Dead (Initial):**
```
┌──────────┐
│    🔷    │  Gray tones
│Calculator│  No animation
│   .vb    │  
│💀 Dead   │  Awaiting...
└──────────┘
```

**Resurrecting (Processing):**
```
┌──────────┐
│    ⚡    │  Green glow
│ Game.as  │  Shaking
│   ⚡⚡⚡   │  Crack growing
│⚡Resurrec│  Pulsing badge
└──────────┘
```

**Alive (Completed):**
```
┌──────────┐
│    🌙    │  Bright green
│MainPage  │  Glowing aura
│  .xaml   │  
│✅ Alive  │  Success!
└──────────┘
```

## Responsive Breakpoints

### Desktop (> 1024px)
```
[Card] [Card] [Card] [Card]
[Card] [Card] [Card] [Card]
```

### Tablet (768px - 1024px)
```
[Card] [Card]
[Card] [Card]
```

### Mobile (< 768px)
```
[Card]
[Card]
[Card]
```

## Icon Legend

- 🪦 Tombstone (upload, empty state)
- 💀 Skull (dead status)
- ⚡ Lightning (resurrecting, ActionScript)
- ✅ Checkmark (alive status)
- ❌ X (failed status)
- 🔷 Diamond (VB6)
- 🌙 Moon (Silverlight)
- 🔵 Blue circle (.NET Framework)
- 👻 Ghost (particles, drag-over)
- 📄 Document (file size)
- 📦 Package (download, dependencies)
- 📝 Note (migration notes)
- ⚠️ Warning (breaking changes)
- 🚨 Alert (warnings)
- ⚰️ Coffin (death certificate)

## Font Sizes

```
H1 (Title):     48px desktop / 32px mobile
H2 (Subtitle):  36px desktop / 24px mobile
H3 (Section):   24px desktop / 18px mobile
Body:           16px
Small:          14px
Code:           14px (Fira Code)
```

## Spacing Scale

```
xs:  4px
sm:  8px
md:  16px
lg:  24px
xl:  32px
2xl: 48px
```

## Shadow Levels

```
Low:    0 4px 8px rgba(0, 255, 65, 0.1)
Medium: 0 8px 16px rgba(0, 255, 65, 0.1)
High:   0 12px 24px rgba(0, 255, 65, 0.2)
Glow:   0 0 20px rgba(0, 255, 65, 0.5)
```

## Border Radius

```
Small:  4px (badges, inputs)
Medium: 8px (buttons)
Large:  12px (cards)
Round:  50% (close button)
Pill:   20px (filter chips)
```

## Transition Timing

```
Quick:    200ms (hover, click)
Standard: 300ms (navigation)
Smooth:   400-600ms (page transitions)
Dramatic: 800-1200ms (tombstone rise)
```

## Z-Index Layers

```
Base:       0 (content)
Fog:        1 (background effects)
Particles:  1 (background effects)
Content:    2 (cards, components)
Modal:      1000 (death certificate)
```
