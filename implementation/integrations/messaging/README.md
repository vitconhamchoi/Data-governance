# Kafka/Messaging Integration

## Overview

Apache Kafka integration for event-driven architecture and data streaming.

## Features

- Producer and Consumer libraries
- Schema Registry integration (Avro/Protobuf)
- Dead Letter Queue handling
- Exactly-once/At-least-once semantics
- Topic management

## Topics

```
telemetry.raw          - Raw device telemetry
telemetry.normalized   - Normalized telemetry
telemetry.enriched     - Enriched with metadata
commands               - Device commands
notifications          - User notifications
audit.events           - Audit trail
data-quality.events    - Quality check results
lineage.events         - Lineage updates
```

## Configuration

```yaml
kafka:
  bootstrap_servers:
    - kafka-1:9092
    - kafka-2:9092
    - kafka-3:9092
  schema_registry_url: http://schema-registry:8081
  security_protocol: SASL_SSL
  sasl_mechanism: PLAIN
```

## Usage

```python
from integrations.messaging import KafkaProducer, KafkaConsumer

# Producer
producer = KafkaProducer(config)
producer.send("telemetry.raw", value=message, key=device_id)

# Consumer
consumer = KafkaConsumer(
    topics=["telemetry.normalized"],
    group_id="analytics-processor"
)

for message in consumer:
    process_message(message.value)
```
