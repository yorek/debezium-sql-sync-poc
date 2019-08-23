#!/bin/bash

# Strict mode, fail on any error
set -euo pipefail

export DEBEZIUM_VERSION=0.10

echo "deploying resource group"
az group create -n dbsync -l WestUS2

echo "deploying eventhubs namespace"
az eventhubs namespace create -g dbsync -n debezium --enable-kafka=true -l WestUS2

echo "deploying eventhubs changestream eventhub"
az eventhubs eventhub create -g dbsync -n debezium_changestream --namespace-name debezium --message-retention 7 --partition-count 1

echo "gathering eventhubs info"
export EH_NAME=`az eventhubs namespace list -g dbsync --query '[].name' -o tsv`
export EH_CONNECTION_STRING=`az eventhubs namespace authorization-rule keys list -g dbsync -n RootManageSharedAccessKey --namespace-name debezium -o tsv --query 'primaryConnectionString'`

echo "starting debezium"
docker-compose -f docker-compose-sqlserver-eh.yaml up

