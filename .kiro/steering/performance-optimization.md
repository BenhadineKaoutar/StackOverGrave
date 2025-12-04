# Performance Optimization

## Frontend Optimizations

### Lazy Loading
```typescript
const routes: Routes = [
  {
    path: 'project/:id',
    loadComponent: () => import('./code-viewer/code-viewer.component')
  }
];
```

### Virtual Scrolling
```typescript
// Use CDK virtual scroll for > 50 items
<cdk-virtual-scroll-viewport itemSize="200">
  <app-tombstone-card 
    *cdkVirtualFor="let project of projects"
    [project]="project">
  </app-tombstone-card>
</cdk-virtual-scroll-viewport>
```

### Image Optimization
- Use SVG for icons (scalable, small)
- Lazy load images: `loading="lazy"`
- Use WebP format with fallbacks
- Compress images (TinyPNG, ImageOptim)

### Bundle Size
- Use standalone components (tree-shakeable)
- Avoid importing entire libraries
- Code splitting by route
- Target: < 500KB initial bundle

## Backend Optimizations

### Caching
```csharp
services.AddMemoryCache();
services.AddResponseCaching();

// Cache AI conversions
_cache.Set(cacheKey, result, TimeSpan.FromHours(24));
```

### Async All The Way
```csharp
// Bad
var result = service.GetDataAsync().Result;

// Good
var result = await service.GetDataAsync();
```

### Database Queries
- Use indexes on frequently queried columns
- Avoid N+1 queries
- Use pagination for large result sets
- Consider read replicas for scaling

## AI Service Optimizations

### Token Management
- Truncate large files (keep first/last 2000 lines)
- Remove comments before sending
- Cache identical conversions

### Parallel Processing
- Process multiple files concurrently
- Use `Task.WhenAll` for batch operations
- Respect API rate limits

## Monitoring

### Key Metrics
- Page load time (target: < 2s)
- Time to interactive (target: < 3s)
- API response time (target: < 500ms)
- AI conversion time (target: < 30s)

### Tools
- Lighthouse for frontend performance
- Application Insights for backend
- OpenAI usage dashboard for costs
