# Code Conversion Patterns

## VB6 to C# Patterns

### Collections
```vb
' VB6
Dim items As Collection
Set items = New Collection
items.Add "value"
```
```csharp
// C#
var items = new List<string>();
items.Add("value");
```

### Error Handling
```vb
' VB6
On Error GoTo ErrorHandler
' code
Exit Sub
ErrorHandler:
MsgBox Err.Description
```
```csharp
// C#
try {
    // code
} catch (Exception ex) {
    _logger.LogError(ex, "Error occurred");
    throw;
}
```

### File I/O
```vb
' VB6
Open "file.txt" For Input As #1
Line Input #1, strLine
Close #1
```
```csharp
// C#
var lines = await File.ReadAllLinesAsync("file.txt");
```

## ActionScript to TypeScript Patterns

### Event Listeners
```actionscript
// ActionScript
button.addEventListener(MouseEvent.CLICK, onClick);
function onClick(e:MouseEvent):void {
    trace("clicked");
}
```
```typescript
// TypeScript/Angular
<button (click)="onClick()">Click</button>

onClick() {
    console.log("clicked");
}
```

### Display Objects
```actionscript
// ActionScript
var sprite:Sprite = new Sprite();
sprite.graphics.beginFill(0xFF0000);
sprite.graphics.drawRect(0, 0, 100, 100);
addChild(sprite);
```
```typescript
// TypeScript (Canvas)
const canvas = document.getElementById('canvas') as HTMLCanvasElement;
const ctx = canvas.getContext('2d')!;
ctx.fillStyle = '#FF0000';
ctx.fillRect(0, 0, 100, 100);
```

## Silverlight to Angular Patterns

### Data Binding
```xml
<!-- Silverlight -->
<TextBox Text="{Binding Username, Mode=TwoWay}" />
```
```html
<!-- Angular -->
<mat-form-field>
  <input matInput [(ngModel)]="username">
</mat-form-field>
```

### Commands
```xml
<!-- Silverlight -->
<Button Command="{Binding SaveCommand}" />
```
```html
<!-- Angular -->
<button mat-button (click)="save()">Save</button>
```

## Best Practices

### Always Preserve Business Logic
- Don't change algorithms
- Keep validation rules identical
- Maintain data structures

### Use Modern Patterns
- Async/await for I/O
- Dependency injection
- LINQ for collections
- Proper error handling

### Add Type Safety
- Use strong types everywhere
- Enable nullable reference types
- Add generics where appropriate
