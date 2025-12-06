# 🚀 Deployment Checklist

## Pre-Deployment

### Code Quality
- [x] All TypeScript files compile without errors
- [x] All C# files compile without warnings
- [x] No console.log statements in production code
- [x] All TODO comments addressed
- [x] Code follows style guide
- [x] No hardcoded secrets

### Testing
- [ ] Manual testing completed
- [ ] All API endpoints tested
- [ ] File upload works
- [ ] Technology detection works
- [ ] AI conversion works (with API key)
- [ ] Download works
- [ ] Error handling tested
- [ ] Mobile responsive tested
- [ ] Cross-browser tested (Chrome, Firefox, Safari, Edge)

### Repository Resurrection Testing
- [ ] Git import works (GitHub/GitLab/Bitbucket)
- [ ] ZIP upload works (max 50MB)
- [ ] Repository analysis works
- [ ] Size limits enforced correctly
- [ ] File prioritization works
- [ ] Batch conversion works
- [ ] Migration guide generated
- [ ] File tree visualization displays
- [ ] Background jobs process correctly
- [ ] Progress updates work
- [ ] Cost tracking accurate
- [ ] Rate limiting works

### Documentation
- [x] README.md complete
- [x] API documentation (Swagger)
- [x] Setup guides written
- [x] Environment variables documented
- [x] Sample files included

## Frontend Deployment

### Build
```bash
cd frontend
npm run build:prod
```

### Checklist
- [ ] Build completes without errors
- [ ] Bundle size < 500KB
- [ ] Source maps generated
- [ ] Environment variables set
- [ ] API URL points to production

### Vercel Deployment
```bash
npm install -g vercel
vercel login
vercel --prod
```

### Post-Deployment
- [ ] Site loads correctly
- [ ] All routes work
- [ ] API calls succeed
- [ ] Animations smooth
- [ ] No console errors
- [ ] SSL certificate valid

## Backend Deployment

### Build
```bash
cd backend
dotnet publish -c Release -o ./publish
```

### Checklist
- [ ] Build completes without errors
- [ ] All dependencies included
- [ ] Database migrations ready
- [ ] Environment variables set
- [ ] OpenAI API key configured
- [ ] CORS configured for production domain
- [ ] Repository limits configured
- [ ] Rate limiting configured
- [ ] Temp directory configured
- [ ] Cleanup job scheduled

### Azure App Service
```bash
# Create App Service
az webapp create --name stackovergrave-api \
  --resource-group myResourceGroup \
  --plan myAppServicePlan

# Deploy
az webapp deployment source config-zip \
  --resource-group myResourceGroup \
  --name stackovergrave-api \
  --src ./publish.zip
```

### Post-Deployment
- [ ] API responds at production URL
- [ ] Swagger UI accessible
- [ ] Database connection works
- [ ] File uploads work
- [ ] OpenAI integration works
- [ ] SSL certificate valid
- [ ] Health check endpoint responds

## Environment Variables

### Frontend (.env.production)
```
VITE_API_URL=https://api.stackovergrave.com
```

### Backend (Azure App Settings)
```
OPENAI_API_KEY=sk-xxx
ConnectionStrings__DefaultConnection=Server=xxx
ASPNETCORE_ENVIRONMENT=Production
Repository__MaxFileSizeMB=50
Repository__MaxFiles=50
Repository__MaxLinesOfCode=15000
Repository__MaxSingleFileLOC=1000
Repository__TempDirectory=/tmp/stackovergrave
OpenAI__Model=gpt-3.5-turbo
OpenAI__Temperature=0.3
OpenAI__MaxCostPerConversion=7.0
RateLimiting__EnableRateLimiting=true
RateLimiting__PermitLimit=10
RateLimiting__WindowSeconds=60
```

## Database

### SQLite (Development)
- [x] Database file created
- [x] Tables created
- [x] Sample data works

### PostgreSQL (Production)
- [ ] Database created
- [ ] Connection string configured
- [ ] Migrations applied
- [ ] Backup configured

### Migration
```bash
# Update connection string in appsettings.json
dotnet ef database update
```

## Security

### Frontend
- [ ] No API keys in code
- [ ] Content Security Policy configured
- [ ] XSS protection enabled
- [ ] HTTPS enforced
- [ ] Secure cookies

### Backend
- [ ] API key in environment variables
- [ ] Input validation on all endpoints
- [ ] File upload size limits (50MB)
- [ ] Repository size limits enforced
- [ ] Rate limiting configured (10 req/min)
- [ ] CORS properly configured
- [ ] SQL injection prevention
- [ ] Error messages don't leak info
- [ ] Temp file cleanup scheduled
- [ ] Git URL validation enabled
- [ ] ZIP extraction limits enforced

## Performance

### Frontend
- [ ] Lazy loading enabled
- [ ] Images optimized
- [ ] Bundle size optimized
- [ ] Service worker configured
- [ ] CDN configured

