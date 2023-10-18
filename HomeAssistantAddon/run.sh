#!/usr/bin/with-contenv bashio
# shellcheck shell=bash

ACTIVITYLEVELS_REFRESH_INTERVAL=$(bashio::config 'refresh_interval' '')
MQTT_PORT=$(bashio::config 'mqtt_port' '1883')
MQTT_USE_TLS=$(bashio::config 'mqtt_use_tls' 'false')
MQTT_IGNORE_CERT_VALIDATION_ERRORS=$(bashio::config 'mqtt_ignore_cert_errors' 'false')
MQTT_CLIENT_ID=$(bashio::config 'mqtt_client_id' '')
MQTT_TOPIC_ROOT=$(bashio::config 'mqtt_topic_root' 'activitylevels')
HA_MQTT_DISCOVERY_PREFIX=$(bashio::config 'mqtt_discovery_prefix' 'homeassistant')

if bashio::services.available "mqtt"; then
    bashio::log.info "MQTT service found, fetching credentials ..."
    MQTT_HOST=$(bashio::services mqtt "host")
    MQTT_USERNAME=$(bashio::services mqtt "username")
    MQTT_PASSWORD=$(bashio::services mqtt "password")
else
    bashio::log.error "No internal MQTT service found"
    MQTT_HOST=$(bashio::config 'mqtt_host' '')
    MQTT_USERNAME=$(bashio::config 'mqtt_username' '')
    MQTT_PASSWORD=$(bashio::config 'mqtt_password' '')
fi

HA_CLIENT_AUTH_TOKEN="${SUPERVISOR_TOKEN:?Supervisor token is required}"
HA_CLIENT_REST_URL_PREFIX="http://supervisor/core/api"
HA_CLIENT_WS_URL="ws://supervisor/core/websocket"

if [ -z "${MQTT_CLIENT_ID}" ] || [ "${MQTT_CLIENT_ID}" == 'null' ]; then
  MQTT_CLIENT_ID="activitylevels_$(date +%Y%m%d%H%M)"
fi

bashio::log.info "ACTIVITYLEVELS_REFRESH_INTERVAL........ $ACTIVITYLEVELS_REFRESH_INTERVAL"
bashio::log.info "MQTT_HOST.............................. $MQTT_HOST"
bashio::log.info "MQTT_PORT.............................. $MQTT_PORT"
bashio::log.info "MQTT_USE_TLS........................... $MQTT_USE_TLS"
bashio::log.info "MQTT_IGNORE_CERT_VALIDATION_ERRORS..... $MQTT_IGNORE_CERT_VALIDATION_ERRORS"
bashio::log.info "MQTT_CLIENT_ID......................... $MQTT_CLIENT_ID"
bashio::log.info "MQTT_TOPIC_ROOT........................ $MQTT_TOPIC_ROOT"
bashio::log.info "MQTT_USERNAME.......................... $MQTT_USERNAME"
bashio::log.info "MQTT_PASSWORD.......................... $(echo "${MQTT_PASSWORD:-}" | sed -E "s/(.{1}).*(.?)/\1*****\2/")"
bashio::log.info "HA_MQTT_DISCOVERY_PREFIX............... $HA_MQTT_DISCOVERY_PREFIX"
bashio::log.info "HA_CLIENT_AUTH_TOKEN................... $(echo "${HA_CLIENT_AUTH_TOKEN:-}" | sed -E "s/(.{1}).*(.?)/\1*****\2/")"
bashio::log.info "HA_CLIENT_REST_URL_PREFIX.............. $HA_CLIENT_REST_URL_PREFIX"
bashio::log.info "HA_CLIENT_WS_URL....................... $HA_CLIENT_WS_URL"

if ! [ -f /data/config.yaml ]; then
  bashio::log.info "Creating initial configuration."
  cp /app/config.sample.yaml /data/config.yaml
fi

export ACTIVITYLEVELS_REFRESH_INTERVAL
export MQTT_HOST
export MQTT_PORT
export MQTT_USE_TLS
export MQTT_IGNORE_CERT_VALIDATION_ERRORS
export MQTT_CLIENT_ID
export MQTT_USERNAME
export MQTT_PASSWORD
export HA_MQTT_DISCOVERY_PREFIX
export HA_CLIENT_AUTH_TOKEN
export HA_CLIENT_REST_URL_PREFIX
export HA_CLIENT_WS_URL

dotnet /app/ActivityLevels.dll -c /data/config.yaml -s /data/state.json

# HA_MQTT_DISCOVERY_QOS
# HA_MQTT_DISCOVERY_RETAIN
# MQTT_AUTO_RECONNECT_DELAY
# MQTT_CLIENT_CERT_PRIVATE_KEY
# MQTT_CLIENT_CERT_PUBLIC_KEY
# MQTT_KEEP_ALIVE_INTERVAL
# MQTT_LWT_MSG_PAYLOAD
# MQTT_LWT_MSG_QOS
# MQTT_LWT_MSG_RETAIN
# MQTT_LWT_MSG_TOPIC
# MQTT_MAX_PENDING_MESSAGES
# MQTT_ONLINE_MSG_PAYLOAD
# MQTT_ONLINE_MSG_QOS
# MQTT_ONLINE_MSG_RETAIN
# MQTT_ONLINE_MSG_TOPIC
# MQTT_PENDING_MESSAGES_OVERFLOW_STRATEGY
# MQTT_PROTOCOL_VERSION
# MQTT_TIMEOUT
