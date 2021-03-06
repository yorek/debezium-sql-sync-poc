#!/bin/bash

# Strict mode, fail on any error
set -euo pipefail

echo "finding debezium ip"
export DEBEZIUM_IP=`az container show -g dbsync -n debezium -o tsv --query "ipAddress.ip"`

echo "debezium ip: ${DEBEZIUM_IP}"

echo "registering connector"
curl -i -X POST \
    -H "Accept:application/json" -H  "Content-Type:application/json" \
    http://${DEBEZIUM_IP}:8083/connectors/ \
    -d @../register-sqlserver-tpc1gb-eh.json
