#!/bin/bash

# Strict mode, fail on any error
set -euo pipefail

export DEBEZIUM_VERSION=0.10

echo "stopping debezium"
docker-compose -f docker-compose-sqlserver-eh.yaml down       