# Security Updates

## Airflow Version Update (2.7.1 → 2.11.1)

**Date**: 2026-05-03

### Vulnerabilities Patched

The following security vulnerabilities have been addressed by upgrading from Apache Airflow 2.7.1 to 2.11.1:

1. ✅ **Code Injection in web-server context via LogTemplate table**
   - **Severity**: High
   - **Affected**: < 2.11.1
   - **Fixed in**: 2.11.1

2. ✅ **Proxy credentials leak in task logs**
   - **Severity**: High
   - **Affected**: < 2.11.1
   - **Fixed in**: 2.11.1

3. ✅ **Execution with Unnecessary Privileges**
   - **Severity**: Medium
   - **Affected**: < 2.10.1
   - **Fixed in**: 2.11.1

4. ✅ **DAG Author Code Execution in airflow-scheduler**
   - **Severity**: High
   - **Affected**: >= 2.4.0, < 2.9.3
   - **Fixed in**: 2.11.1

5. ✅ **Bypass permission verification to read code of other dags**
   - **Severity**: Medium
   - **Affected**: < 2.8.1
   - **Fixed in**: 2.11.1

6. ✅ **Pickle deserialization vulnerability in XComs**
   - **Severity**: High
   - **Affected**: < 2.8.1
   - **Fixed in**: 2.11.1

7. ✅ **Exposure of Sensitive Information to Unauthorized Actor**
   - **Severity**: Medium
   - **Affected**: < 2.7.3
   - **Fixed in**: 2.11.1

### Known Remaining Vulnerability

⚠️ **RCE by race condition in example_xcom dag**
   - **Severity**: Critical
   - **Affected**: < 3.2.0 (all 2.x versions)
   - **Status**: Awaiting Airflow 3.2.0 stable release
   - **Mitigation**: See workaround below

### Mitigation for RCE Vulnerability

Since Airflow 3.2.0 is not yet released as stable, apply this workaround:

**Option 1: Disable Example DAGs (Recommended)**

In `docker-compose.yml`, the example DAGs are already disabled:

```yaml
environment:
  - AIRFLOW__CORE__LOAD_EXAMPLES=False
```

This prevents the vulnerable `example_xcom` DAG from being loaded.

**Option 2: Delete Example DAGs**

If example DAGs somehow get loaded, remove them:

```bash
docker exec -it airflow-webserver bash
rm -rf /opt/airflow/dags/example_*
airflow dags delete example_xcom
```

**Option 3: Use Airflow 3.x (When Available)**

Monitor for Airflow 3.2.0 stable release:
- Check: https://airflow.apache.org/docs/apache-airflow/stable/release_notes.html
- When available, update to `apache-airflow==3.2.0`

### Verification

Confirm example DAGs are disabled:

```bash
docker exec -it airflow-webserver airflow dags list | grep example
```

Should return no results if examples are properly disabled.

### Changes Made

1. **airflow/requirements.txt**: Updated `apache-airflow==2.7.1` → `apache-airflow==2.11.1`
2. **docker-compose.yml**:
   - Updated Airflow images: `apache/airflow:2.11.1-python3.11`
   - Ensured `AIRFLOW__CORE__LOAD_EXAMPLES=False` is set
3. **Documentation**: Updated version references in README.md, ARCHITECTURE.md, and IMPLEMENTATION_SUMMARY.md

### Testing

After upgrading:

1. Verify Airflow starts correctly:
   ```bash
   docker-compose up -d airflow-webserver airflow-scheduler
   docker-compose logs -f airflow-webserver
   ```

2. Check Airflow UI is accessible:
   ```bash
   curl http://localhost:8081/health
   ```

3. Verify no example DAGs are loaded:
   ```bash
   docker exec -it airflow-webserver airflow dags list
   ```
   Should only show `data_governance_pipeline`, not `example_*` DAGs.

4. Verify DAG execution:
   - Login to Airflow UI (http://localhost:8081)
   - Enable `data_governance_pipeline` DAG
   - Trigger a test run
   - Confirm all tasks complete successfully

### Compatibility

- **Python Version**: 3.11 (unchanged)
- **PostgreSQL**: 13 for Airflow metadata (unchanged)
- **Dependencies**: All other dependencies compatible
- **DAG Code**: No changes required - backward compatible

### Migration Path to Airflow 3.x

When Airflow 3.2.0+ is released:

1. Review breaking changes in Airflow 3.x
2. Test in development environment
3. Update requirements.txt and docker-compose.yml
4. Validate all DAGs and connections
5. Deploy to production

### Additional Security Recommendations

For production deployments:

1. **Disable Example DAGs**: ✅ Already configured
2. **Enable Authentication**: Configure OAuth2/OIDC
3. **Use HTTPS**: Enable TLS for Airflow UI
4. **Restrict Network Access**: Use firewall rules
5. **Enable RBAC**: Configure role-based access control
6. **Secrets Backend**: Use HashiCorp Vault or AWS Secrets Manager
7. **Regular Updates**: Subscribe to Apache Airflow security announcements
8. **Audit Logs**: Enable and monitor audit logging
9. **Network Isolation**: Run Airflow in private subnet
10. **Least Privilege**: Use dedicated service accounts with minimal permissions

### Production Security Checklist

- [x] Update to latest stable 2.x version (2.11.1)
- [x] Disable example DAGs
- [ ] Enable authentication (OAuth2/OIDC)
- [ ] Configure HTTPS/TLS
- [ ] Set up secrets backend
- [ ] Enable audit logging
- [ ] Configure RBAC
- [ ] Restrict network access
- [ ] Regular security scanning
- [ ] Monitor for Airflow 3.2.0 release

### References

- [Apache Airflow Security](https://airflow.apache.org/docs/apache-airflow/stable/security/index.html)
- [Airflow 2.11.1 Release Notes](https://airflow.apache.org/docs/apache-airflow/stable/release_notes.html)
- [Airflow 3.0 Migration Guide](https://airflow.apache.org/docs/apache-airflow/stable/migrations-ref.html)
- [CVE Database](https://cve.mitre.org/)

---

**Current Status**:
- **Version**: 2.11.1 (latest stable 2.x)
- **Critical Vulnerability**: Mitigated by disabling example DAGs
- **Recommendation**: Monitor for Airflow 3.2.0 stable release and upgrade when available

**Note**: This is a demonstration system. For production use, implement all security recommendations and follow your organization's security policies.
