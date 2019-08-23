#!/bin/bash

# Strict mode, fail on any error
set -euo pipefail

echo "unregistering connector"
curl -i -X DELETE http://localhost:8083/connectors/tpch  