### Backend
- [ ] Response caching enabled
- [ ] Database indexes created
- [ ] Connection pooling configured
- [ ] Async/await used throughout
- [ ] File cleanup scheduled

## Monitoring

### Frontend
- [ ] Error tracking (Sentry)
- [ ] Analytics (Google Analytics)
- [ ] Performance monitoring
- [ ] User feedback mechanism

### Backend
- [ ] Application Insights configured
- [ ] Log aggregation setup
- [ ] Health checks configured
- [ ] Alerts configured
- [ ] Uptime monitoring

## DNS & SSL

### Domain
- [ ] Domain purchased
- [ ] DNS records configured
- [ ] A record points to frontend
- [ ] CNAME for API subdomain

### SSL Certificates
- [ ] Frontend SSL configured
- [ ] Backend SSL configured
- [ ] Auto-renewal enabled
- [ ] HTTPS redirect enabled

## Cost Management

### OpenAI
- [ ] Usage limits set
- [ ] Billing alerts configured
- [ ] Cost tracking enabled
- [ ] Budget defined
- [ ] Max cost per conversion: $7
- [ ] Repository limits enforced (50 files, 15K LOC)
- [ ] Rate limiting active (10 req/min)
- [ ] Token limits configured

### Hosting
- [ ] Hosting plan selected
- [ ] Auto-scaling configured
- [ ] Cost alerts set
- [ ] Budget defined
- [ ] Temp storage cleanup scheduled
- [ ] Database size monitored

## Backup & Recovery

### Database
- [ ] Automated backups enabled
- [ ] Backup retention policy set
- [ ] Restore tested
- [ ] Point-in-time recovery enabled

### Files
- [ ] Uploaded files backed up
- [ ] Backup location configured
- [ ] Restore procedure documented

## Legal & Compliance

- [ ] Privacy policy created
- [ ] Terms of service created
- [ ] Cookie consent implemented
- [ ] GDPR compliance checked
- [ ] Data retention policy defined

## Launch Checklist

### Pre-Launch
- [ ] All tests passing
- [ ] Documentation complete
- [ ] Team trained
- [ ] Support channels ready
- [ ] Monitoring active

### Launch
- [ ] Deploy backend
- [ ] Deploy frontend
- [ ] Verify all endpoints
- [ ] Test complete user flow
- [ ] Monitor for errors

### Post-Launch
- [ ] Monitor logs for 24 hours
- [ ] Check error rates
- [ ] Verify performance metrics
- [ ] Collect user feedback
- [ ] Address critical issues

## Rollback Plan

### If Issues Occur
1. Identify the issue
2. Check logs and monitoring
3. Decide: fix forward or rollback
4. If rollback:
   - Revert frontend deployment
   - Revert backend deployment
   - Restore database if needed
5. Communicate with users
6. Fix issue in development
7. Re-deploy when ready

### Rollback Commands

**Vercel:**
```bash
vercel rollback
```

**Azure:**
```bash
az webapp deployment slot swap \
  --resource-group myResourceGroup \
  --name stackovergrave-api \
  --slot staging \
  --target-slot production
```

## Post-Deployment Tasks

### Week 1
- [ ] Monitor error rates daily
- [ ] Check performance metrics
- [ ] Review user feedback
- [ ] Fix critical bugs
- [ ] Optimize slow queries

### Month 1
- [ ] Review costs
- [ ] Analyze usage patterns
- [ ] Plan feature updates
- [ ] Review security
- [ ] Update documentation

## Success Metrics

### Technical
- [ ] Uptime > 99.9%
- [ ] Response time < 500ms
- [ ] Error rate < 1%
- [ ] Conversion success rate > 95%

### Business
- [ ] User signups
- [ ] Conversions completed
- [ ] User satisfaction
- [ ] Cost per conversion

## Support

### Documentation
- [ ] User guide published
- [ ] API docs accessible
- [ ] FAQ created
- [ ] Troubleshooting guide

### Channels
- [ ] Email support configured
- [ ] Issue tracker setup
- [ ] Community forum (optional)
- [ ] Social media accounts

## Continuous Improvement

### Regular Tasks
- [ ] Weekly: Review logs and metrics
- [ ] Monthly: Update dependencies
- [ ] Quarterly: Security audit
- [ ] Yearly: Architecture review

### Feature Pipeline
- [ ] User feedback collection
- [ ] Feature prioritization
- [ ] Development roadmap
- [ ] Release schedule

## Emergency Contacts

- **DevOps:** [Contact]
- **Backend Lead:** [Contact]
- **Frontend Lead:** [Contact]
- **OpenAI Support:** https://help.openai.com
- **Azure Support:** [Portal]
- **Vercel Support:** [Portal]

## Notes

- Keep this checklist updated
- Review before each deployment
- Document any issues encountered
- Share learnings with team

---

**Last Updated:** [Date]
**Next Review:** [Date]
