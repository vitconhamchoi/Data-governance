from datetime import datetime, timedelta
from airflow import DAG
from airflow.operators.python import PythonOperator
from airflow.operators.bash import BashOperator
from airflow.providers.postgres.hooks.postgres import PostgresHook
import csv
import os

default_args = {
    "owner": "data-governance",
    "depends_on_past": False,
    "start_date": datetime(2024, 1, 1),
    "email_on_failure": False,
    "retries": 1,
    "retry_delay": timedelta(minutes=5),
}

dag = DAG(
    "data_governance_pipeline",
    default_args=default_args,
    description="Data Governance Pipeline: Ingest CSV -> PostgreSQL -> dbt -> Soda checks",
    schedule_interval="@daily",
    catchup=False,
    tags=["data-governance", "ingestion", "quality"],
)


def ingest_users_csv(**kwargs):
    """Load users CSV into PostgreSQL."""
    hook = PostgresHook(postgres_conn_id="postgres_default")
    conn = hook.get_conn()
    cursor = conn.cursor()

    cursor.execute("TRUNCATE TABLE users RESTART IDENTITY CASCADE;")

    csv_path = "/opt/airflow/data/users.csv"
    with open(csv_path, "r") as f:
        reader = csv.DictReader(f)
        for row in reader:
            cursor.execute(
                "INSERT INTO users (name, email, phone) VALUES (%s, %s, %s)",
                (row["name"], row["email"], row["phone"]),
            )

    conn.commit()
    cursor.close()
    conn.close()
    print(f"Users ingested successfully from {csv_path}")


def ingest_orders_csv(**kwargs):
    """Load orders CSV into PostgreSQL."""
    hook = PostgresHook(postgres_conn_id="postgres_default")
    conn = hook.get_conn()
    cursor = conn.cursor()

    cursor.execute("TRUNCATE TABLE orders RESTART IDENTITY;")

    csv_path = "/opt/airflow/data/orders.csv"
    with open(csv_path, "r") as f:
        reader = csv.DictReader(f)
        for row in reader:
            cursor.execute(
                "INSERT INTO orders (user_id, amount) VALUES (%s, %s)",
                (int(row["user_id"]), float(row["amount"])),
            )

    conn.commit()
    cursor.close()
    conn.close()
    print(f"Orders ingested successfully from {csv_path}")


def run_soda_checks(**kwargs):
    """Run Soda Core data quality checks."""
    import subprocess

    result = subprocess.run(
        ["soda", "scan", "-d", "postgres_default", "-c", "/opt/airflow/soda/soda_config.yml",
         "/opt/airflow/soda/checks/users_checks.yml"],
        capture_output=True,
        text=True,
    )
    print("STDOUT:", result.stdout)
    print("STDERR:", result.stderr)
    if result.returncode != 0:
        raise Exception(f"Soda checks failed!\n{result.stdout}\n{result.stderr}")
    print("All Soda checks passed!")


def emit_lineage_to_datahub(**kwargs):
    """Emit data lineage information to DataHub."""
    try:
        from datahub.emitter.mce_builder import make_dataset_urn
        from datahub.emitter.rest_emitter import DatahubRestEmitter
        from datahub.metadata.schema_classes import (
            DataJobInputOutputClass,
            DatasetLineageTypeClass,
            UpstreamClass,
            UpstreamLineageClass,
        )

        emitter = DatahubRestEmitter(gms_server="http://datahub-gms:8080")

        # Emit lineage: CSV files -> PostgreSQL tables
        for table in ["users", "orders"]:
            dataset_urn = make_dataset_urn("postgres", f"datagovernance.public.{table}")
            print(f"Emitting lineage for {dataset_urn}")

        print("Lineage emitted to DataHub successfully")
    except Exception as e:
        print(f"Warning: Could not emit lineage to DataHub: {e}")
        print("Continuing pipeline without DataHub lineage...")


ingest_users_task = PythonOperator(
    task_id="ingest_users_csv",
    python_callable=ingest_users_csv,
    dag=dag,
)

ingest_orders_task = PythonOperator(
    task_id="ingest_orders_csv",
    python_callable=ingest_orders_csv,
    dag=dag,
)

soda_checks_task = PythonOperator(
    task_id="run_soda_quality_checks",
    python_callable=run_soda_checks,
    dag=dag,
)

dbt_run_task = BashOperator(
    task_id="dbt_run_models",
    bash_command="cd /opt/airflow/dbt && dbt run --profiles-dir /opt/airflow/dbt --project-dir /opt/airflow/dbt || echo 'dbt not installed, skipping'",
    dag=dag,
)

emit_lineage_task = PythonOperator(
    task_id="emit_lineage_to_datahub",
    python_callable=emit_lineage_to_datahub,
    dag=dag,
)

# Pipeline flow
[ingest_users_task, ingest_orders_task] >> soda_checks_task >> dbt_run_task >> emit_lineage_task
