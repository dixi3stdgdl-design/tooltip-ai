# Tooltip AI Pilot Program Guide

## Overview

This guide outlines the process for onboarding 1-3 pilot customers and collecting KPIs during the initial deployment phase.

## Pre-Requisites

Before starting the pilot, ensure the following are in place:

- [ ] Agent installed and functioning on Windows 10/11
- [ ] License validation working
- [ ] Privacy controls implemented
- [ ] Management API with Azure AD SSO
- [ ] Enrichment Service with LLM active
- [ ] Deployment documentation ready

## Pilot Selection Criteria

### Ideal Pilot Customers

1. **QA Teams** - Testing software, need quick access to UI element info
2. **Helpdesk/Support** - Troubleshooting user issues, need context
3. **Training Departments** - Creating training materials, need tooltips

### Selection Process

1. Identify 1-3 potential customers
2. Conduct initial meeting to explain value proposition
3. Agree on pilot duration (typically 4 weeks)
4. Define success metrics together
5. Set up regular feedback cadence (weekly)

## Onboarding Process

### Week 1: Installation & Setup

1. **Day 1**: Deploy MSI to pilot users
   ```powershell
   msiexec /i TooltipAI.msi /quiet /norestart ALLUSERS=1
   ```

2. **Day 2**: Configure privacy settings
   - Review and approve privacy settings
   - Set up app blacklist if needed
   - Enable/disable AI enrichment as per customer policy

3. **Day 3**: Initial training session (30 min)
   - Overview of Tooltip AI features
   - How to read tooltips
   - How to provide feedback

### Week 2-3: Usage & Feedback

- Collect daily usage metrics
- Weekly feedback sessions (15 min)
- Address any issues or bugs
- Gather feature requests

### Week 4: Review & Decision

- Present KPI dashboard to customer
- Discuss findings and improvements
- Decide on continuation/purchase

## KPI Tracking

### Key Metrics to Collect

| Metric | Definition | Target | How to Measure |
|--------|-----------|--------|----------------|
| **Adoption Rate** | % of invited users who install and use the agent | >70% | Agent installation logs |
| **Daily Active Users (DAU)** | Users who use Tooltip AI at least once per day | Track trend | Telemetry events |
| **Tooltips per User per Day** | Average number of tooltips viewed per user | >5 | Telemetry events |
| **Relevance Rate** | % of tooltips marked as "useful" | >60% | Feedback prompts |
| **Retention 7-Day** | % of Week 1 users still active in Week 2 | >80% | DAU comparison |
| **Retention 30-Day** | % of Week 1 users still active in Week 4 | >60% | DAU comparison |
| **Time to First Value** | Minutes from installation to first useful tooltip | <5 min | Usage tracking |
| **Support Tickets** | Number of issues reported per week | <5 | Ticket tracking |
| **SLA Compliance** | Backend uptime percentage | >99% | Health checks |

### Data Collection

- Use the Telemetry API to collect usage events
- Store in Azure Table Storage
- Query via MetricsController endpoints
- Export weekly reports

## Feedback Collection

### Weekly Feedback Questions

1. How often did you use Tooltip AI this week?
2. Did you find the tooltips helpful? Why or why not?
3. What applications did you use it with most?
4. Did you encounter any issues or bugs?
5. What features would you like to see added?

### Feedback Channels

- Weekly 15-minute video call
- Email feedback form
- In-app feedback button (future)

## Bug Tracking

- Use GitHub Issues for bug tracking
- Label issues with `pilot` and `priority`
- Respond to critical bugs within 24 hours
- Release fixes weekly

## Iteration Process

1. Collect feedback weekly
2. Prioritize fixes and features
3. Implement changes
4. Release updates
5. Measure impact

## Pilot Success Criteria

### Minimum Success (Continue)

- >50% adoption rate
- >4 tooltips/user/day
- >50% relevance rate
- <10 support tickets total

### Full Success (Purchase)

- >70% adoption rate
- >5 tooltips/user/day
- >60% relevance rate
- <5 support tickets total
- Customer willing to pay/purchase

## Exit Criteria

The pilot ends when:

1. **Duration**: 4 weeks completed
2. **Decision**: Customer decides to purchase or not
3. **Feedback**: Final feedback session completed
4. **Report**: Pilot report delivered to customer

## Templates

### Pilot Agreement Template

```
TOOLTIP AI PILOT PROGRAM AGREEMENT

Between: [Company Name] ("Customer")
And: Tooltip AI ("Provider")

Duration: [Start Date] to [End Date] (4 weeks)

Terms:
1. Provider will supply up to [X] licenses for evaluation
2. Customer will provide feedback as outlined in the pilot guide
3. All data collected during the pilot will be anonymized
4. Customer may purchase licenses at the end of the pilot
5. Either party may terminate with 1 week notice

Signed: _________________ Date: _________________
```

### Weekly Report Template

```
TOOLTIP AI PILOT - WEEKLY REPORT

Week: [X] of 4
Date: [Date]

Metrics:
- Adoption Rate: [X]%
- DAU: [X] users
- Tooltips/User/Day: [X]
- Relevance Rate: [X]%
- Support Tickets: [X]

Feedback Summary:
- [Key feedback points]

Issues:
- [Any issues encountered]

Next Steps:
- [Planned actions for next week]
```
