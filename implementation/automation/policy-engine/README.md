# Policy Engine - Governance Automation

## Overview

Policy engine for automated governance, access control, and compliance enforcement using Open Policy Agent (OPA).

## Features

- **Access Control**: Row-level and column-level security
- **Data Masking**: Automatic PII redaction
- **Compliance Rules**: GDPR, HIPAA, SOC2 enforcement
- **Approval Workflows**: Multi-step approval for sensitive data
- **Audit Logging**: Comprehensive access audit trail

## Architecture

```
Request → Policy Engine → Decision
   ↓           ↓             ↓
 API/SQL     OPA Rules    Allow/Deny
  User      Rego Policy   + Context
 Context    Evaluation    + Masking
```

## Policy Definition (Rego)

```rego
# policies/data_access.rego
package data.access

import future.keywords.if

# Default deny
default allow = false

# Allow if user has role and purpose
allow if {
    user_has_role(input.user, "data_analyst")
    valid_purpose(input.purpose)
    not contains_pii(input.dataset)
}

# Allow PII access only with approval
allow if {
    user_has_role(input.user, "data_analyst")
    contains_pii(input.dataset)
    has_approval(input.user, input.dataset)
}

# Masking rules
mask_columns[column] {
    column := input.columns[_]
    is_pii(input.dataset, column)
    not has_pii_access(input.user)
}

# Helper functions
user_has_role(user, role) if {
    data.users[user].roles[_] == role
}

contains_pii(dataset) if {
    data.datasets[dataset].tags[_] == "PII"
}

has_approval(user, dataset) if {
    approval := data.approvals[_]
    approval.user == user
    approval.dataset == dataset
    approval.status == "approved"
    approval.expires_at > time.now_ns()
}
```

## Usage

### API Integration

```python
from policy_engine import PolicyEngine

engine = PolicyEngine()

# Check data access
decision = engine.evaluate({
    "user": "alice@example.com",
    "dataset": "postgres.analytics.users",
    "action": "read",
    "purpose": "marketing_analysis",
    "columns": ["email", "name", "phone"]
})

if decision.allow:
    # Apply column masking if needed
    masked_columns = decision.mask_columns
    query = apply_masking(query, masked_columns)
    execute_query(query)
else:
    raise PermissionDenied(decision.reason)
```

### SQL Integration

```python
# Automatic row-level security
@policy_enforced
def query_users(user_context):
    query = "SELECT * FROM users"

    # Policy engine adds WHERE clause
    # WHERE tenant_id = :user_tenant
    # AND (pii_access = true OR email IS NULL)

    return db.execute(query, context=user_context)
```

## Policy Examples

### 1. Purpose-Based Access Control

```rego
# policies/purpose_limitation.rego
package governance.purpose

valid_purposes = {
    "analytics",
    "marketing",
    "fraud_detection",
    "customer_support"
}

deny[msg] {
    not input.purpose in valid_purposes
    msg = sprintf("Invalid purpose: %v", [input.purpose])
}

deny[msg] {
    input.purpose == "marketing"
    contains(input.dataset, "healthcare")
    msg = "Healthcare data cannot be used for marketing"
}
```

### 2. Data Retention

```rego
# policies/retention.rego
package governance.retention

deny[msg] {
    dataset := data.datasets[input.dataset]
    retention_days := dataset.retention_policy.days
    data_age_days := (time.now_ns() - dataset.created_at) / 86400000000000

    data_age_days > retention_days
    msg = sprintf("Data exceeds retention period: %v days", [retention_days])
}
```

### 3. Column-Level Masking

```rego
# policies/masking.rego
package governance.masking

# PII columns that should be masked
pii_columns = {
    "email",
    "phone",
    "ssn",
    "credit_card",
    "address"
}

# Masking functions by column type
masking_rules := {
    "email": "hash",
    "phone": "last_4_digits",
    "ssn": "full_mask",
    "credit_card": "last_4_digits"
}

mask[result] {
    column := input.columns[_]
    column.name in pii_columns
    not has_pii_access(input.user)

    result := {
        "column": column.name,
        "method": masking_rules[column.name]
    }
}
```

## Access Request Workflow

```python
# Request access to PII data
access_request = {
    "user": "alice@example.com",
    "dataset": "postgres.analytics.users",
    "purpose": "fraud_investigation",
    "duration_days": 7,
    "justification": "Investigating reported fraud case #12345"
}

# Submit request
request_id = engine.request_access(access_request)

# Approver reviews
engine.approve_request(
    request_id=request_id,
    approver="bob@example.com",
    comment="Approved for fraud investigation"
)
```

## Audit Logging

```python
# All policy decisions are logged
audit_log = {
    "timestamp": "2024-01-15T10:30:00Z",
    "user": "alice@example.com",
    "dataset": "postgres.analytics.users",
    "action": "read",
    "decision": "allow",
    "reason": "User has role data_analyst",
    "masked_columns": ["email", "phone"],
    "purpose": "marketing_analysis"
}
```

## Configuration

```yaml
# config/policy_engine.yml
opa:
  url: http://opa:8181
  policy_path: /policies
  decision_logs: true

policies:
  - path: policies/data_access.rego
  - path: policies/purpose_limitation.rego
  - path: policies/retention.rego
  - path: policies/masking.rego

masking:
  methods:
    hash: sha256
    last_4_digits: "XXX-XXX-{last_4}"
    full_mask: "XXXXX"

approvals:
  required_for:
    - PII
    - PHI
    - FINANCIAL
  approvers:
    - data-governance-team@example.com
  max_duration_days: 30
```

## Directory Structure

```
policy-engine/
├── policies/
│   ├── data_access.rego
│   ├── purpose_limitation.rego
│   ├── retention.rego
│   └── masking.rego
├── engine/
│   ├── opa_client.py
│   ├── policy_evaluator.py
│   └── masking.py
├── workflows/
│   ├── access_request.py
│   └── approval.py
├── audit/
│   └── audit_logger.py
├── tests/
│   └── policy_tests.rego
├── config/
└── README.md
```

## Monitoring

- Policy evaluation latency
- Access denied rate
- Approval request volume
- Audit log completeness

## Testing Policies

```bash
# Test policies with OPA
opa test policies/ -v

# Example test
test_deny_marketing_on_healthcare {
    deny with input as {
        "purpose": "marketing",
        "dataset": "healthcare.patients"
    }
}
```

## References

- [Open Policy Agent Documentation](https://www.openpolicyagent.org/docs/)
- [Rego Policy Language](https://www.openpolicyagent.org/docs/latest/policy-language/)
