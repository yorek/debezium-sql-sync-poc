#!/bin/bash

# Strict mode, fail on any error
set -euo pipefail

echo "registering connector"
curl -i -X POST \
    -H "Accept:application/json" -H  "Content-Type:application/json" \
    http://localhost:8083/connectors/ \
    -d @../register-sqlserver-tpc1gb-eh.json
