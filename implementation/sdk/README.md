# SDK - Python Client Library

## Overview

Python SDK for interacting with the Data Governance platform API.

## Installation

```bash
pip install data-governance-sdk
```

## Quick Start

```python
from data_governance import Client

# Initialize client
client = Client(
    api_url="https://api.governance.example.com",
    api_key="your-api-key"
)

# Search datasets
datasets = client.datasets.search(query="users", zone="curated")

# Get dataset details
dataset = client.datasets.get("postgres.analytics.users")

# Get quality metrics
metrics = client.quality.get_metrics("postgres.analytics.users")

# Get lineage
lineage = client.lineage.get("postgres.analytics.users", depth=5)

# Trigger quality scan
scan_result = client.quality.trigger_scan("postgres.analytics.users")
```

## Features

- Full REST API coverage
- Type hints and validation
- Async support
- Retry logic
- Rate limiting
- Caching

## Directory Structure

```
sdk/
├── data_governance/
│   ├── __init__.py
│   ├── client.py
│   ├── datasets.py
│   ├── quality.py
│   ├── lineage.py
│   ├── governance.py
│   └── models/
├── tests/
├── examples/
├── setup.py
└── README.md
```
