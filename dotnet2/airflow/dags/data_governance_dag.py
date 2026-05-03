from datetime import datetime, timedelta
from airflow import DAG
from airflow.operators.python import PythonOperator
from airflow.operators.bash import BashOperator
import json

default_args = {
    'owner': 'airflow',
    'depends_on_past': False,
    'start_date': datetime(2024, 1, 1),
    'email_on_failure': False,
    'email_on_retry': False,
    'retries': 1,
    'retry_delay': timedelta(minutes=5),
}

def run_data_quality_checks(**context):
    """Run Soda Core data quality checks"""
    import psycopg2

    # Connect to PostgreSQL
    conn = psycopg2.connect(
        host='postgres',
        database='datagovernance',
        user='datauser',
        password='datapass123'
    )
    cursor = conn.cursor()

    checks_passed = True
    results = []

    # Check 1: Users table row count > 0
    cursor.execute("SELECT COUNT(*) FROM users")
    user_count = cursor.fetchone()[0]
    check1 = {
        'table': 'users',
        'check': 'row_count > 0',
        'status': 'PASS' if user_count > 0 else 'FAIL',
        'details': f'Row count: {user_count}'
    }
    results.append(check1)
    if check1['status'] == 'FAIL':
        checks_passed = False

    # Check 2: Users table - no null emails
    cursor.execute("SELECT COUNT(*) FROM users WHERE email IS NULL")
    null_emails = cursor.fetchone()[0]
    check2 = {
        'table': 'users',
        'check': 'email NOT NULL',
        'status': 'PASS' if null_emails == 0 else 'FAIL',
        'details': f'Null emails: {null_emails}'
    }
    results.append(check2)
    if check2['status'] == 'FAIL':
        checks_passed = False

    # Check 3: Orders table row count > 0
    cursor.execute("SELECT COUNT(*) FROM orders")
    order_count = cursor.fetchone()[0]
    check3 = {
        'table': 'orders',
        'check': 'row_count > 0',
        'status': 'PASS' if order_count > 0 else 'FAIL',
        'details': f'Row count: {order_count}'
    }
    results.append(check3)
    if check3['status'] == 'FAIL':
        checks_passed = False

    # Check 4: Orders table - no null user_id
    cursor.execute("SELECT COUNT(*) FROM orders WHERE user_id IS NULL")
    null_user_ids = cursor.fetchone()[0]
    check4 = {
        'table': 'orders',
        'check': 'user_id NOT NULL',
        'status': 'PASS' if null_user_ids == 0 else 'FAIL',
        'details': f'Null user_ids: {null_user_ids}'
    }
    results.append(check4)
    if check4['status'] == 'FAIL':
        checks_passed = False

    # Store results
    for result in results:
        cursor.execute(
            "INSERT INTO data_quality_checks (table_name, check_name, status, result_details) VALUES (%s, %s, %s, %s)",
            (result['table'], result['check'], result['status'], result['details'])
        )

    conn.commit()
    cursor.close()
    conn.close()

    if not checks_passed:
        raise Exception("Data quality checks failed!")

    return json.dumps(results)


def emit_lineage_to_datahub(**context):
    """Emit lineage information to DataHub"""
    import requests

    # This would normally use DataHub's Python SDK
    # For now, we'll just log the lineage information
    lineage_info = {
        'pipeline': 'data_governance_pipeline',
        'source': 'CSV -> PostgreSQL',
        'transformation': 'dbt models',
        'destination': 'users, orders tables'
    }

    print(f"Lineage Info: {json.dumps(lineage_info, indent=2)}")

    # In production, you would emit to DataHub like this:
    # from datahub.emitter.mce_builder import make_data_job_urn
    # from datahub.emitter.rest_emitter import DatahubRestEmitter
    # emitter = DatahubRestEmitter('http://datahub-gms:8080')
    # ... emit lineage

    return lineage_info


with DAG(
    'data_governance_pipeline',
    default_args=default_args,
    description='Data Governance Pipeline with Quality Checks and Lineage',
    schedule_interval=timedelta(hours=1),
    catchup=False,
    tags=['governance', 'quality', 'lineage'],
) as dag:

    # Task 1: Run dbt models (would normally use dbt operator)
    run_dbt = BashOperator(
        task_id='run_dbt_models',
        bash_command='echo "Running dbt models..." && echo "dbt run --project-dir /opt/airflow/dbt"',
    )

    # Task 2: Run data quality checks
    quality_checks = PythonOperator(
        task_id='run_quality_checks',
        python_callable=run_data_quality_checks,
        provide_context=True,
    )

    # Task 3: Emit lineage to DataHub
    emit_lineage = PythonOperator(
        task_id='emit_lineage_to_datahub',
        python_callable=emit_lineage_to_datahub,
        provide_context=True,
    )

    # Define task dependencies
    run_dbt >> quality_checks >> emit_lineage
